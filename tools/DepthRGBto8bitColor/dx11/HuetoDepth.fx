//@author: vux
//@help: template for standard shaders
//@tags: template
//@credits: 

Texture2D texture2d <string uiname="Texture";>;

SamplerState linearSampler : IMMUTABLE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};
SamplerState pointSampler : IMMUTABLE
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
  
cbuffer cbPerDraw : register( b0 )
{
	float4x4 tVP : VIEWPROJECTION;	
};

cbuffer cbPerObj : register( b1 )
{
	float4x4 tW : WORLD;
	float4 cAmb <bool color=true;String uiname="Color";> = { 1.0f,1.0f,1.0f,1.0f };
};

struct VS_IN
{
	float4 PosO : POSITION;
	float4 TexCd : TEXCOORD0;

};

struct vs2ps
{
    float4 PosWVP: SV_POSITION;
    float4 TexCd: TEXCOORD0;
};

vs2ps VS(VS_IN input)
{
    vs2ps output;
    output.PosWVP  = mul(input.PosO,mul(tW,tVP));
    output.TexCd = input.TexCd;
    return output;
}



float RGBCVtoHUE(in float3 RGB, in float C, in float V)
{
  float3 Delta = (V - RGB) / C;
  Delta.rgb -= Delta.brg;
  Delta.rgb += float3(2,4,6);
  // NOTE 1
  Delta.brg = step(V, RGB) * Delta.brg;
  float H;
  H = max(Delta.r, max(Delta.g, Delta.b));
  return frac(H / 6);
}

float3 RGBtoHSL(in float3 RGB)
{
	float3 HSL = 0;
	float U, V;

	U = -min(RGB.r, min(RGB.g, RGB.b));
	V = max(RGB.r, max(RGB.g, RGB.b));
	HSL.z = (V - U) * 0.5;
	float C = V + U;
	if (C != 0)
	{
	  HSL.x = RGBCVtoHUE(RGB, C, V);
	  HSL.y = C / (1 - abs(2 * HSL.z - 1));
	}
	return HSL;
}

float4 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return float4(saturate(float3(R,G,B)),1);
}
float substractionFactor = 1;
float divisionFactor = 0.2;
float4 PS(vs2ps In): SV_Target
{
    float4 col = texture2d.Sample(pointSampler,In.TexCd.xy);
	col.rgb = (RGBtoHSL(col.rgb));
	//filter jpg artefacts - non saturated edges
	if (col.b<0.2) col.r = 1000;	
	col.rgb = (((col.r))*(1/divisionFactor)+substractionFactor);
	
    return float4(col.rrr,1);
}

technique10 Constant
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}




