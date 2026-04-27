using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Scripting;
using SLParameter = Skyline.DataMiner.Scripting.Parameter;

/// <summary>
/// DataMiner QAction: Synchronizes values between two tables based on row keys.
/// Copies the "high speed" value from IfxTable to IfTable.
/// If the speed from IfTable equals uint.MaxValue, the value from IfxTable is used instead.
/// </summary>
public static class QAction
{
    private const double UIntMax = uint.MaxValue;
    private const double BpsToMbps = 1_000_000.0;

    /// <summary>
    /// Entry point of the QAction.
    /// Reads instance keys and speed values from both IfTable and IfxTable,
    /// then writes the correct interface speed (in Mbps) into the ifHighSpeed column of IfTable.
    /// </summary>
    /// <param name="protocol">SLProtocolExt instance used to access DataMiner parameters.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            if (!TryGetIfTableColumns(protocol, out Dictionary<string, double> ifSpeedTable) ||
                !TryGetIfxTableColumns(protocol, out Dictionary<string, double> xSpeedTable))
            {
                return;
            }

            var resultSpeeds = CalculateSpeeds(ifSpeedTable, xSpeedTable);

            SetColumnFromDictionary(protocol, SLParameter.Iftable.tablePid, SLParameter.Iftable.Pid.ifcalculatedspeed, resultSpeeds);
        }
        catch (Exception ex)
        {
            protocol.Log(
                $"QAction|Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}",
                LogType.Error,
                LogLevel.NoLogging);
        }
    }

    private static void SetColumnFromDictionary(
        SLProtocolExt protocol,
        int tablePid,
        int columnPid,
        Dictionary<string, double> data)
    {
        protocol.NotifyProtocol(
            (int)NotifyType.NT_FILL_ARRAY_WITH_COLUMN,
            new object[] { tablePid, columnPid, true },
            new object[] { data.Keys.ToArray(), data.Values.Cast<object>().ToArray() });
    }

    private static bool TryGetIfTableColumns(
        SLProtocolExt protocol,
        out Dictionary<string, double> ifSpeedTable)
    {
        ifSpeedTable = null;

        var result = protocol.NotifyProtocol(
            (int)NotifyType.NT_GET_TABLE_COLUMNS,
            SLParameter.Iftable.tablePid,
            new uint[]
            {
                SLParameter.Iftable.Idx.iftableindex,
                SLParameter.Iftable.Idx.iftablespeed,
            });

        if (!(result is object[] columns) || columns.Length < 2)
        {
            protocol.Log("QAction|IfTable columns are null or invalid.", LogType.Error, LogLevel.NoLogging);
            return false;
        }

        if (!(columns[0] is object[] keys) || !(columns[1] is object[] speeds))
        {
            protocol.Log("QAction|IfTable keys/speeds are null.", LogType.Error, LogLevel.NoLogging);
            return false;
        }

        ifSpeedTable = BuildLookup(keys, speeds);
        return true;
    }

    private static bool TryGetIfxTableColumns(
        SLProtocolExt protocol,
        out Dictionary<string, double> ifxSpeedTable)
    {
        ifxSpeedTable = null;

        var result = protocol.NotifyProtocol(
            (int)NotifyType.NT_GET_TABLE_COLUMNS,
            SLParameter.Ifxtable.tablePid,
            new uint[]
            {
                SLParameter.Ifxtable.Idx.ifxtableinstance,
                SLParameter.Ifxtable.Idx.ifxcalculatedspeed,
            });

        if (!(result is object[] columns) || columns.Length < 2)
        {
            protocol.Log("QAction|IfxTable columns are null or invalid.", LogType.Error, LogLevel.NoLogging);
            return false;
        }

        if (!(columns[0] is object[] keys) || !(columns[1] is object[] speeds))
        {
            protocol.Log("QAction|IfxTable keys/speeds are null.", LogType.Error, LogLevel.NoLogging);
            return false;
        }

        ifxSpeedTable = BuildLookup(keys, speeds);
        return true;
    }

    private static Dictionary<string, double> BuildLookup(object[] xKeys, object[] speeds)
    {
        var lookup = new Dictionary<string, double>(xKeys.Length, StringComparer.Ordinal);

        for (int i = 0; i < xKeys.Length; i++)
        {
            lookup[Convert.ToString(xKeys[i])] = Convert.ToDouble(speeds[i]);
        }

        return lookup;
    }

    private static Dictionary<string, double> CalculateSpeeds(
        Dictionary<string, double> ifSpeedLookup,
        Dictionary<string, double> xSpeedLookup)
    {
        Dictionary<string, double> resultKeys = new Dictionary<string, double>(ifSpeedLookup.Count);

        foreach (var kvp in ifSpeedLookup)
        {
            string key = kvp.Key;
            double ifSpeed = kvp.Value;

            resultKeys[key] = ifSpeed >= UIntMax
                ? GetExtendedSpeed(xSpeedLookup, key)
                : ifSpeed / BpsToMbps;
        }

        return resultKeys;
    }

    private static double GetExtendedSpeed(Dictionary<string, double> lookup, string key)
    {
        return lookup.TryGetValue(key, out double speed) ? speed : 0.0;
    }
}