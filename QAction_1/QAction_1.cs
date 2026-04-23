using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Scripting;
using System;
using System.Collections.Generic;
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
            object[] ifKeys, ifSpeeds, xKeys, xSpeeds;

            if (!TryGetIfTableColumns(protocol, out ifKeys, out ifSpeeds) ||
                !TryGetIfxTableColumns(protocol, out xKeys, out xSpeeds))
            {
                return;
            }

            var xSpeedLookup = BuildXSpeedLookup(xKeys, xSpeeds);

            var (resultKeys, resultValues) = CalculateSpeeds(ifKeys, ifSpeeds, xSpeedLookup);

            protocol.NotifyProtocol(
                (int)NotifyType.NT_FILL_ARRAY_WITH_COLUMN,
                new object[] { SLParameter.Iftable.tablePid, SLParameter.Iftable.Pid.ifhighspeed, true },
                new object[] { resultKeys, resultValues });
        }
        catch (Exception ex)
        {
            protocol.Log(
                $"QAction|Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}",
                LogType.Error,
                LogLevel.NoLogging);
        }
    }

    private static bool TryGetIfTableColumns(SLProtocolExt protocol, out object[] keys, out object[] speeds)
    {
        var columns = (object[])protocol.NotifyProtocol(
            (int)NotifyType.NT_GET_TABLE_COLUMNS,
            SLParameter.Iftable.tablePid,
            new uint[]
            {
                SLParameter.Iftable.Idx.iftableindex,
                SLParameter.Iftable.Idx.iftablespeed,
            });

        keys = columns?[0] as object[];
        speeds = columns?[1] as object[];

        if (keys != null && speeds != null)
            return true;

        protocol.Log("QAction|IfTable columns are null, skipping.", LogType.Error, LogLevel.NoLogging);
        return false;
    }

    private static bool TryGetIfxTableColumns(SLProtocolExt protocol, out object[] keys, out object[] speeds)
    {
        var columns = (object[])protocol.NotifyProtocol(
            (int)NotifyType.NT_GET_TABLE_COLUMNS,
            SLParameter.Ifxtable.tablePid,
            new uint[]
            {
                SLParameter.Ifxtable.Idx.ifxtableinstance,
                SLParameter.Ifxtable.Idx.ifxifhighspeed,
            });

        keys = columns?[0] as object[];
        speeds = columns?[1] as object[];

        if (keys != null && speeds != null)
            return true;

        protocol.Log("QAction|IfxTable columns are null, skipping.", LogType.Error, LogLevel.NoLogging);
        return false;
    }

    private static Dictionary<string, double> BuildXSpeedLookup(object[] xKeys, object[] xSpeeds)
    {
        var lookup = new Dictionary<string, double>(xKeys.Length, StringComparer.Ordinal);

        for (int i = 0; i < xKeys.Length; i++)
        {
            lookup[Convert.ToString(xKeys[i])] = Convert.ToDouble(xSpeeds[i]);
        }

        return lookup;
    }

    private static (object[] keys, object[] values) CalculateSpeeds(
        object[] ifKeys,
        object[] ifSpeeds,
        Dictionary<string, double> xSpeedLookup)
    {
        var resultKeys = new object[ifKeys.Length];
        var resultValues = new object[ifKeys.Length];

        for (int i = 0; i < ifKeys.Length; i++)
        {
            string key = Convert.ToString(ifKeys[i]);
            double ifSpeed = Convert.ToDouble(ifSpeeds[i]);

            resultKeys[i] = key;
            resultValues[i] = ifSpeed >= UIntMax
                ? GetExtendedSpeed(xSpeedLookup, key)
                : ifSpeed / BpsToMbps;
        }

        return (resultKeys, resultValues);
    }

    private static double GetExtendedSpeed(Dictionary<string, double> lookup, string key)
    {
        return lookup.TryGetValue(key, out double speed) ? speed : 0.0;
    }
}