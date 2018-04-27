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
	[PluginInfo(Name = "AvoidNIL", Category = "Value", Version = "Cheap", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class CheapValueAvoidNILNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Input("Default", DefaultValue = 1.0)]
		public ISpread<double> FInputDefault;
		
		[Output("Output")]
		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{


			if (FInput.SliceCount < 1) {
				FOutput.SliceCount = 1;
				FOutput[0] = FInputDefault[0];
			} else {
				FOutput.SliceCount = FInput.SliceCount;
				for (int i = 0; i < SpreadMax; i++)
					FOutput[i] = FInput[i];

			}


		}
	}
}
