#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ClosestValueIndex", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueClosestValueIndexNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		[Input("Find value", DefaultValue = 1.0)]
		public ISpread<double> FInputFind;
		[Output("Output")]
		public ISpread<double> FOutput;
		[Output("Delta")]
		public ISpread<double> FOutputDelta;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = 1;
			double delta = 1000000;
			double oldDelta = 1000000;
			double  founddelta = 1000000;
			int foundindex = 0;
			for (int i = 0; i < SpreadMax; i++) 
			{
				
				delta = Math.Abs(FInput[i] -  FInputFind[0]);
				if (delta < oldDelta) 
				{
					foundindex = i;
					founddelta = delta;
				}
				oldDelta = delta;
			}
			FOutput[0] = foundindex;
			FOutputDelta[0] = founddelta;
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
