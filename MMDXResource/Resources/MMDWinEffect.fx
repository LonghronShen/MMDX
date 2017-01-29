//-----------------------------------------------------------
// MMDWinEffect.fx
//
// MMDX
// Copyright (C) Wilfrem
// �}�C�N���\�t�g�̃T���v�������Ƃɉ���
//-----------------------------------------------------------

//-----------------------------------------------------------------------------
// �e�X�N�`��
//-----------------------------------------------------------------------------
texture Texture;		// �e�N�X�`��
uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

//�X�t�B�A�}�b�v�g�p�t���O�B0:���� 1:��Z 2:���Z
uniform const int UseSphere;
texture Sphere;
uniform const sampler SphereSampler : register(s1) = sampler_state
{
	Texture = (Sphere);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};
uniform const bool UseToon;
texture ToonTex;
uniform const sampler ToonTexSampler : register(s2) = sampler_state
{
	Texture = (ToonTex);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};


//-----------------------------------------------------------------------------
// �萔���W�X�^�錾
//=============================================================================
uniform shared const float3	EyePosition;		// in world space
//-----------------------------------------------------------------------------
// �}�e���A���ݒ�
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 1;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;
uniform const bool		Edge=true;

//-----------------------------------------------------------------------------
// ���C�g�ݒ�
//-----------------------------------------------------------------------------
uniform const float3	AmbientLightColor;
uniform const float3	DirLight0Direction;

//-----------------------------------------------------------------------------
// �}�g���b�N�X
//-----------------------------------------------------------------------------
// �I�u�W�F�N�g�̃��[���h���W
uniform const float4x4	World;	
// �r���[�̃g�����X�t�H�[��
uniform shared const float4x4	View;
// �v���W�F�N�V�����̃g�����X�t�H�[��
uniform shared const float4x4	Projection;


//-----------------------------------------------------------------------------
// Structure definitions
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
	float2 ToonTex;
};

struct CommonVSOutput
{
	float4	Pos_ws;
	float4	Pos_ps;
	float4	Diffuse;
	float3	Specular;
	float2 ToonTexCoord;
	float2 SphereCoord;
};


//-----------------------------------------------------------------------------
// Shader I/O structures
// Nm: Normal
// Tx: Texture
// Vc: Vertex color
//
// Nm Tx Vc
//  1  0  0 VSInputNm
//  1  0  1 VSInputNmVc
//  1  1  0 VSInputNmTx
//  1  1  1 VSInputNmTxVc


//-----------------------------------------------------------------------------
// Vertex shader inputs
//-----------------------------------------------------------------------------


struct VSInputNm
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
};

struct VSInputNmVc
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
	float4	Color		: COLOR;
};

struct VSInputNmTx
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
	float2	TexCoord	: TEXCOORD0;
};

struct VSInputNmTxVc
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
	float2	TexCoord	: TEXCOORD0;
	float4	Color		: COLOR;
};


//-----------------------------------------------------------------------------
// Vertex shader outputs
//-----------------------------------------------------------------------------

struct VertexLightingVSOutput
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;		// Specular.rgb and fog factor
	float2	SphereCoord	: TEXCOORD1;
	float2	ToonTexCoord: TEXCOORD2;
};

struct VertexLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
	float2	SphereCoord	: TEXCOORD1;
	float2	ToonTexCoord: TEXCOORD2;
};

struct EdgeVSOutput
{
	float4	PositionPS	: POSITION;
	float4	Color		: COLOR0;
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct VertexLightingPSInput
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	SphereCoord	: TEXCOORD1;
	float2	ToonTexCoord	: TEXCOORD2;
};

struct VertexLightingPSInputTx
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
	float2	SphereCoord	: TEXCOORD1;
	float2	ToonTexCoord	: TEXCOORD2;
};

//-----------------------------------------------------------------------------
// Compute lighting
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputeLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;

	// Directional Light 0
	float3 L = normalize(-DirLight0Direction);
	float3 H = normalize(E+L);
	float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;//VectorIndex.y=�p���b�g�ԍ�
	result.Specular+=AmbientLightColor*ret.y;
	
	//MMD�ł�Emissive�𑫂��Ă���saturate����̂������炵���B
	result.Diffuse *= DiffuseColor;
	result.Diffuse	+= EmissiveColor;
	result.Diffuse	= saturate(result.Diffuse);
	result.Specular	*= SpecularColor;
	
	//�g�D�[���e�N�X�`���p�̃T���v���ʒu���v�Z
	result.ToonTex.x=clamp(0.5f-dot(normalize(N),normalize(E))*0.5f,0,1);
	result.ToonTex.y=clamp(0.5f-dot(normalize(N),normalize(L))*0.5f,0,1);
	
	return result;
}

CommonVSOutput ComputeCommonVSOutputWithLighting(float4 position, float3 normal)
{
	CommonVSOutput vout;
	
	float4 pos_ws = mul(position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	vout.Pos_ws = pos_ws;
	vout.Pos_ps = pos_ps;
	
	float3 N = normalize(mul(normal, World));
	float3 posToEye = EyePosition - pos_ws;
	float3 E = normalize(posToEye);
	ColorPair lightResult = ComputeLights(E, N);
	
	vout.Diffuse	= float4(lightResult.Diffuse.rgb, Alpha);
	vout.Specular	= lightResult.Specular;
	
	//�g�D�[���e�N�X�`���擾�ʒu���R�s�[
	vout.ToonTexCoord=lightResult.ToonTex;
	//�X�t�B�A�v�Z
	vout.SphereCoord=float2(normal.x/2+0.5,normal.y/2+0.5);
	
	return vout;
}


//-----------------------------------------------------------------------------
// Vertex shaders
//-----------------------------------------------------------------------------
VertexLightingVSOutput VSBasicNm(VSInputNm vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}


VertexLightingVSOutput VSBasicNmVc(VSInputNmVc vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, 1);
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}

VertexLightingVSOutputTx VSBasicNmTx(VSInputNmTx vin)
{
	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	vout.TexCoord	= vin.TexCoord;
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}


VertexLightingVSOutputTx VSBasicNmTxVc(VSInputNmTxVc vin)
{
	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, 1);
	vout.TexCoord	= vin.TexCoord;
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}

EdgeVSOutput VSEdgeNm(VSInputNm vin)
{
	EdgeVSOutput vout;
	
	vout.PositionPS	= mul(mul(mul(vin.Position,World),View),Projection);
	float3 normal	= normalize(mul(vin.Normal,World));
	if(Edge){
		vout.Color.rgb=(normal+1)/2;//�@����0-1�̊Ԃɂ��ĕۑ�
		vout.Color.w=vout.PositionPS.z/vout.PositionPS.w;//�[�x��0-1�ɂ��ăA���t�@�l�ɕۑ�
	}else{
		vout.Color=1;
	}
	return vout;
}

//-----------------------------------------------------------------------------
// Pixel shaders
//-----------------------------------------------------------------------------

float4 PSBasic(VertexLightingPSInput pin) : COLOR
{
	float4 color = pin.Diffuse + float4(pin.Specular.rgb, 0);
	//�X�t�B�A�}�b�v
	if(UseSphere==1)//�y���ӓ_�z�����Ńo�O�� if else�����̃V�F�[�_���܂߂ď����ƃo�O��Ȃ�
		color*=tex2D(SphereSampler,pin.SphereCoord);//�X�t�B�A�}�b�v����Z
	else if(UseSphere==2)//�X�t�B�A�}�b�v�����Z
		color+=tex2D(SphereSampler,pin.SphereCoord);
	//�g�D�[��
	if(UseToon)//������̏�������͉��̂��o�O��Ȃ�
		color*=tex2D(ToonTexSampler,pin.ToonTexCoord);
	return color;//+dummy*0;
}


float4 PSBasicTx(VertexLightingPSInputTx pin) : COLOR
{
	float4 color = tex2D(TextureSampler, pin.TexCoord) * pin.Diffuse + float4(pin.Specular.rgb, 0);
	//�X�t�B�A�}�b�v
	if(UseSphere==1)
		color*=tex2D(SphereSampler,pin.SphereCoord);//�X�t�B�A�}�b�v����Z
	else if(UseSphere==2)//�X�t�B�A�}�b�v�����Z
		color+=tex2D(SphereSampler,pin.SphereCoord);
	//�g�D�[��
	if(UseToon)
		color*=tex2D(ToonTexSampler,pin.ToonTexCoord);
	return color;
}

float4 PSEdge(float4 color : COLOR0) : COLOR
{
	return color;//�G�b�W�p�ɏ������̂܂܏o��
}

//-----------------------------------------------------------------------------
// �V�F�[�_�[
//-----------------------------------------------------------------------------

int ShaderIndex = 0;


VertexShader VSArray[4] =
{
	compile vs_2_0 VSBasicNm(),
	compile vs_2_0 VSBasicNmVc(),
	compile vs_2_0 VSBasicNmTx(),
	compile vs_2_0 VSBasicNmTxVc(),
};


PixelShader PSArray[4] =
{
	compile ps_2_0 PSBasic(),
	compile ps_2_0 PSBasic(),
	compile ps_2_0 PSBasicTx(),
	compile ps_2_0 PSBasicTx(),
};

VertexShader VSEdgeArray[4] =
{
	compile vs_2_0 VSEdgeNm(),
	compile vs_2_0 VSEdgeNm(),
	compile vs_2_0 VSEdgeNm(),
	compile vs_2_0 VSEdgeNm(),
};


PixelShader PSEdgeArray[4] =
{
	compile ps_2_0 PSEdge(),
	compile ps_2_0 PSEdge(),
	compile ps_2_0 PSEdge(),
	compile ps_2_0 PSEdge(),
};


Technique MMDEffect
{
	Pass
	{
		VertexShader = (VSArray[ShaderIndex]);
		PixelShader	 = (PSArray[ShaderIndex]);
	}
}
Technique MMDNormalDepth
{
	Pass
	{
		VertexShader =(VSEdgeArray[ShaderIndex]);
		PixelShader = (PSEdgeArray[ShaderIndex]);
	}
}
