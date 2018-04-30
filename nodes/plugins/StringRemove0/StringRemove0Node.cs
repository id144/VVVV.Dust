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
	[PluginInfo(Name = "Remove0", Category = "String", Help = "Basic template with one string in/out", Tags = "c#")]
	#endregion PluginInfo
	public class StringRemove0Node : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultString = "hello c#")]
		public ISpread<string> FInput;

		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++) 
			{
				/*for (int j = FInput[i].Length; j>=0; j--)
				{
					
					FOutput[i] = FInput[i].Replace("c#", "vvvv");
				} */
				FOutput[i] = string.Format("{0:G29}", decimal.Parse(FInput[i],System.Globalization.CultureInfo.GetCultureInfo("en-US")));
			}
			//FLogger.Log(LogType.Debug, "Logging to Renderer (TTY)");
		}
	}
}
