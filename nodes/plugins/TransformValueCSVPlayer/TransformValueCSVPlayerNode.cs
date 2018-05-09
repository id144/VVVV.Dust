#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Linq;
using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Player", Category = "CSV", Version = "Transform", Help = "Basic template with one value in/out", Tags = "c#")]
	#endregion PluginInfo

	public class TransformValueCSVPlayerNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Filename", IsSingle = true, StringType = StringType.Filename)] 
		public IDiffSpread<string> FInputFileName;

		[Input("Time")]
		public ISpread<double> FInputTime;

		[Input("Reset", IsSingle = true, IsBang = true)]
		public ISpread<bool> FInputReset;

		[Output("Track Name")]
		public ISpread<string> FOutputTrackName;

		
		[Output("Time")]
		public ISpread<double> FOutput;

		
		[Output("Output Transfrom")]
		public ISpread<Matrix4x4> FOutputTransform;

        [Output("Start")]
        public ISpread<double> FOutputStart;

        [Output("End")]
		public ISpread<double> FOutputEnd;

		
		[Import()]
		public ILogger FLogger; 
		
		#endregion fields & pins
	
		public class keyFrame
		{
		   // public string name { get; set; }
		    public double timeCode { get; set; }
		    public Matrix4x4 Transfrom{ get; set; }
		    //Other properties, methods, events... 
		}

		
		private delegate void ArgDelegate(string _str); 	
		private delegate void ArgDelegateDouble(double _time); 			
		private bool _update { get; set; }  
		
		private List<keyFrame> keyFramesList = new List<keyFrame>();
		
		public class keyFrames
		{
			public string name { get; set; }
			public List<keyFrame> frame{ get; set; }		    
		}
		public List<keyFrames> keyFramesList2 = new List<keyFrames>();
		public TransformValueCSVPlayerNode()
		{
			_update = true;
			//StremReader does not work called from this method
		}

        private void ParseFile(string filename)
        {
            try
            {
                using (StreamReader sr = new StreamReader(FInputFileName[0]))
                {
                    //int i = 0;
                    while (sr.Peek() >= 0)
                    {
                        List<string> names = sr.ReadLine().Split(';').ToList<string>();
                        if (names.Count > 17)
                        {
                            keyFrame _keyFrame = new keyFrame();
                            Matrix4x4 _matrix = new Matrix4x4();


                            int recordingIndex = -1;
                            for (int i = 0; i < keyFramesList2.Count; i++)
                            {
                                if (keyFramesList2[i].name == names[0])
                                {
                                    recordingIndex = i;
                                }
                            }

                            //name not found, add new recordingtrack
                            if (recordingIndex == -1)
                            {
                                recordingIndex = keyFramesList2.Count;
                                keyFramesList2.Add(new keyFrames());
                                keyFramesList2[recordingIndex].name = names[0];
                                keyFramesList2[recordingIndex].frame = new List<keyFrame>();
                            }


                            //_keyFrame.name =  names[0];
                            _keyFrame.timeCode = double.Parse(names[1], System.Globalization.CultureInfo.InvariantCulture);
                            for (int j = 0; j < 16; j++)
                            {
                                _matrix[j] = double.Parse(names[j + 2], System.Globalization.CultureInfo.InvariantCulture);
                            }
                            _keyFrame.Transfrom = _matrix;
                            keyFramesList.Add(_keyFrame);

                            keyFramesList2[recordingIndex].frame.Add(_keyFrame);
                        }

                    }
                    FOutputEnd.SliceCount = keyFramesList2.Count;
                    FOutputStart.SliceCount = keyFramesList2.Count;                	
                    for (int j = 0; j < keyFramesList2.Count; j++)
                    {
                     //   FOutputEnd[j] = keyFramesList[(keyFramesList.Count - 1)].timeCode;
                     //   FOutputStart[j] = keyFramesList[0].timeCode;
                    	FOutputEnd[j] = keyFramesList2[j].frame[keyFramesList2[j].frame.Count-1].timeCode;
                    	FOutputStart[j] = keyFramesList2[j].frame[0].timeCode;
                    }


                }
            }
            catch (IOException ioex)
            {

            }
        }
        //private void FindKeyframe(double Time) 
        private void FindKeyframe(ISpread<double> Time)
        {
            for (int offset = 0; offset < Time.SliceCount; offset++)
            {
                FOutput.SliceCount = keyFramesList2.Count * Time.SliceCount;

                FOutputTransform.SliceCount = keyFramesList2.Count * Time.SliceCount;

                FOutputTrackName.SliceCount = keyFramesList2.Count * Time.SliceCount;
                for (int j = 0; j < keyFramesList2.Count; j++)
                {
                    FOutputTrackName[j] = keyFramesList2[j].name;
                }
                for (int j = 0; j < keyFramesList2.Count; j++)
                {
                    double _delta = double.MaxValue;
                    double _deltaOld = double.MaxValue;

                    for (int i = 0; i < keyFramesList2[j].frame.Count; i++)
                    {
                        _delta = Math.Abs(keyFramesList2[j].frame[i].timeCode - Time[offset]);
                        if (_delta > _deltaOld)
                        {
                            FOutput[j + offset] = keyFramesList2[j].frame[Math.Max((i - 1), 0)].timeCode;
                            FOutputTransform[j + offset] = keyFramesList2[j].frame[Math.Max((i - 1), 0)].Transfrom;
                            break;
                        };
                        _deltaOld = _delta;
                    }

                }
            }
		}
		public void Evaluate(int SpreadMax)
		{
			//FOutput.SliceCount = SpreadMax;	
			if ((FInputFileName.IsChanged) || (FInputReset[0]))
			{
				_update = true;
			}
			
			if (_update) {				
				_update = false;
                //ArgDelegate sd = ParseFile;
                //IAsyncResult asyncRes = sd.BeginInvoke(FInputFileName[0],null,null);
                keyFramesList2 = new List<keyFrames>();
                if (FInputFileName.SliceCount > 0)
                {
                    Task.Run(() => ParseFile(FInputFileName[0]));
                }
            }			
			if (keyFramesList.Count > 1) 
			{
                if (FInputTime.SliceCount > 0)
                {
                    Task.Run(() => FindKeyframe(FInputTime));
                }
				//ArgDelegateDouble sd = FindKeyframe;
				//IAsyncResult asyncRes = sd.BeginInvoke(FInputTime[0],null,null);
			}
		}
	}
}
