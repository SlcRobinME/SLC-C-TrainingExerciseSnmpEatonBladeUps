using System;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Scripting;
using SLParameter = Skyline.DataMiner.Scripting.Parameter;

/// <summary>
/// DataMiner QAction: Synchronizes values between two tables based on row keys.
/// Copies the "high speed" value from IfxTable to IfTable.
/// </summary>
public static class QAction
{
    private const uint IfxInstanceColIdx = 0;
    private const uint IfxHighSpeedColIdx = 1;

    /// <summary>
    /// Entry point of the QAction.
    /// Reads instance keys and high-speed values from IfxTable,
    /// then writes them into the ifHighSpeed column of IfTable.
    /// </summary>
    /// <param name="protocol">SLProtocolExt instance used to access DataMiner parameters.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            object[] rawKeys = (object[])protocol.NotifyProtocol(
                (int)NotifyType.NT_GET_TABLE_COLUMNS,
                SLParameter.Ifxtable.tablePid,
                new uint[] { IfxInstanceColIdx });

            object[] rawValues = (object[])protocol.NotifyProtocol(
                (int)NotifyType.NT_GET_TABLE_COLUMNS,
                SLParameter.Ifxtable.tablePid,
                new uint[] { IfxHighSpeedColIdx });

            object[] xKeys = rawKeys?[0] as object[];
            object[] xValues = rawValues?[0] as object[];

            protocol.NotifyProtocol(
                (int)NotifyType.NT_FILL_ARRAY_WITH_COLUMN,
                new object[] { SLParameter.Iftable.tablePid, SLParameter.Iftable.Pid.ifhighspeed, true },
                new object[] { xKeys, xValues });
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction1|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}