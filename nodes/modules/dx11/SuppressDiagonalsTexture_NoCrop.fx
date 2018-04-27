cbuffer cbPerDraw : register(b0)
{
	float4x4 tV: VIEW;
	float4x4 tP: PROJECTION;
	float4x4 tVP : VIEWPROJECTION;	
	float4x4 tWVP : WORLDVIEWPROJECTION;	
};




cbuffer cbPerObj : register( b1 )
{
	float4x4 tW : WORLD;
	float4x4 tWV: WORLDVIEW;
	float4x4 tWIT: WORLDINVERSETRANSPOSE;
	float alpha = 1;	
};



Texture2D inputTexture <string uiname="Texture";>;
SamplerState linearSampler <string uiname="Sampler State";>
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct vsin
{
	float4 pos : POSITION;
	float3 normal : NORMAL;		
	float4 uv: TEXCOORD0;	
};

struct vs2gs
{
    float4 pos : POSITION;
	float3 normal : NORMAL;		
	float4 uv: TEXCOORD0;	
};

struct psIn
{
    float4 pos: SV_POSITION;
	float3 normal : NORMAL;		
	float4 uv: TEXCOORD0;	
};

vs2gs VS_Pass(vsin input)
{
	//Passtrough in that case, since we will process in gs
	
	//We don't need normals we will calculate them on the fly
	vs2gs output;
	output.uv = input.uv;
	output.pos = input.pos;
	output.normal = input.normal;
    return output;
}

psIn VS(vsin input)
{
	//Standard displat, so transform as we would usually do
	psIn output;
	output.uv = input.uv;	
	output.pos = mul(input.pos,tWVP);
	output.normal = input.normal;	
    return output;
}




[maxvertexcount(6)]
void GS_Diag(triangle vs2gs input[3], inout LineStream<psIn> gsout)
{
	psIn o;
	
	float EPSILON = 0.01f;
	
	//Grab triangles positions
	float3 t1 = input[0].pos.xyz;
    float3 t2 = input[1].pos.xyz;
	float3 t3 = input[2].pos.xyz;
	
	
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
	//float3 NormV = normalize(mul(mul(input[0].normal, (float3x3)tWIT),(float3x3)tV).xyz);
	if (dl1 < maxdistsqr)
	{
		o.pos = mul(float4(t1,1),tWVP);
		o.uv = input[0].uv;
		o.normal = input[0].normal;
		gsout.Append(o);
	
		o.pos = mul(float4(t2,1),tWVP);
		o.uv = input[1].uv;		
		o.normal = input[1].normal;		
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
		
	if (dl2 < maxdistsqr)
	{
		o.pos = mul(float4(t3,1),tWVP);
		o.uv = input[2].uv;		
		o.normal = input[2].normal;
		gsout.Append(o);
	
		o.pos = mul(float4(t2,1),tWVP);
		o.uv = input[1].uv;		
		o.normal = input[1].normal;
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	
	
	if (dl3 < maxdistsqr)
	{
		o.pos = mul(float4(t3,1),tWVP)*2;
		o.uv = input[2].uv;
		o.normal = input[2].normal;		
		gsout.Append(o);
	
		o.pos = mul(float4(t1,1),tWVP);
		o.uv = input[0].uv;
		o.normal = input[0].normal;				
		gsout.Append(o);
		
		gsout.RestartStrip();
	}
	
}

float4 PS(psIn input): SV_Target
{
	float4 col = inputTexture.Sample(linearSampler,input.uv.xy);
	if (input.normal.z > 0 )
	{
		col.a *= alpha*0.3;
	} else
	{
		col.a *= alpha;
	}
	
	
    return col;
}

technique10 Render
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}

technique11 RenderNoDiagonals
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Pass() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS_Diag() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}





