#region usings
using System;
using System.ComponentModel.Composition;
using System.Xml;
using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ParseXMP", 
	Category = "Reality Capture",
	Help = "Basic template with one transform in/out",
	Tags = "photogrammetry")]
	#endregion PluginInfo
	public class RealityCaptureParseXMPNode : IPluginEvaluate
	{
		#region fields & pins

		[Input("Input")]
		public IDiffSpread<Matrix4x4> FInput;

		[Input("File")]
		public IDiffSpread<string> FInputFile;

		[Output("Output")]
		public ISpread<Matrix4x4> FOutput;

		[Output("DistortionModel")]
		public ISpread<string> FOutputDistortionModel;

		[Output("FocalLength35mm")]
		public ISpread<double> FOutputFocalLength35mm;

		[Output("Skew")]
		public ISpread<double> FOutputSkew;

		[Output("PrincipalPointU")]
		public ISpread<double> FOutputPrincipalPointU;

		[Output("PrincipalPointV")]
		public ISpread<double> FOutputPrincipalPointV;
	
		[Output("DistortionCoeficients")]
		public ISpread<double> FOutputDistortionCoeficients;
		
		//[Output("Debug")]
		//public ISpread<string> FOutputInfo;


		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			if (FInput.IsChanged || FInputFile.IsChanged) 
			{
				FOutput.SliceCount = SpreadMax;
				FOutputDistortionModel.SliceCount = SpreadMax;
				FOutputFocalLength35mm.SliceCount = SpreadMax;
				FOutputSkew.SliceCount = SpreadMax;
				FOutputPrincipalPointU.SliceCount = SpreadMax;
				FOutputPrincipalPointV.SliceCount = SpreadMax;
				FOutputDistortionCoeficients.SliceCount = SpreadMax*6;
				
				for (int i = 0; i < SpreadMax; i++)
				{
					XmlDocument xmlDoc= new XmlDocument();
					xmlDoc.Load(FInputFile[i]);
					XmlNodeList xmpDescription = xmlDoc.GetElementsByTagName("rdf:Description");
	
					FOutputDistortionModel[i] = xmpDescription[0].Attributes["xcr:DistortionModel"].Value.ToString();
					FOutputFocalLength35mm[i] = double.Parse(xmpDescription[0].Attributes["xcr:FocalLength35mm"].Value.ToString());
					FOutputSkew[i] = double.Parse(xmpDescription[0].Attributes["xcr:Skew"].Value.ToString());
					FOutputPrincipalPointU[i] = double.Parse(xmpDescription[0].Attributes["xcr:PrincipalPointU"].Value.ToString());
					FOutputPrincipalPointV[i] = double.Parse(xmpDescription[0].Attributes["xcr:PrincipalPointV"].Value.ToString());
								
					XmlNodeList xmpRotation = xmlDoc.GetElementsByTagName("xcr:Rotation");
					string[] tokens = xmpRotation[0].InnerText.ToString().Split(' ');
					double[] numbers = Array.ConvertAll(tokens, double.Parse); 				
					Matrix4x4 rotationMatrix = new Matrix4x4(
					numbers[0],numbers[1],numbers[2],0,
					numbers[3],numbers[4],numbers[5],0,
					numbers[6],numbers[7],numbers[8],0,
					0,0,0,1);
							
					XmlNodeList xmpPosition = xmlDoc.GetElementsByTagName("xcr:Position");
					string[] tokens2 = xmpPosition[0].InnerText.ToString().Split(' ');
					double[] numbers2 = Array.ConvertAll(tokens2, double.Parse); 				
					Matrix4x4 translationMatrix = new Matrix4x4(
					 1, 0, 0, 0,
					 0, 1, 0, 0,
					 0, 0, 1, 0,
					numbers2[0],numbers2[1],numbers2[2],1);
					
					rotationMatrix.m13 = -rotationMatrix.m13;
					rotationMatrix.m23 = -rotationMatrix.m23;
					rotationMatrix.m31 = -rotationMatrix.m31;
					rotationMatrix.m32 = -rotationMatrix.m32;
					
					FOutput[i] = FInput[i]* (rotationMatrix*translationMatrix);
					
					XmlNodeList xmpDistortionCoeficients = xmlDoc.GetElementsByTagName("xcr:DistortionCoeficients");
					string[] tokens3 = xmpDistortionCoeficients[0].InnerText.ToString().Split(' ');
					double[] numbers3 = Array.ConvertAll(tokens3, double.Parse); 			
					for (int j = 0; j < 6; j++)
					{
						FOutputDistortionCoeficients[i*6+j] = numbers3[j];
					}

					
				}
			}
		}
	}
}

