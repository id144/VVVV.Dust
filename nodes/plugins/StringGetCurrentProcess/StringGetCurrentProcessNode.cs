#region usings
using System;
using System.Diagnostics;
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
	[PluginInfo(Name = "GetCurrentProcess", Category = "VVVV", Help = "Basic template with one string in/out", Tags = "Dust", Author ="id144")]
	#endregion PluginInfo
	public class StringGetCurrentProcessNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Update", IsSingle = true, DefaultValue = 0)]
		public IDiffSpread<bool> FInputUpdate;

		[Output("Process ID")]
		public ISpread<int> FOutputProcessID;

        [Output("Process Name")]
        public ISpread<string> FOutputProcessName;

        [Output("Filename")]
		public ISpread<string> FOutputFilename;

		[Output("Process Priority")]
		public ISpread<int> FOutputProcessPriority;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		private StringGetCurrentProcessNode()
		{
			//UpdateOutputs();
		}
		private void UpdateOutputs()
		{
			Process currentProcess = Process.GetCurrentProcess();

            FOutputProcessID.SliceCount = 1;
            FOutputProcessName.SliceCount = 1;
            FOutputFilename.SliceCount = 1;

            FOutputProcessID[0] = currentProcess.Id;
            FOutputProcessName[0] = currentProcess.ProcessName;
            FOutputFilename[0] = currentProcess.MainModule.FileName;			
		}
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			if (FInputUpdate[0]) {
				UpdateOutputs();
			}
		}
	}
}
