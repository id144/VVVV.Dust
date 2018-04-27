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
	[PluginInfo(Name = "ComputerName", Category = "String", Help = "Basic template with one string in/out", Tags = "")]
	#endregion PluginInfo
	public class StringComputerNameNode : IPluginEvaluate
	{
		#region fields & pins


		[Output("Output")]
		public ISpread<string> FOutput;

		
		string  name;
		#endregion fields & pins
		public StringComputerNameNode() 
		{
						
			name = Environment.MachineName;
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
				FOutput.SliceCount = 1;
				FOutput[0] = name;
		}
	}
}
