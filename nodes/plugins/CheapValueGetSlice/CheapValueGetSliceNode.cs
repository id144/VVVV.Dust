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
	[PluginInfo(Name = "GetSlice", Category = "String", Version = "Cheap", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class CheapValueGetSliceNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Input("Input", IsSingle = true, DefaultValue = 1.0)]
		public ISpread<int> FInputIndex;

		[Output("Output")]
		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = 1;			
			FOutput[0] = FInput[FInputIndex[0]];			
		}
	}
}
