using Skyline.DataMiner.Scripting;
using System;
using SLParameter = Skyline.DataMiner.Scripting.Parameter;
using Skyline.DataMiner.Net.Messages;


/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
    private const double IfSpeedMaxValue = 4_294_967_295.0;
    private static string FormatSpeed(double speedMbps)
    {
        if (speedMbps >= 1000)
            return (speedMbps / 1000.0) + " Gbps";
        else
            return speedMbps + " Mbps";
    }
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocolExt protocol)
    {

        try
        {
            object[] ifColumns = (object[])protocol.NotifyProtocol(
                (int)NotifyType.NT_GET_TABLE_COLUMNS,
                SLParameter.Iftable.tablePid,
                new uint[]
                {
                    SLParameter.Iftable.Idx.iftableindex,
                    SLParameter.Iftable.Idx.iftablespeed,
                });

            object[] ifKeys = ifColumns?[0] as object[];
            object[] ifSpeeds = ifColumns?[1] as object[];

            if (ifKeys == null || ifSpeeds == null || ifKeys.Length == 0)
            {
                protocol.Log(string.Format("QA{0}|ifTable returned no rows.", protocol.QActionID), LogType.Allways, LogLevel.NoLogging);
                return;
            }

            object[] ifxColumns = (object[])protocol.NotifyProtocol(
                (int)NotifyType.NT_GET_TABLE_COLUMNS,
                SLParameter.Ifxtable.tablePid,
                new uint[]
                {
                                SLParameter.Ifxtable.Idx.ifxtableinstance,
                                SLParameter.Ifxtable.Idx.ifxifhighspeed_2002,
                });

            object[] xKeys = ifxColumns?[0] as object[];
            object[] xValues = ifxColumns?[1] as object[];


            var highSpeedLookup = new System.Collections.Generic.Dictionary<string, double>(StringComparer.Ordinal);
            if (xKeys != null && xValues != null)
            {
                for (int i = 0; i < xKeys.Length; i++)
                {
                    string key = Convert.ToString(xKeys[i]);
                    double val = xValues[i] != null ? Convert.ToDouble(xValues[i]) : double.NaN;
                    highSpeedLookup[key] = val;
                }
            }


            object[] calculatedSpeeds = new object[ifKeys.Length];

            for (int i = 0; i < ifKeys.Length; i++)
            {
                string key = Convert.ToString(ifKeys[i]);
                double rawSpeed = ifSpeeds[i] != null ? Convert.ToDouble(ifSpeeds[i]) : double.NaN;

                if (double.IsNaN(rawSpeed))
                {
                    calculatedSpeeds[i] = "N/A";
                }
                else if (rawSpeed >= IfSpeedMaxValue)
                {
                    if (highSpeedLookup.TryGetValue(key, out double highSpeed))
                    {
                        calculatedSpeeds[i] = FormatSpeed(highSpeed);
                    }
                    else
                    {
                        calculatedSpeeds[i] = "N/A";
                        protocol.Log(
                            string.Format("QA{0}|ifHighSpeed not found for key '{1}'.", protocol.QActionID, key),
                            LogType.Allways,
                            LogLevel.NoLogging);
                    }
                }
                else
                {
                    double speedMbps = rawSpeed / 1_000_000.0;
                    calculatedSpeeds[i] = FormatSpeed(speedMbps);
                }
            }

            protocol.NotifyProtocol(
               (int)NotifyType.NT_FILL_ARRAY_WITH_COLUMN,
               new object[] { SLParameter.Iftable.tablePid, SLParameter.Iftable.Pid.ifhighspeedcalculated, true },
               new object[] { ifKeys, calculatedSpeeds });

        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
