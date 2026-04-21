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

            if (keys == null || keys.Length == 0)
                return;

            var tableIds = new int[keys.Length];
            var rowKeys = new string[keys.Length];
            var colIdxs = new int[keys.Length];
            var values = new object[keys.Length];

            for (int i = 0; i < keys.Length; i++)
            {
                double highSpeed = Convert.ToDouble(protocol.GetParameterIndexByKey(Parameter.Ifxtable.tablePid, keys[i], HighSpeedCol));

                tableIds[i] = Parameter.Iftable.tablePid;
                rowKeys[i] = keys[i];
                colIdxs[i] = TargetSpeedCol;
                values[i] = highSpeed;
            }

            protocol.SetParametersIndexByKey(tableIds, rowKeys, colIdxs, values);
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction1|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}