#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SetProcessPriority", Category = "System", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class SystemSetProcessPriorityNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

//		[Output("Output")]
//		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		Process processById;
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			using (Process p = Process.GetCurrentProcess()) 			
			{	
	    		p.PriorityClass = ProcessPriorityClass.RealTime;  		
			}
	

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
