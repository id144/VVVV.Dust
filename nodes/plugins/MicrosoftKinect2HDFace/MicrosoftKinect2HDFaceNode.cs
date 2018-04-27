using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2; 
using SlimDX; 
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1; 
using Microsoft.Kinect;
using Vector4 = SlimDX.Vector4;
using Quaternion = SlimDX.Quaternion;
using Microsoft.Kinect.Face;


namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "HDFace",
                Category = "Kinect2",
                Version = "Microsoft",
                Author = "flateric",
                Tags = "DX11",
                Help = "Returns high definition face data for user")]
    public unsafe class KinectHDFaceNode : IPluginEvaluate, IPluginConnections, IDisposable
    {

        [Input("Kinect Runtime")]
        protected Pin<KinectRuntime> FInRuntime;

        [Input("Rotation Check")]
        protected Pin<bool> FInRCheck;

        [Input("Tracking Id")]
        protected ISpread<string> FInId;

        [Input("Is Paused")]
        protected ISpread<bool> FInPaused;


        [Output("Face Vertices Points")]
        protected ISpread<Vector3> FOutFaceVerticesPoints;

        [Output("Face Vertices Camera Points")]
        protected ISpread<Vector2> FOutFaceVerticesCameraPoints;


        [Output("Face UV Points")]
        protected ISpread<Vector2> FOutFaceUVPoints;

        [Output("Face Shape Deformations")]
        protected ISpread<double> FOutFaceShapeDeformations;


        [Output("Rotation")]
        protected ISpread<Quaternion> FOutOrientation;

        [Output("BMin")]
        protected ISpread<Vector3> FOutBmin;

        [Output("BMax")]
        protected ISpread<Vector3> FOutBMax;

        [Output("Is Paused")]
        protected ISpread<bool> FOutPaused;

        [Output("TrackingId")]
        protected ISpread<ulong> FOuTrackingId;


        [Output("Tracking Quality")]
        protected ISpread<string> FOuTrackingQuality;

        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private HighDefinitionFaceFrameSource faceFrameSource = null;
        private HighDefinitionFaceFrameReader faceFrameReader = null;

        private FaceModel faceModel = new FaceModel();
        private FaceAlignment faceAlignment = new FaceAlignment();


        private Body[] lastframe = new Body[6];

        private object m_lock = new object();
        private string alignmentQuality = "";


        private CameraSpacePoint[] cameraPoints;
        private ColorSpacePoint[] colorPoints = new ColorSpacePoint[FaceModel.VertexCount];

        public KinectHDFaceNode()
        {
        }



        public void Evaluate(int SpreadMax)
        {
            
            if (this.FInvalidateConnect)
            {
                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.faceFrameSource = new HighDefinitionFaceFrameSource(this.runtime.Runtime);
                        this.faceFrameReader = this.faceFrameSource.OpenReader();
                        this.faceFrameReader.FrameArrived += this.faceReader_FrameArrived;
                        this.faceFrameReader.IsPaused = true;
                    }
                }
                else
                {
                    this.faceFrameReader.FrameArrived -= this.faceReader_FrameArrived;
                    this.faceFrameReader.Dispose();
                }

                this.FInvalidateConnect = false;
            }

            if (this.faceFrameSource != null)
            {
                ulong id = 0;
                try
                {
                    id = ulong.Parse(this.FInId[0]);
                }
                catch
                {

                }
                this.faceFrameSource.TrackingId = id;
                this.faceFrameReader.IsPaused = this.FInPaused[0];
            }

            

            this.FOutPaused[0] = this.faceFrameReader != null ? this.faceFrameReader.IsPaused : true;

            this.FOutFaceShapeDeformations.SliceCount = this.faceModel.FaceShapeDeformations.Count;
            for (int i = 0; i < this.faceModel.FaceShapeDeformations.Count; i++)
            {

                this.FOutFaceShapeDeformations[i] = this.faceModel.FaceShapeDeformations[0];

            }
			if (this.cameraPoints != null) 
        	{
	            this.FOutFaceVerticesPoints.SliceCount = this.cameraPoints.Length;
	            for (int i = 0; i < this.cameraPoints.Length; i++) 
	            { 
	                Vector3 x;
	
	                x.X = this.cameraPoints[i].X;
	                x.Y = this.cameraPoints[i].Y;
	                x.Z = this.cameraPoints[i].Z;
	                this.FOutFaceVerticesPoints[i] = x;
	            }        		
				this.runtime.Runtime.CoordinateMapper.MapCameraPointsToColorSpace(this.cameraPoints, this.colorPoints);
				
				this.FOutFaceVerticesCameraPoints.SliceCount = this.colorPoints.Length;
				for (int i = 0; i < this.colorPoints.Length; i++)
				{
					Vector2 x;
					
					x.X = this.colorPoints[i].X;
					x.Y = this.colorPoints[i].Y;
					
					this.FOutFaceVerticesCameraPoints[i] = x;
				}
        	}



            this.FOuTrackingQuality.SliceCount = 1;
            this.FOuTrackingQuality[0] = this.alignmentQuality;
        }


        void faceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (HighDefinitionFaceFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    this.alignmentQuality = frame.FaceAlignmentQuality.ToString();
                    if (frame.IsTrackingIdValid == false) { return; }
                    if (frame.FaceAlignmentQuality == FaceAlignmentQuality.Low) { return; }

                    frame.GetAndRefreshFaceAlignmentResult(this.faceAlignment);
                    var o = this.faceAlignment.FaceOrientation;
                    this.FOutOrientation[0] = new Quaternion(o.X, o.Y, o.Z, o.W);

                    if (this.FInRCheck[0])
                    {
                        float f = this.FOutOrientation[0].LengthSquared();
                        if (f > 0.1f)
                        {
                            this.cameraPoints = this.faceModel.CalculateVerticesForAlignment(this.faceAlignment).ToArray();
                            this.runtime.Runtime.CoordinateMapper.MapCameraPointsToColorSpace(this.cameraPoints, this.colorPoints);
                            SetBounds();

                        }
                    }
                    else
                    {
                        this.cameraPoints = this.faceModel.CalculateVerticesForAlignment(this.faceAlignment).ToArray();
                        this.runtime.Runtime.CoordinateMapper.MapCameraPointsToColorSpace(this.cameraPoints, this.colorPoints);

                        SetBounds();

                    }
                   

                }
            }
        }

        private void SetBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            for (int i = 0; i < this.cameraPoints.Length ;i++)
            {
                CameraSpacePoint cp = this.cameraPoints[i];
                min.X = Math.Min(min.X, cp.X);
                min.Y = Math.Min(min.Y, cp.Y);
                min.Z = Math.Min(min.Z, cp.Z);

                max.X = Math.Max(max.X, cp.X);
                max.Y = Math.Max(max.Y, cp.Y);
                max.Z = Math.Max(max.Z, cp.Z);
            }

            this.FOutBmin[0] = min;
            this.FOutBMax[0] = max;

        }


        private void SkeletonReady(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame skeletonFrame = e.FrameReference.AcquireFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.GetAndRefreshBodyData(this.lastframe);

                    bool found = false;
                    float minZ = float.MaxValue;
                    ulong cloestId = 0;

                    for (int i = 0; i < this.lastframe.Length; i++)
                    {
                        if (this.lastframe[i].IsTracked)
                        {
                            found = true;
                            var z = this.lastframe[i].Joints[JointType.Head].Position.Z;
                            if (z < minZ)
                            {
                                z = minZ;
                                cloestId = this.lastframe[i].TrackingId;
                            }
                        }
                    }

                    if (found)
                    {
                        this.faceFrameSource.TrackingId = cloestId;
                        this.FOuTrackingId[0] = this.faceFrameSource.TrackingId;                 
                        this.faceFrameReader.IsPaused = false;
                    }
                    else
                    {
                        this.faceFrameReader.IsPaused = true;
                        this.FOuTrackingId[0] =0;
                    }

                    

                    skeletonFrame.Dispose();
                }
            }
        }


        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void Dispose()
        {

        }
    }
}
