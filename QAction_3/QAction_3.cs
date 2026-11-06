using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: copyAction.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			var index = protocol.RowKey();

			var initialSpeed = protocol.GetParameterIndexByKey(Parameter.Iftable.tablePid, index, 3);

			if (initialSpeed == null)
			{
                protocol.Log("ifSpeed is empty for row " + index, LogType.Information, LogLevel.NoLogging);
                return;
            }

			double result;

			if((double)initialSpeed < UInt32.MaxValue)
                result = (double)initialSpeed;
			else
			{
                if (!protocol.Exists(Parameter.Ifxtable.tablePid, index))
                {
                    protocol.Log("No matching ifXTable row for interface " + index, LogType.Information, LogLevel.NoLogging);
                    return;
                }

                object highSpeedValue = protocol.GetParameterIndexByKey(Parameter.Ifxtable.tablePid, index, 2);

                if (highSpeedValue == null)
                {
                    protocol.Log("ifHighSpeed is empty for row " + index, LogType.Information, LogLevel.NoLogging);
                    return;
                }

                result = (double)highSpeedValue;
            }
			
			protocol.SetParameterIndexByKey(Parameter.Iftable.tablePid, index, 5, result);

        }
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}