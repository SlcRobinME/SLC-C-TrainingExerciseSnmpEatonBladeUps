using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Scripting;
using System;
using System.Collections.Generic;
using SLParameter = Skyline.DataMiner.Scripting.Parameter;


/// <summary>
///  Calculate interface speed.
/// </summary>
public static class QAction
{
    private const double BpsPerMbps = 1000000.0;
    private const double UIntMax = uint.MaxValue;

    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocolExt protocol)
    {

        try
        {
            var (ifKeys, ifSpeeds) = GetIfTableData(protocol);

            if (ifKeys == null || ifSpeeds == null || ifKeys.Length == 0)
            {
                protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|IfTable returned no rows.",LogType.Error,LogLevel.NoLogging);
                return;
            }

            Dictionary<string, double> highSpeedLookup = BuildHighSpeedLookup(protocol);

            var (resultKeys,resultValues) = BuildResultArrays(protocol, ifKeys, ifSpeeds, highSpeedLookup);



            protocol.NotifyProtocol((int)NotifyType.NT_FILL_ARRAY_WITH_COLUMN,new object[] { SLParameter.Iftable.tablePid, SLParameter.Iftable.Pid.ifhighspeedcalculated, true },new object[] { resultKeys, resultValues });

        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
     private static (object[] keys, object[] speeds) GetIfTableData(SLProtocolExt protocol)
    {
        var columns = (object[])protocol.NotifyProtocol(
            (int)NotifyType.NT_GET_TABLE_COLUMNS,
            SLParameter.Iftable.tablePid,
            new uint[] { SLParameter.Iftable.Idx.iftableindex, SLParameter.Iftable.Idx.iftablespeed });

        return (columns?[0] as object[], columns?[1] as object[]);
    }

    private static (object[] keys, object[] speeds) GetIfxTableData(SLProtocolExt protocol)
    {
        var columns = (object[])protocol.NotifyProtocol(
            (int)NotifyType.NT_GET_TABLE_COLUMNS,
            SLParameter.Ifxtable.tablePid,
            new uint[] { SLParameter.Ifxtable.Idx.ifxtableinstance, SLParameter.Ifxtable.Idx.ifxifhighspeed_2002 });

        return (columns?[0] as object[], columns?[1] as object[]);
    }
    private static Dictionary<string, double> BuildHighSpeedLookup(SLProtocolExt protocol)
    {

        var (xKeys, xSpeeds) = GetIfxTableData(protocol);

        var lookup = new Dictionary<string, double>();

        if (xKeys == null || xSpeeds == null)
            return lookup;

        for (int i = 0; i < xKeys.Length; i++)
        {
            string key = Convert.ToString(xKeys[i]);
            double val = xSpeeds[i] != null ? Convert.ToDouble(xSpeeds[i]) : double.NaN;
            lookup[key] = val;
        }

        return lookup;
    }
    private static double ResolveSpeed(
        SLProtocolExt protocol,
        string key,
        double rawSpeed,
        Dictionary<string, double> highSpeedLookup)
    {
        if (double.IsNaN(rawSpeed))
            return 0.0;

        if (rawSpeed >= UIntMax)
        {
            if (highSpeedLookup.TryGetValue(key, out double highSpeed) && !double.IsNaN(highSpeed))
                return highSpeed;

            protocol.Log($"QA{protocol.QActionID}|ifHighSpeed not found for key '{key}'.",LogType.Allways,LogLevel.NoLogging);
            return 0.0;
        }

        return rawSpeed / BpsPerMbps;
    }
    private static (object[] resultKeys, object[] resultValues) BuildResultArrays(SLProtocolExt protocol, object[] ifKeys, object[] ifSpeeds, Dictionary<string, double> highSpeedLookup)
    {
        object[] resultKeys = new object[ifKeys.Length];
        object[] resultValues = new object[ifKeys.Length];

        for (int i = 0; i < ifKeys.Length; i++) { 
        
            string key = Convert.ToString(ifKeys[i]);
            double rawSpeed = ifSpeeds[i] != null ? Convert.ToDouble(ifSpeeds[i]) : double.NaN; 

            resultKeys[i] = key;
            resultValues[i] = ResolveSpeed(protocol,key,rawSpeed,highSpeedLookup);
        
        }
        return (resultKeys, resultValues);
    }

}
