//@author: vux
//@help: standard constant shader
//@tags: color
//@credits: 

struct vsInput
{
    float4 posObject : POSITION;
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
	float3 col: COLOR;
};

Texture2D inputTexture <string uiname="Texture";>;
Texture2D displaceTexture <string uiname="Displace Texture";>;
SamplerState linearSampler <string uiname="Sampler State";>
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

cbuffer cbPerDraw : register(b0)
{
	float4x4 tVP : VIEWPROJECTION;
};

cbuffer cbPerObj : register( b1 )
{
	float4x4 tWVP : WORLDVIEWPROJECTION;
	float4x4 tW : WORLD;	
	float factor;
	float distortionFactor;	
	float alpha = 0.5;
};

psInput VS(vsInput input)
{
	psInput output;
	output.posScreen = mul(input.posObject,tWVP);
	return output;
}

psInputTextured VS_Textured(vsInputTextured input)
{
	psInputTextured output;
	float4 displaceTmp = float4(displaceTexture.SampleLevel(linearSampler,input.uv.xy,0).rgb*distortionFactor,0);
	
	output.posScreen = mul(input.posObject+ displaceTmp,tWVP);

	float4 colTmp = inputTexture.SampleLevel(linearSampler,input.uv.xy,0);
	float4 col = colTmp;
	colTmp.r = (colTmp.r+colTmp.b+colTmp.g)*0.3333;
	colTmp.rgb = colTmp.r;
	
	output.col = lerp(colTmp, col,factor).rgb;
	return output;
}


float4 PS(psInput input): SV_Target
{
    float4 col = float4(1,1,1,1);
    return col;
}



float4 PS_Textured(psInputTextured input): SV_Target
{
    return float4(input.col, alpha);
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





