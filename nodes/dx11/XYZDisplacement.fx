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

struct vsInputTextured
{
    float4 posObject : POSITION;
	float4 uv: TEXCOORD0;
};

struct psInputTextured
{
    float4 posScreen : SV_Position;
    float4 uv: TEXCOORD0;
};
Texture2D xyzTexture <string uiname="XYZ Texture";>;
Texture2D inputTexture <string uiname="Texture";>;
Texture2D displaceTexture <string uiname="Displace Texture";>;
Texture2D maskTexture <string uiname="Mask Texture";>;
Texture2D irTexture <string uiname="IR Texture";>;
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
	output.posScreen = mul(float4(col.rgb,1),mul(tW,tVP));
	return output;
}

psInputTextured VS_Textured(vsInputTextured input)
{
	psInputTextured output;
	float4 col = xyzTexture.SampleLevel(linearSampler,input.uv.xy,0);
	output.posScreen = mul(float4(col.rgb,1),mul(tW,tVP));
	output.uv = mul(input.uv, tTex);
	return output;
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
	float4 xyzCol = xyzTexture.Sample(linearSampler,input.uv.xy);
    float4 col = inputTexture.Sample(linearSampler,input.uv.xy) * cAmb;
	col.rgb = col.rgb;
	col.a *= xyzCol.b;
    return col;
}

technique11 Constant <string noTexCdFallback="ConstantNoTexture"; >
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Textured() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Textured() ) );
	}
}

technique11 ConstantNoTexture
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}





