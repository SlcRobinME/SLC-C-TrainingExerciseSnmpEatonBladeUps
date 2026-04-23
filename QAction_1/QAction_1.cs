using System;
using SLParameter = Skyline.DataMiner.Scripting.Parameter;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            double ticks = Convert.ToDouble(protocol.GetParameter(SLParameter.sysuptime));
            long totalSeconds = (long)(ticks / 100.0);

            protocol.SetParameter(SLParameter.systemuptime, (double)totalSeconds);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}