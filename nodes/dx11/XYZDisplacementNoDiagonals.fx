//@author: vux
//@help: standard constant shader
//@tags: color
//@credits: 

struct vsInput
{
    float4 posObject : POSITION;
    float4 uv: TEXCOORD0;	
};

struct psInput
{
    float4 posScreen : SV_Position;
};
//    //normal in view space
    //
struct vsInputTextured
{
    float4 posObject : POSITION;
	//float4 normalObject: NORMAL;
	float4 uv: TEXCOORD0;
};

struct gsInputTextured
{
    float4 posObject : POSITION;
	float4 uv: TEXCOORD0;
};
struct psInputTextured
{
    float4 posScreen : SV_Position;
	//float4 normScreen: NORMAL;		
    float4 uv: TEXCOORD0;
};
Texture2D xyzTexture <string uiname="XYZ Texture";>;
Texture2D inputTexture <string uiname="Texture";>;
SamplerState linearSampler <string uiname="Sampler State";>
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState pointSampler <string uiname="Point Sampler State";>
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
cbuffer cbPerDraw : register(b0)
{
	float4x4 tVP : VIEWPROJECTION;
	float4x4 tWVP : WORLDVIEWPROJECTION;
	float4x4 tWIT: WORLDINVERSETRANSPOSE;	
	float4x4 tV: VIEW;
	float4x4 tP: PROJECTION;	
};

cbuffer cbPerObj : register( b1 )
{
	float4x4 tW : WORLD;
	float Alpha <float uimin=0.0; float uimax=1.0;> = 1; 
	float4 cAmb <bool color=true;String uiname="Color";> = { 1.0f,1.0f,1.0f,1.0f };
	float4x4 tColor <string uiname="Transform";>;
};

cbuffer cbTextureData : register(b2)
{
	float4x4 tTex <string uiname="Texture Transform"; bool uvspace=true; >;
};

psInput VS(vsInput input)
{
	psInput output;

	float4 col = inputTexture.SampleLevel(pointSampler,input.uv.xy,0);
	output.posScreen =input.posObject;
	
	return output;
}

psInputTextured VS_Textured(vsInputTextured input)
{
	psInputTextured output;
	float4 col = xyzTexture.SampleLevel(linearSampler,input.uv.xy,0);
	//output.normScreen = float4(normalize(mul(mul(input.normalObject.xyz, (float3x3)tWIT),(float3x3)tV).xyz),1);	
	output.posScreen =input.posObject;
	output.uv = mul(input.uv, tTex);
	return output;
}
float maxLength = 0.005;
[maxvertexcount(6)]
void GS_Textured(triangle psInputTextured input[3], inout LineStream<psInputTextured> gsout)
{
	psInputTextured o;
	
	float EPSILON = 0.01f;
	
	//Grab triangles positions
	float3 t1 = input[0].posScreen.xyz;
    float3 t2 = input[1].posScreen.xyz;
	float3 t3 = input[2].posScreen.xyz;
	
	
	//Compute lines
	float3 l1 = t2 - t1;
	float3 l2 = t3 - t2;
	float3 l3 = t3 - t1;
	
	//Compute edge length
	float dl1 = dot(l1,l1);
	float dl2 = dot(l2,l2);
	float dl3 = dot(l3,l3);
	
	//Get max length
	float maxdistsqr = max(max(dl1,dl2),dl3);
	
	/*Append if lower than max length
	please note that will not work with all goemetries,
	but for grid/boxes/spheres type of topology this is a very simple
	way. Also note this is not optimized, 
	code is quite expanded for readability*/

	if ((dl1 < maxdistsqr) && (maxdistsqr<maxLength))
	{
		o.posScreen =  mul(float4(t1,1),tWVP);
		//o.normScreen = input[0].normScreen;
		o.uv = input[0].uv;
		gsout.Append(o);
	
		o.posScreen =  mul(float4(t2,1),tWVP);
		//o.normScreen = input[1].normScreen;		
		o.uv = input[1].uv;
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
		
	if ((dl2 < maxdistsqr) && (maxdistsqr<maxLength))
	{
		o.posScreen =  mul(float4(t3,1),tWVP);
		//o.normScreen = input[2].normScreen;		
		o.uv = input[2].uv;
		gsout.Append(o);
	
		o.posScreen =  mul(float4(t2,1),tWVP);
		//o.normScreen = input[1].normScreen;
		o.uv = input[1].uv;
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	
	
	if ((dl3 < maxdistsqr)&&(maxdistsqr<maxLength))
	{
		o.posScreen = mul(float4(t3,1),tWVP);
		//o.normScreen = input[2].normScreen;
		o.uv = input[2].uv;		
		gsout.Append(o);
	
		o.posScreen = mul(float4(t1,1),tWVP);
		//o.normScreen = input[0].normScreen;
		o.uv = input[0].uv;		
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	
}
float minDistance = 0;
[maxvertexcount(6)]
void GS_TexturedCrop(triangle psInputTextured input[3], inout LineStream<psInputTextured> gsout)
{
	psInputTextured o;
	
	float EPSILON = 0.01f;
	
	//Grab triangles positions
	float4 t1 = mul(float4(input[0].posScreen.xyz,1),tW);
    float4 t2 = mul(float4(input[1].posScreen.xyz,1),tW);
	float4 t3 = mul(float4(input[2].posScreen.xyz,1),tW);
	
	
	//Compute lines
	float3 l1 = t2.xyz - t1.xyz;
	float3 l2 = t3.xyz - t2.xyz;
	float3 l3 = t3.xyz - t1.xyz;
	
	//Compute edge length
	float dl1 = dot(l1,l1);
	float dl2 = dot(l2,l2);
	float dl3 = dot(l3,l3);
	
	//Get max length
	float maxdistsqr = max(max(dl1,dl2),dl3);
	
	/*Append if lower than max length
	please note that will not work with all goemetries,
	but for grid/boxes/spheres type of topology this is a very simple
	way. Also note this is not optimized, 
	code is quite expanded for readability*/

	if ((dl1 < maxdistsqr) && (maxdistsqr<maxLength))
	{
		o.posScreen =  mul(t1,tVP);
		//o.normScreen = input[0].normScreen;						
		o.uv = input[0].uv;
		gsout.Append(o);
	
		o.posScreen =  mul(t2,tVP);
		//o.normScreen = input[1].normScreen;						
		o.uv = input[1].uv;
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
		
	if ((dl2 < maxdistsqr) && (maxdistsqr<maxLength))
	{
		o.posScreen =  mul(t3,tVP);
		//o.normScreen = input[2].normScreen;						
		o.uv = input[2].uv;
		gsout.Append(o);
	
		o.posScreen =  mul(t2,tVP);
		//o.normScreen = input[1].normScreen;						
		o.uv = input[1].uv;
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	
	
	if ((dl3 < maxdistsqr)&&(maxdistsqr<maxLength))
	{
		o.posScreen =  mul(t3,tVP);
		//o.normScreen = input[2].normScreen;						
		o.uv = input[2].uv;		
		gsout.Append(o);
	
		o.posScreen =  mul(t1,tVP);
		//o.normScreen = input[0].normScreen;						
		o.uv = input[0].uv;		
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
		
	
	
}
float3 boundsMin;
float3 boundsMax;
[maxvertexcount(3)]
void GS_TexturedCropMesh(triangle psInputTextured input[3], inout TriangleStream<psInputTextured> gsout)
{
	psInputTextured o;
	
	float EPSILON = 0.01f;
	
	//Grab triangles positions
	float4 t1 = mul(float4(input[0].posScreen.xyz,1),tW);
    float4 t2 = mul(float4(input[1].posScreen.xyz,1),tW);
	float4 t3 = mul(float4(input[2].posScreen.xyz,1),tW);
	
	
	//Compute lines
	float3 l1 = t2.xyz - t1.xyz;
	float3 l2 = t3.xyz - t2.xyz;
	float3 l3 = t3.xyz - t1.xyz;
	
	//Compute edge length
	float dl1 = dot(l1,l1);
	float dl2 = dot(l2,l2);
	float dl3 = dot(l3,l3);
	
	//Get max length
	float maxdistsqr = max(max(dl1,dl2),dl3);
	
	/*Append if lower than max length
	please note that will not work with all goemetries,
	but for grid/boxes/spheres type of topology this is a very simple
	way. Also note this is not optimized, 
	code is quite expanded for readability*/
	if ((boundsMin.x<t1.x)&&(boundsMax.x>t1.x)
	  &&(boundsMin.y<t1.y)&&(boundsMax.y>t1.y) 
	  &&(boundsMin.z<t1.z)&&(boundsMax.z>t1.z)) 	
	{
		if (t1.z>minDistance) 
		{
			if (maxdistsqr<maxLength)
			{
				o.posScreen =  mul(t1,tVP);
				o.uv = input[0].uv;
				gsout.Append(o);
			
				o.posScreen =  mul(t2,tVP);
				o.uv = input[1].uv;
				gsout.Append(o);
	
				o.posScreen =  mul(t3,tVP);
				o.uv = input[2].uv;
				gsout.Append(o);
				
				gsout.RestartStrip();
			}		
		}		
	}	
}

[maxvertexcount(3)]
void GS_TexturedMesh(triangle psInputTextured input[3], inout TriangleStream<psInputTextured> gsout)
{
	psInputTextured o;
	
	float EPSILON = 0.01f;
	if ((input[0].posScreen.z>EPSILON)&&(input[1].posScreen.z>EPSILON)&&(input[2].posScreen.z>EPSILON))
	{
		//Grab triangles positions
		float4 t1 = mul(float4(input[0].posScreen.xyz,1),tW);
	    float4 t2 = mul(float4(input[1].posScreen.xyz,1),tW);
		float4 t3 = mul(float4(input[2].posScreen.xyz,1),tW);

		o.posScreen =  mul(t1,tVP);
		o.uv = input[0].uv;
		gsout.Append(o);
	
		o.posScreen =  mul(t2,tVP);
		o.uv = input[1].uv;
		gsout.Append(o);

		o.posScreen =  mul(t3,tVP);
		o.uv = input[2].uv;
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
}
float4 PS(psInput input): SV_Target
{
    float4 col = cAmb;
	col = mul(col, tColor);
	col.a *= Alpha;
    return col;
}


float4 PS_Textured(psInputTextured input): SV_Target
{
    float4 col = inputTexture.Sample(linearSampler,input.uv.xy);
	col.a *= Alpha;	
    return col; 
}

technique11 ConstantNoDiagonalsLines
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Textured() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_Textured() ) );		
		SetPixelShader( CompileShader( ps_4_0, PS_Textured() ) );
	}
}

technique11 ConstantNoDiagonalsCropLines
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Textured() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_TexturedCrop() ) );		
		SetPixelShader( CompileShader( ps_4_0, PS_Textured() ) );
	}
}

technique11 ConstantNoDiagonalsMesh
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Textured() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_TexturedCropMesh() ) );		
		SetPixelShader( CompileShader( ps_4_0, PS_Textured() ) );
	}
}

technique11 ConstantNoDiagonalsMeshNoCrop
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Textured() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_TexturedMesh() ) );		
		SetPixelShader( CompileShader( ps_4_0, PS_Textured() ) );
	}
}








