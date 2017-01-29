//-----------------------------------------------------------------------------
// AccessoryEffect.fx
// アクセサリ用のエフェクト。
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// テスクチャ
//-----------------------------------------------------------------------------
texture Texture;		// テクスチャ
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

//スフィアマップ使用フラグ。0:無し 1:乗算 2:加算
int UseSphere;
texture Sphere;
sampler SphereSampler : register(s1) = sampler_state
{
	Texture = (Sphere);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};


//-----------------------------------------------------------------------------
// 定数レジスタ宣言
//=============================================================================
float3	EyePosition;		// in world space

//-----------------------------------------------------------------------------
// ライト設定
//-----------------------------------------------------------------------------
float3	AmbientLightColor;
float3	DirLight0Direction;

//-----------------------------------------------------------------------------
// 変換行列
//-----------------------------------------------------------------------------

float4x4	World		;	// 12 - 15
float4x4	View		;	// 16 - 19
float4x4	Projection	;	// 20 - 23

//-----------------------------------------------------------------------------
// マテリアル設定
//-----------------------------------------------------------------------------
float3	DiffuseColor = 1;
float	Alpha = 1;
float3	EmissiveColor = 0;
float3	SpecularColor = 1;
float	SpecularPower = 16;
bool	Edge=false;


//-----------------------------------------------------------------------------
// 構造体出力
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

struct CommonVSOutput
{
	float4	Pos_ws;
	float4	Pos_ps;
	float4	Diffuse;
	float3	Specular;
};
struct CommonVSOutputSd
{
	float4	Pos_ws;
	float4	Pos_ps;
	float4	Diffuse;
	float3	Specular;
	float	FogFactor;
};

// Nm: 法線
// Tx: テクスチャ
// Vc: 頂点カラー
//
// Nm Tx Vc
//  0  0  0	VSInput
//  0  0  1 VSInputVc
//  0  1  0 VSInputTx
//  0  1  1 VSInputTxVc
//  1  0  0 VSInputNm
//  1  0  1 VSInputNmVc
//  1  1  0 VSInputNmTx
//  1  1  1 VSInputNmTxVc


//-----------------------------------------------------------------------------
// 頂点シェーダー入力構造体
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
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
};

struct VSInputNmTxVc
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
	float4	Color		: COLOR;
};


//-----------------------------------------------------------------------------
// 頂点シェーダ出力
//-----------------------------------------------------------------------------

struct VertexLightingVSOutput
{
	float4	PositionPS	: POSITION;		// プロジェクション済み頂点位置
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;		// Specular.rgbとfog factor
};

struct VertexLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// プロジェクション済み頂点位置
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
};

//スフィアマップ
struct VertexLightingVSOutputSp
{
	float4	PositionPS	: POSITION;		// プロジェクション済み頂点位置
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;		// Specular.rgb と fog factor
	float3  NormalWS	: TEXCOORD1;	// 法線
};

struct VertexLightingVSOutputTxSp
{
	float4	PositionPS	: POSITION;		// プロジェクション済み頂点位置
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
	float3  NormalWS	: TEXCOORD1;	// 法線
};

//スフィアマップ
struct VertexLightingVSOutputSpSd
{
	float4	PositionPS	: POSITION;		// プロジェクション済み頂点位置
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;		// Specular.rgb と fog factor
	float3  NormalWS	: TEXCOORD1;	// 法線
	float4	Pos_Light	: TEXCOORD3;	// 光源からみた頂点位置
};

struct VertexLightingVSOutputTxSpSd
{
	float4	PositionPS	: POSITION;		// プロジェクション済み頂点位置
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
	float3  NormalWS	: TEXCOORD1;	// 法線
	float4	Pos_Light	: TEXCOORD3;	// 光源からみた頂点位置
};
struct EdgeVSOutput
{
	float4	PositionPS	: POSITION;
	float4	Color		: COLOR0;
};

//-----------------------------------------------------------------------------
// ピクセルシェーダ入力構造体
//-----------------------------------------------------------------------------

struct VertexLightingPSInput
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
};

struct VertexLightingPSInputTx
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
};

//スフィアマップ
struct VertexLightingPSInputSp
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float3	NormalWS	: TEXCOORD1;
};

struct VertexLightingPSInputTxSp
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
};

//スフィアマップ
struct VertexLightingPSInputSpSd
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float3	NormalWS	: TEXCOORD1;
	float4	Pos_Light	: TEXCOORD3;	// 光源からみた頂点位置
};

struct VertexLightingPSInputTxSpSd
{
	float4	Diffuse		: COLOR0;
	float4	Specular	: COLOR1;
	float2	TexCoord	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
	float4	Pos_Light	: TEXCOORD3;	// 光源からみた頂点位置
};

//-----------------------------------------------------------------------------
// ライティング計算
// E: 視線ベクトル
// N: ワールド座標系における単位法線ベクトル
//-----------------------------------------------------------------------------
ColorPair ComputeLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;

	
	// Directional Light 0
	float3 L = normalize(-DirLight0Direction);
	float3 H = normalize(E+L);
	float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Specular+=AmbientLightColor*ret.y;
	
	//MMDではEmissiveを足してからsaturateするのが正解らしい。
	result.Diffuse *= DiffuseColor;
	result.Diffuse	+= EmissiveColor;
	result.Diffuse	= saturate(result.Diffuse);
	result.Specular	*= SpecularColor;
	
	return result;
}

CommonVSOutput ComputeCommonVSOutput(float4 position)
{
	CommonVSOutput vout;
	
	float4 pos_ws = mul(position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	vout.Pos_ws = pos_ws;
	vout.Pos_ps = pos_ps;
	
	vout.Diffuse	= float4(DiffuseColor.rgb + EmissiveColor, Alpha);
	vout.Specular	= 0;
	
	return vout;
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
	float3 posToEye = EyePosition - pos_ws.xyz;
	float3 E = normalize(posToEye);
	ColorPair lightResult = ComputeLights(E, N);
	
	vout.Diffuse	= float4(lightResult.Diffuse.rgb, Alpha);
	vout.Specular	= lightResult.Specular;
	
	return vout;
}


//-----------------------------------------------------------------------------
// 頂点シェーダ関数
//-----------------------------------------------------------------------------

VertexLightingVSOutput VSBasicNm(VSInputNm vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	
	return vout;
}


VertexLightingVSOutput VSBasicNmVc(VSInputNmVc vin)
{
	VertexLightingVSOutput vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, 1);
	
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
	
	return vout;
}
//スフィアマップ
VertexLightingVSOutputSp VSBasicNmSp(VSInputNm vin)
{
	VertexLightingVSOutputSp vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	vout.NormalWS	= normalize(mul(vin.Normal, World));
	
	return vout;
}


VertexLightingVSOutputSp VSBasicNmVcSp(VSInputNmVc vin)
{
	VertexLightingVSOutputSp vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, 1);
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	
	return vout;
}
VertexLightingVSOutputTxSp VSBasicNmTxSp(VSInputNmTx vin)
{
	VertexLightingVSOutputTxSp vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	vout.TexCoord	= vin.TexCoord;
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	
	return vout;
}


VertexLightingVSOutputTxSp VSBasicNmTxVcSp(VSInputNmTxVc vin)
{
	VertexLightingVSOutputTxSp vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vin.Color;
	vout.Specular	= float4(cout.Specular, 1);
	vout.TexCoord	= vin.TexCoord;
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	
	return vout;
}

EdgeVSOutput VSEdgeNm(VSInputNm vin)
{
	EdgeVSOutput vout;
	
	vout.PositionPS	= mul(mul(mul(vin.Position,World),View),Projection);
	float3 normal	= normalize(mul(vin.Normal,World));
	if(Edge){
		vout.Color.rgb=(normal+1)/2;//法線を0-1の間にして保存
		vout.Color.w=vout.PositionPS.z/vout.PositionPS.w;//深度を0-1にしてアルファ値に保存
	}else{
		vout.Color=1;
	}
	return vout;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------

float4 PSBasic(VertexLightingPSInput pin) : COLOR
{
	float4 color = pin.Diffuse + float4(pin.Specular.rgb, 0);
	return color;
}


float4 PSBasicTx(VertexLightingPSInputTx pin) : COLOR
{
	float4 color = tex2D(TextureSampler, pin.TexCoord) * pin.Diffuse + float4(pin.Specular.rgb, 0);
	return color;
}

//スフィアマップ
float4 PSBasicSp(VertexLightingPSInputSp pin) : COLOR
{
	float4 color = pin.Diffuse + float4(pin.Specular.rgb, 0);
	//スフィアマップ計算
	float3 N = normalize(pin.NormalWS);
	float2 SphereCoord = float2(N.x*0.5f+0.5f,N.y*0.5f+0.5f);
	if(UseSphere==1)
		color*=tex2D(SphereSampler,SphereCoord);//スフィアマップを乗算
	else if(UseSphere==2)
		color+=tex2D(SphereSampler,SphereCoord);//スフィアマップを加算
	return color;
}


float4 PSBasicTxSp(VertexLightingPSInputTxSp pin) : COLOR
{
	float4 color = tex2D(TextureSampler, pin.TexCoord) * pin.Diffuse + float4(pin.Specular.rgb, 0);
	//スフィアマップ計算
	float3 N = normalize(pin.NormalWS);
	float2 SphereCoord = float2(N.x*0.5f+0.5f,N.y*0.5f+0.5f);
	if(UseSphere==1)
		color*=tex2D(SphereSampler,SphereCoord);//スフィアマップを乗算
	else if(UseSphere==2)
		color+=tex2D(SphereSampler,SphereCoord);//スフィアマップを加算
	return color;
}


float4 PSEdge(float4 color : COLOR0) : COLOR
{
	return color;//エッジ用に情報をそのまま出力
}



//-----------------------------------------------------------------------------
// シェーダテクニック定義
//-----------------------------------------------------------------------------

int ShaderIndex = 0;


VertexShader VSArray[8] =
{
	compile vs_2_0 VSBasicNm(),
	compile vs_2_0 VSBasicNmVc(),
	compile vs_2_0 VSBasicNmTx(),
	compile vs_2_0 VSBasicNmTxVc(),
	
	
	//スフィアマップ
	compile vs_2_0 VSBasicNmSp(),
	compile vs_2_0 VSBasicNmVcSp(),
	compile vs_2_0 VSBasicNmTxSp(),
	compile vs_2_0 VSBasicNmTxVcSp(),
	
	
	
	
};


PixelShader PSArray[8] =
{
	compile ps_2_0 PSBasic(),
	compile ps_2_0 PSBasic(),
	compile ps_2_0 PSBasicTx(),
	compile ps_2_0 PSBasicTx(),
	
	//スフィアマップ
	compile ps_2_0 PSBasicSp(),
	compile ps_2_0 PSBasicSp(),
	compile ps_2_0 PSBasicTxSp(),
	compile ps_2_0 PSBasicTxSp(),
	
	
	
	
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
		VertexShader = (compile vs_2_0 VSEdgeNm());
		PixelShader = (compile ps_2_0 PSEdge());
	}
}
