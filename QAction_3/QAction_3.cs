using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;

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
			var ifTableColumns = protocol.GetColumns(Parameter.Iftable.tablePid, new uint[] { Parameter.Iftable.indexColumn, Parameter.Iftable.Idx.iftablespeed_1003});

			if (ifTableColumns == null || ifTableColumns.Length != 2)
            {
                return;
            }

			var ifKeys = ifTableColumns[0] as object[];
			var ifSpeeds = ifTableColumns[1] as object[];

            if (ifKeys == null || ifSpeeds == null || ifKeys.Length == 0 || ifKeys.Length != ifSpeeds.Length)
                return;
            	
            var highSpeeds = new Dictionary<string, object>();
			var ifxTableColumns = protocol.GetColumns(Parameter.Ifxtable.tablePid, new uint[] { Parameter.Ifxtable.indexColumn, Parameter.Ifxtable.Idx.ifxifhighspeed_2002});

			if (ifxTableColumns != null && ifxTableColumns.Length == 2)
            {
                var ifxKeys = ifxTableColumns[0] as object[];
                var ifxHighSpeeds = ifxTableColumns[1] as object[];

                if (ifxKeys != null && ifxHighSpeeds != null && ifxKeys.Length == ifxHighSpeeds.Length)
                {
                    for (int i = 0; i < ifxKeys.Length; i++)
                    {
                        var key = Convert.ToString(ifxKeys[i]);
                        highSpeeds[key] = ifxHighSpeeds[i];
                    }
                }
            }

			var rowKeys = new List<object>(ifKeys.Length);
			var outValues = new List<object>(ifKeys.Length);

			for(int i = 0; i < ifKeys.Length; i++)
			{
				var key = ifKeys[i];
				rowKeys.Add(key);

				var speed = Convert.ToDouble(ifSpeeds[i]);
				object value = null;

				double result;
				if (speed < UInt32.MaxValue)
                {
                    value = speed;
                }
				else
				{
                    object highSpeed;
                    if (highSpeeds.TryGetValue((string)key, out highSpeed) && highSpeed != null)
                    {
                        value = Convert.ToDouble(highSpeed);
                    }
                }
                
				outValues.Add(value);
            }

			protocol.SetColumns(
				new int[] { Parameter.Iftable.tablePid, Parameter.Iftable.Pid.ifspeed_1005 },
				new IEnumerable<object>[] { rowKeys, outValues }
			);
        }
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}