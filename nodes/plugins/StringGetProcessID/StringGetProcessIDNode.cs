#region usings
using System;
using System.ComponentModel.Composition;

using System.Diagnostics;
using System.ComponentModel;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "GetProcessID", Category = "String", Help = "Basic template with one string in/out", Tags = "")]
	#endregion PluginInfo
	public class StringGetProcessIDNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultString = "hello c#")]
		public ISpread<string> FInput;

		[Output("Output")]
		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			Process[] localByName = Process.GetProcessesByName(FInput[0]);
			int i = 0;
			foreach (Process process in localByName)
        	{
        		FOutput.SliceCount = i+1;
        		FOutput[i] = process.Id;
        		i++;
        	}
		}
	}
}
