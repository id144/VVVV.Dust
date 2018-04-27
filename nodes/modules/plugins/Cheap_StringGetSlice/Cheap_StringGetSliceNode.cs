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
	[PluginInfo(Name = "GetSlice", Category = "String", Version = "Cheap_", Help = "Basic template with one string in/out", Tags = "")]
	#endregion PluginInfo
	public class Cheap_StringGetSliceNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultString = "hello c#")]
		public ISpread<string> FInput;

		[Input("Index", IsSingle = true, DefaultValue = 1.0)]
		public ISpread<int> FInputIndex;
		
		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			FOutput.SliceCount = 1;			
			if (FInput.SliceCount>0) 			
			{
				FOutput[0] = FInput[FInputIndex[0]];					
			}
		}
	}
}
