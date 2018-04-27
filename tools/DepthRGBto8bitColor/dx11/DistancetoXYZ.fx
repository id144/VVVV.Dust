//@author: vux
//@help: template for standard shaders
//@tags: template
//@credits: 
Texture2D texture2dRGB <string uiname="Texture RGB";>;
Texture2D texture2dDistance <string uiname="Texture distance";>;

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
	float4x4 tWVP : WORLDVIEWPROJECTION;	
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
struct vs2gs
{
    float4 PosO : SV_POSITION;
	float4 TexCd : TEXCOORD0;	
};

struct vs2ps
{
    float4 PosWVP: SV_POSITION;
    float4 TexCd: TEXCOORD0;
};
float xFactor;
float yFactor;

vs2ps VS(VS_IN input)
{
    vs2ps output;
	float4 col = texture2dDistance.SampleLevel(pointSampler,input.TexCd.xy,0) * cAmb;
	if (col.r <2) col.r = 1000;
	input.PosO.z= col.r;
	input.PosO.x= col.r*input.PosO.x*xFactor;
	input.PosO.y= col.r*input.PosO.y*yFactor;	
    output.PosWVP  = input.PosO;
    output.TexCd = input.TexCd;
    return output;
}
[maxvertexcount(6)]
void GS_Diag(triangle vs2gs input[3], inout LineStream<vs2ps> gsout)
{
	vs2ps o;
	
	float EPSILON = 0.01f;
	
	//Grab triangles positions
	float3 t1 = input[0].PosO.xyz;
    float3 t2 = input[1].PosO.xyz;
	float3 t3 = input[2].PosO.xyz;
	
	
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
	if (maxdistsqr<10.3) { 
	if (dl1 < maxdistsqr)
	{
		o.PosWVP = mul(float4(t1,1),tWVP);
		o.TexCd = input[0].TexCd;		
		gsout.Append(o);
	
		o.PosWVP = mul(float4(t2,1),tWVP);
		o.TexCd = input[1].TexCd;	
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
		
	if (dl2 < maxdistsqr)
	{
		o.PosWVP = mul(float4(t3,1),tWVP);
		o.TexCd = input[2].TexCd;			
		gsout.Append(o);
	
		o.PosWVP = mul(float4(t2,1),tWVP);
		o.TexCd = input[1].TexCd;			
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	
	
	if (dl3 < maxdistsqr)
	{
		o.PosWVP = mul(float4(t3,1),tWVP);
		o.TexCd = input[2].TexCd;			
		gsout.Append(o);
	
		o.PosWVP = mul(float4(t1,1),tWVP);
		o.TexCd = input[0].TexCd;			
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	}
}




float4 PS(vs2ps In): SV_Target
{
    float4 col = texture2dRGB.Sample(linearSampler,In.TexCd.xy) * cAmb;
    return col;
}





technique10 Constant
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_Diag() ) );		
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}




