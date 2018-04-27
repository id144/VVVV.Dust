#region usings
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using Microsoft.Kinect.Tools; 

#endregion usings

namespace VVVV.Nodes  
{
	#region PluginInfo
	[PluginInfo(Name = "Playback",
				Category = "Kinect2 Tools", 
				Version = "Microsoft", 
				Author = "id144",
				Help = "Create recording of sensor stream using Kinect Tools", 
				Tags = "")]
	#endregion PluginInfo
	
	public class Kinect2ToolsPlaybackNode : IPluginEvaluate
	{
		#region fields & pins 
		[Input("Play", DefaultValue = 0.0, IsToggle=true)]
		public IDiffSpread<bool> FInputPlay;

        [Input("Pause", IsSingle = true, IsToggle = true, DefaultValue = 0.0)]
        public IDiffSpread<bool> FInputPause;

        [Input("StepOnce", IsSingle = true, IsBang = true, DefaultValue = 0.0)]
        public IDiffSpread<bool> FInputStep;


		[Input("Do Seek", IsSingle = true, IsBang = true, DefaultValue = 0.0)]
        public IDiffSpread<bool> FInputDoSeek;

        [Input("Seek Time", IsSingle = true,  DefaultValue = 0.0)]
        public IDiffSpread<double> FInputSeek;

        [Input("Filename", IsSingle = true, StringType = StringType.Filename, DefaultString = ".xef")]
        public ISpread<string> FInputFilename;
		
		[Output("Status")]
		public ISpread<string> FRecord;

        [Output("Time")]
        public ISpread<double> FRecordDuration;

        [Output("Duration", IsSingle = true)]
        public ISpread<double> FRecordTotalDuration;

        [Import()]
		public ILogger FLogger;		 
		#endregion fields & pins
		
        private delegate void OneArgDelegate(string arg);      
		private bool doPlay;
		private KStudioPlayback playback = null;

		
		public Kinect2ToolsPlaybackNode()
		{

		}
		public void Evaluate(int SpreadMax)
		{          
            if (FInputPlay.IsChanged)
            {
                if (FInputPlay[0])
                {
                    string xefFilePath = @FInputFilename[0];

                    if (!string.IsNullOrEmpty(xefFilePath))
                    {		
                    	doPlay = true;
                        OneArgDelegate recording = new OneArgDelegate(this.PlayClip);
			            recording.BeginInvoke(xefFilePath, null, null);
                    }                	
                }
            	else
            	{
                    if (playback != null) playback.Stop();
            	}
            }
            if (FInputPause.IsChanged && (playback != null))
            {

                if (FInputPause[0])
                {
                    if ((playback.State == KStudioPlaybackState.Playing))
                    {
                        playback.Pause();
                    }

                }
                else
                {
                    if ((playback.State == KStudioPlaybackState.Paused))
                    {
                        playback.Resume();
                    }

                }

            }

                if (FInputStep[0])
                {
                    if ((playback.State == KStudioPlaybackState.Playing))
                    {
                        playback.Pause();
                    }
                    playback.StepOnce();
                }
                if (FInputSeek.IsChanged && (playback !=null))
                {
                    TimeSpan seekTime =  TimeSpan.FromSeconds(FInputSeek[0]);
                    playback.Pause();
                    playback.SeekByRelativeTime(seekTime);
                    playback.Resume();
            }
			if (playback!=null)
			{
				FRecordDuration[0] = playback.CurrentRelativeTime.TotalSeconds;	                	
				FRecord[0] =  playback.State.ToString();
			}
            

        }

        private void PlayClip(string filePath)
        {
   			using (KStudioClient client = KStudio.CreateClient())
            {          	
                client.ConnectToService();


				KStudioPlaybackFlags flags = KStudioPlaybackFlags.IgnoreOptionalStreams;
                playback = client.CreatePlayback(filePath, flags);
				using (playback)                
            	{
	                playback.PropertyChanged += Recording_PropertyChanged;
                    playback.StateChanged += Playback_StateChanged;
                    playback.LoopCount = 10000;
                    playback.Start();
					
	            	FRecordTotalDuration[0] = playback.Duration.TotalSeconds;

	                while ((playback.State == KStudioPlaybackState.Playing)||(playback.State == KStudioPlaybackState.Paused) && (doPlay))
	                {
	                    Thread.Sleep(50);                        
                    }
	                playback.Stop();
            	}
				client.DisconnectFromService();                
            }				
        }		
        public void StopRecording()
        {
			doPlay = false;
        }

        // when we create the recording object in a different threadm this handler gets 
        // fired only at the beginning and the end of the recording
        // when we create the recording object in a main thread, thigs get ugly
        private void Playback_StateChanged(object sender, EventArgs e)
        {

        }

        private void Recording_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e != null))
            {
                try
                {
                }
                catch (Exception)
                {
                    
                }
            }        					
        }	
	}
}
