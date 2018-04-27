#region usings
using System;
using System.Globalization;

using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Dir", 
				Category = "File", 
				Version = "Async", 
				Help = "Basic template with one string in/out", 
				Tags = "",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class AsyncFileDirNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Mask", DefaultString = "*.*")]
		public IDiffSpread<string> FInputMask;

		[Input("Directory", DefaultString = "hello c#")]
		public IDiffSpread<string> FInput;

		[Input("Update")]
		public ISpread<bool> FInputUpdate;

		
		[Output("Filenames")]
		public ISpread<string> FOutput;

		[Output("Short Filenames")]
		public ISpread<string> FOutputShort;

		[Output("Prefix")]
		public ISpread<string> FOutputPrefix;

		[Output("Numeric Prefix")]
		public ISpread<double> FOutputNumericPrefix;
		
		[Output("Count")]
		public ISpread<int> FOutputCount;

		[Output("Updated")]
		public ISpread<bool> FOutputUpdated;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		private delegate string[] ArgDelegate(string _str); 	
		private NumberStyles styles;
		bool updated;
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			if (FInputUpdate[0] || FInputMask.IsChanged || FInput.IsChanged) 			
			{
				ArgDelegate sd = DirectoryListing;
				IAsyncResult asyncRes = sd.BeginInvoke(FInput[0],null,null);
			}
			
			FOutputUpdated.SliceCount =1;
			FOutputUpdated[0] = updated;
			updated = false;	
		}

		private  string[] DirectoryListing(string Dir)
		{
			var directory = new DirectoryInfo(Dir);
			bool isDirectory = directory.Exists;			

			string[] files = {""};
			styles = NumberStyles.Float | NumberStyles.AllowDecimalPoint;
			if (directory.Exists) 
			{
				files = Directory.GetFiles(Dir,FInputMask[0], SearchOption.AllDirectories).OrderBy(f => f).ToArray();	
				FOutput.SliceCount = files.Length;
				FOutputShort.SliceCount = files.Length;
				FOutputNumericPrefix.SliceCount = files.Length;
				FOutputCount.SliceCount = 1;
				for (int i = 0; i < files.Length; i++)
				{
					FOutput[i] = files[i];
					FOutputShort[i] = Path.GetFileName(files[i]);			
					string _prefix = FOutputShort[i].Split('_').First();
					FOutputPrefix[i] = _prefix;
					double numprefix = 0;
		            try
		            {
		            	
		             	numprefix = double.Parse(_prefix, styles, CultureInfo.InvariantCulture);
		            }
		            catch (FormatException e)
		            {
		                
		            }
					FOutputNumericPrefix[i] = numprefix;
				}				
				
				FOutputCount[0] = files.Length;
				updated = true;
			}
		
			return files;
		}
		
		
	}
		
	
}
