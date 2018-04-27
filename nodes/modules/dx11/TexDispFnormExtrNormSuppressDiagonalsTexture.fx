float4x4 tW: WORLD;        
float4x4 tV: VIEW;         
float4x4 tWV: WORLDVIEW;
float4x4 tP: PROJECTION;
float4x4 tWVP : WORLDVIEWPROJECTION;

Texture2D NormalizedDepthTexture <string uiname="Depth Texture";>;
Texture2D<float2> RayTexture <string uiname="Raytable Texture";>;
Texture2D inputTexture <string uiname="Displacement Texture";>;
Texture2D rgbInputTexture <string uiname="RGBDepth  Texture";>;

SamplerState pointSampler <string uiname="Point Sampler State";>
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
SamplerState linearSampler <string uiname="Linear Sampler State";>
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};
struct vertextStruct
{
	float4 pos : POSITION;
	float3 normal : NORMAL;	
	float2 uv : TEXCOORD0;
};
struct psIn
{
    float4 pos: SV_POSITION;
	float4 uv: TEXCOORD0;	
};
struct vsin
{
	float4 pos : POSITION;
	float4 uv: TEXCOORD0;	
};

struct vs2gs
{
    float4 pos : POSITION;
	float4 uv: TEXCOORD0;	
};
vertextStruct VS(vertextStruct input)
{
	vertextStruct o;
	
	float d = NormalizedDepthTexture.SampleLevel(pointSampler,float2(input.uv.xy),0).r;	
	if (d <0.0001) 
	{
		o.pos =  input.pos;
		o.normal = input.normal;
		o.uv = input.uv;

	}
	else
	{
		d *= 65.535f; //Multiply by MAX_USHORT-1	
		float2 ray = RayTexture.SampleLevel(pointSampler,float2(input.uv.xy),0).rg;
		o.pos =  float4(d*ray,d,1);	
		o.normal = input.normal;
		o.uv = input.uv;
		
	}
	o.pos =  input.pos;
	o.normal = input.normal;
	o.uv = input.uv;
	return o;
}

float extrusionFactor;
[maxvertexcount(3)]
void GS(triangle vertextStruct input[3], inout LineStream<vertextStruct> gsout)
{ 
	
		vertextStruct o;
		
		o.pos = mul(float4(input[0].pos),tWVP);
		o.uv = input[0].uv;
		o.normal = input[0].normal;
		gsout.Append(o);
	
		o.pos = mul(float4(input[1].pos),tWVP);
		o.uv = input[1].uv;
		o.normal = input[1].normal;
		gsout.Append(o);

		o.pos = mul(float4(input[2].pos),tWVP);
		o.uv = input[2].uv;
		o.normal = input[2].normal;
		gsout.Append(o);
		
		gsout.RestartStrip();	
	/*
	if ((input[0].pos.z >0.2)&&(input[1].pos.z >0.2)&& (input[2].pos.z >0.2))
	{		


		float3 p1 = input[0].pos.xyz;
		float3 p2 = input[1].pos.xyz;
		float3 p3 = input[2].pos.xyz;

		float3 l1 = p2.xyz - p1.xyz;
		float3 l2 = p3.xyz - p2.xyz;
		float3 l3 = p3.xyz - p1.xyz;
		
		//Compute edge length
		float dl1 = dot(l1,l1);
		float dl2 = dot(l2,l2);
		float dl3 = dot(l3,l3);
		
		//Get max length
		float maxdistsqr = max(max(dl1,dl2),dl3);	

	
		
		float3 faceEdgeA = p2 - p1;
	    float3 faceEdgeB = p1 - p3;
	    float3 norm = cross(faceEdgeB, faceEdgeA);	
		norm = normalize(norm);
		float3 extrusion = 0;//norm*(1 - inputTexture.SampleLevel(pointSampler,input[0].uv.xy,0).xyz)*extrusionFactor;			
		
		//Compute lines
		
		if (dl1 < maxdistsqr)
		{		
			elem.pos = mul(float4((p1+extrusion),1),tWVP);
			elem.uv = input[0].uv;
			elem.normal = norm;			
			gsout.Append(elem);
			
			elem.pos = mul(float4((p2+extrusion),1),tWVP);
			elem.uv = input[1].uv;		
			elem.normal = norm;			
			gsout.Append(elem);
			
			gsout.RestartStrip();
		}
		
		if (dl2 < maxdistsqr)
		{		
			elem.pos = mul(float4((p3+extrusion),1),tWVP);
			elem.uv = input[2].uv;
			elem.normal = norm;			
			gsout.Append(elem);
			
			elem.pos = mul(float4((p2+extrusion),1),tWVP);
			elem.uv = input[1].uv;		
			elem.normal = norm;			
			gsout.Append(elem);
			
			gsout.RestartStrip();		
		
		}
	
		if (dl3 < maxdistsqr)
		{		
			elem.pos = mul(float4((p3+extrusion),1),tWVP);
			elem.uv = input[2].uv;
			elem.normal = norm;			
			gsout.Append(elem);
			
			elem.pos = mul(float4((p1+extrusion),1),tWVP);
			elem.uv = input[0].uv;		
			elem.normal = norm;			
			gsout.Append(elem);
			
			gsout.RestartStrip();				
		}
	}
	*/
}
float alpha = 1;
float4 PS(vertextStruct input): SV_Target
{
	float4 col = rgbInputTexture.Sample(linearSampler,input.uv.xy);
	//col.a = alpha;
	col = 10;
    return col;
}




technique10 RenderNoDiagonals
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );   
		//SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );    	
    }  
}

