using System;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction: Synchronizes values between two tables based on row keys.
/// Copies the "high speed" value from IfxTable to IfTable.
/// </summary>
public static class QAction
{
    private const int HighSpeedCol = 2;
    private const int TargetSpeedCol = 5;

    /// <summary>
    /// Entry point of the QAction.
    /// Iterates over all keys in IfTable, retrieves corresponding values
    /// from IfxTable, and updates column 5 in IfTable.
    /// </summary>
    /// <param name="protocol">SLProtocolExt instance used to access DataMiner parameters.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            var keys = protocol.GetKeys(Parameter.Iftable.tablePid);
            foreach (var key in keys)
            {
                double highSpeed = Convert.ToDouble(protocol.GetParameterIndexByKey(Parameter.Ifxtable.tablePid, key, HighSpeedCol));
                protocol.SetParameterIndexByKey(Parameter.Iftable.tablePid, key, TargetSpeedCol, highSpeed);
            }
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction1|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}