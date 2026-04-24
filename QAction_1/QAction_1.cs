using System;
using SLParameter = Skyline.DataMiner.Scripting.Parameter;
using Skyline.DataMiner.Scripting;

/// <summary>
/// Format System Up Time
/// </summary>
public static class QAction
{
    /// <summary>
    /// Format System Up Time.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    /// 
    private const double TimeTicksPerSecond = 100.0;
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            object rawTicks = protocol.GetParameter(SLParameter.sysuptime);

            if (rawTicks == null) {
                protocol.Log($"QA{protocol.QActionID}|sysUpTime returned null.", LogType.Error, LogLevel.NoLogging);
                return;
            }
            double ticks = Convert.ToDouble(rawTicks);
            long totalSeconds = (long)(ticks / TimeTicksPerSecond);

            protocol.SetParameter(SLParameter.systemuptime, (double)totalSeconds);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}