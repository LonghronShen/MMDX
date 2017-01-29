//XBox用MMDXシェーダ
//CPUが弱いのでvfetch使ってGPUに計算させる。


//表情の実装は頂点バッファ+vfetchで行う
//各頂点に表情データ読み取り位置と個数を付けておく(Null=-1許可)
//次に表情データを読み取る。ここには各表情における頂点移動量と表情番号が書かれている

//verticesはfloat2 fvpを持ってる。fvp.x=読み取り位置、fvp.y=カウント
//facedataはfloat4 fdを持ってる。fd.xyz=表情による移動量、fd.w=表情番号

int ShaderIndex = 0;

//-----------------------------------------------------------------------------
// テスクチャ
//-----------------------------------------------------------------------------
texture Texture;		// テクスチャ
uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

//スフィアマップ使用フラグ。0:無し 1:乗算 2:加算
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
// 定数レジスタ宣言
//=============================================================================
uniform shared const float3	EyePosition;		// in world space
//-----------------------------------------------------------------------------
// マテリアル設定
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 1;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;
uniform const bool		Edge=true;

//-----------------------------------------------------------------------------
// ライト設定
//-----------------------------------------------------------------------------
uniform const float3	AmbientLightColor;
uniform const float3	DirLight0Direction;

//-----------------------------------------------------------------------------
// マトリックス
//-----------------------------------------------------------------------------
// オブジェクトのワールド座標
uniform const float4x4	World;	
// ビューのトランスフォーム
uniform const float4x4	View;
// プロジェクションのトランスフォーム
uniform const float4x4	Projection;

//表情(100種類あればいいだろ……)
uniform const float		faceRates[100];

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
// クォータニオンヘルパーメソッド
//=============================================================================
//-----------------------------------------------------------------------------
// クォータニオンヘルパーメソッド
//=============================================================================
// クォータニオンと平行移動から行列に変換する
float4x4 CreateTransformFromQuaternionTransform( float4 quaternion, float3 translation )
{
	float4 q = quaternion;
	float ww = q.w * q.w - 0.5f;
	float3 v00 = float3( ww       , q.x * q.y, q.x * q.z );
	float3 v01 = float3( q.x * q.x, q.w * q.z,-q.w * q.y );
	float3 v10 = float3( q.x * q.y, ww,        q.y * q.z );
	float3 v11 = float3(-q.w * q.z, q.y * q.y, q.w * q.x );
	float3 v20 = float3( q.x * q.z, q.y * q.z, ww        );
	float3 v21 = float3( q.w * q.y,-q.w * q.x, q.z * q.z );
	
	return float4x4(
		2.0f * ( v00 + v01 ), 0,
		2.0f * ( v10 + v11 ), 0, 
		2.0f * ( v20 + v21 ), 0,
		translation, 1
	);
}

// 複数のクォータニオン+平行移動のブレンディング処理
float4x4 BlendQuaternionTransforms(
		float4 q1, float3 t1,
		float4 q2, float3 t2,
		float2 weights )
{
	return
		CreateTransformFromQuaternionTransform(q1, t1) * weights.x +
		CreateTransformFromQuaternionTransform(q2, t2) * weights.y;
}

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
	float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;//VectorIndex.y=パレット番号
	result.Specular+=AmbientLightColor*ret.y;
	
	//MMDではEmissiveを足してからsaturateするのが正解らしい。
	result.Diffuse *= DiffuseColor;
	result.Diffuse	+= EmissiveColor;
	result.Diffuse	= saturate(result.Diffuse);
	result.Specular	*= SpecularColor;
	
	//トゥーンテクスチャ用のサンプル位置を計算
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
	
	//トゥーンテクスチャ取得位置をコピー
	vout.ToonTexCoord=lightResult.ToonTex;
	//スフィア計算
	vout.SphereCoord=float2(normal.x/2+0.5,normal.y/2+0.5);
	
	return vout;
}

//-----------------------------------------------------------------------------
// Vertex shaders
//-----------------------------------------------------------------------------
VertexLightingVSOutput VSBasicNm(int index : INDEX)
{
	float4 position;
	float4 normal;
	float4 boneIndices;
    float4 boneWeights;
	float4 facePtr;
	// vfetchはアセンブリ命令なのでasmブロックを使う必要がある
    asm
    {
		// 頂点データのフェッチ
        vfetch position,		index, position0
        vfetch normal,			index, normal0
	    vfetch boneIndices,		index, blendindices
        vfetch boneWeights,		index, blendweight
		vfetch facePtr,			index, texcoord1
    };
	//表情情報のフェッチ
	for(int i=(int)round(facePtr.x);i<(int)round(facePtr.x)+(int)round(facePtr.y);++i)
	{
		float4 faceData;
		asm
		{
			vfetch faceData,	i, texcoord4
		};
		position+=float4(faceData.xyz*saturate(faceRates[(int)round(faceData.w)]),0);
	}
	// ボーン情報のフェッチ
    float4 q1, q2;
    float4 t1, t2;
    asm
    {
        vfetch q1,	boneIndices.x, texcoord2
        vfetch q2,	boneIndices.y, texcoord2
        
        vfetch t1,	boneIndices.x, texcoord3
        vfetch t2,	boneIndices.y, texcoord3
    };
	//スキニング行列のビルド
	float4x4 skinTransform = BlendQuaternionTransforms(
					q1, t1,
					q2, t2,
					boneWeights.xy);
	//スキニング行列の適用
	float4 pos_sk = mul(position, skinTransform);
	normal.w=0;
	float4 normal_sk=normalize(mul(normal, skinTransform));

	VertexLightingVSOutput vout;
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(pos_sk, normal_sk.xyz);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}

VertexLightingVSOutput VSBasicNmVc(int index : INDEX)
{
	float4 position;
	float4 normal;
	float4 boneIndices;
    float4 boneWeights;
	float4 facePtr;
	float4 vColor;
	// vfetchはアセンブリ命令なのでasmブロックを使う必要がある
    asm
    {
		// 頂点データのフェッチ
        vfetch position,		index, position0
        vfetch normal,			index, normal0
		vfetch vColor,			index, color0
        vfetch boneIndices,		index, blendindices
        vfetch boneWeights,		index, blendweight
		vfetch facePtr,			index, texcoord1
    };
	//表情情報のフェッチ
	for(int i=(int)round(facePtr.x);i<(int)round(facePtr.x)+(int)round(facePtr.y);++i)
	{
		float4 faceData;
		asm
		{
			vfetch faceData,	i, texcoord4
		};
		position+=float4(faceData.xyz*saturate(faceRates[(int)round(faceData.w)]),0);
	}
	// ボーン情報のフェッチ
    float4 q1, q2;
    float4 t1, t2;
    asm
    {
        vfetch q1,	boneIndices.x, texcoord2
        vfetch q2,	boneIndices.y, texcoord2
        
        vfetch t1,	boneIndices.x, texcoord3
        vfetch t2,	boneIndices.y, texcoord3
    };
	//スキニング行列のビルド
	float4x4 skinTransform = BlendQuaternionTransforms(
					q1, t1,
					q2, t2,
					boneWeights.xy);
	//スキニング行列の適用
	float4 pos_sk = mul(position, skinTransform);
	normal.w=0;
	float4 normal_sk=normalize(mul(normal, skinTransform));

	VertexLightingVSOutput vout;
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(pos_sk, normal_sk.xyz);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vColor;
	vout.Specular	= float4(cout.Specular, 1);
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}

VertexLightingVSOutputTx VSBasicNmTx(int index : INDEX)
{
	float4 position;
	float4 normal;
	float4 texCoord;
	float4 boneIndices;
    float4 boneWeights;
	float4 facePtr;
	// vfetchはアセンブリ命令なのでasmブロックを使う必要がある
    asm
    {
		// 頂点データのフェッチ
        vfetch position,		index, position0
        vfetch normal,			index, normal0
		vfetch texCoord,		index, texcoord0
        vfetch boneIndices,		index, blendindices
        vfetch boneWeights,		index, blendweight
		vfetch facePtr,			index, texcoord1
    };
	//表情情報のフェッチ
	for(int i=(int)round(facePtr.x);i<(int)round(facePtr.x)+(int)round(facePtr.y);++i)
	{
		float4 faceData;
		asm
		{
			vfetch faceData,	i, texcoord4
		};
		position+=float4(faceData.xyz*saturate(faceRates[(int)round(faceData.w)]),0);
	}
	// ボーン情報のフェッチ
    float4 q1, q2;
    float4 t1, t2;
    asm
    {
        vfetch q1,	boneIndices.x, texcoord2
        vfetch q2,	boneIndices.y, texcoord2
        
        vfetch t1,	boneIndices.x, texcoord3
        vfetch t2,	boneIndices.y, texcoord3
    };
	//スキニング行列のビルド
	float4x4 skinTransform = BlendQuaternionTransforms(
					q1, t1,
					q2, t2,
					boneWeights.xy);
	//スキニング行列の適用
	float4 pos_sk = mul(position, skinTransform);
	normal.w=0;
	float4 normal_sk=normalize(mul(normal, skinTransform));

	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(pos_sk, normal_sk.xyz);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse;
	vout.Specular	= float4(cout.Specular, 1);
	vout.TexCoord	= texCoord.xy;
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}

VertexLightingVSOutputTx VSBasicNmTxVc(int index : INDEX)
{
	float4 position;
	float4 normal;
	float4 texCoord;
	float4 boneIndices;
    float4 boneWeights;
	float4 facePtr;
	float4 vColor;
	// vfetchはアセンブリ命令なのでasmブロックを使う必要がある
    asm
    {
		// 頂点データのフェッチ
        vfetch position,		index, position0
        vfetch normal,			index, normal0
		vfetch texCoord,		index, texcoord0
		vfetch vColor,			index, color0
        vfetch boneIndices,		index, blendindices
        vfetch boneWeights,		index, blendweight
		vfetch facePtr,			index, texcoord1
    };
	//表情情報のフェッチ
	for(int i=(int)round(facePtr.x);i<(int)round(facePtr.x)+(int)round(facePtr.y);++i)
	{
		float4 faceData;
		asm
		{
			vfetch faceData,	i, texcoord4
		};
		position+=float4(faceData.xyz*saturate(faceRates[(int)round(faceData.w)]),0);
	}
	// ボーン情報のフェッチ
    float4 q1, q2;
    float4 t1, t2;
    asm
    {
        vfetch q1,	boneIndices.x, texcoord2
        vfetch q2,	boneIndices.y, texcoord2
        
        vfetch t1,	boneIndices.x, texcoord3
        vfetch t2,	boneIndices.y, texcoord3
    };
	//スキニング行列のビルド
	float4x4 skinTransform = BlendQuaternionTransforms(
					q1, t1,
					q2, t2,
					boneWeights.xy);
	//スキニング行列の適用
	float4 pos_sk = mul(position, skinTransform);
	normal.w=0;
	float4 normal_sk=normalize(mul(normal, skinTransform));

	VertexLightingVSOutputTx vout;
	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(pos_sk, normal_sk.xyz);

	vout.PositionPS	= cout.Pos_ps;
	vout.Diffuse	= cout.Diffuse * vColor;
	vout.Specular	= float4(cout.Specular, 1);
	vout.TexCoord	= texCoord.xy;
	vout.ToonTexCoord = cout.ToonTexCoord;
	vout.SphereCoord=cout.SphereCoord;
	
	return vout;
}

EdgeVSOutput VSEdgeNm(int index : INDEX)
{
	float4 position;
	float4 n;
	float4 boneIndices;
    float4 boneWeights;
	float4 facePtr;
	
	// vfetchはアセンブリ命令なのでasmブロックを使う必要がある
    asm
    {
		// 頂点データのフェッチ
        vfetch position,		index, position0
        vfetch n,				index, normal0
        vfetch boneIndices,		index, blendindices
        vfetch boneWeights,		index, blendweight
		vfetch facePtr,			index, texcoord1
    };
	//表情情報のフェッチ
	for(int i=(int)round(facePtr.x);i<(int)round(facePtr.x)+(int)round(facePtr.y);++i)
	{
		float4 faceData;
		asm
		{
			vfetch faceData,	i, texcoord4
		};
		position+=float4(faceData.xyz*saturate(faceRates[(int)round(faceData.w)]),0);
	}
	// ボーン情報のフェッチ
    float4 q1, q2;
    float4 t1, t2;
    asm
    {
        vfetch q1,	boneIndices.x, texcoord2
        vfetch q2,	boneIndices.y, texcoord2
        
        vfetch t1,	boneIndices.x, texcoord3
        vfetch t2,	boneIndices.y, texcoord3
    };
	//スキニング行列のビルド
	float4x4 skinTransform = BlendQuaternionTransforms(
					q1, t1,
					q2, t2,
					boneWeights.xy);
	//スキニング行列の適用
	float4 pos_sk = mul(position, skinTransform);
	n.w=0;
	float4 normal_sk=normalize(mul(n, skinTransform));

	
	CommonVSOutput cout = ComputeCommonVSOutputWithLighting(pos_sk, normal_sk.xyz);
	EdgeVSOutput vout;
	
	vout.PositionPS	= cout.Pos_ps;
	float3 normal	= normalize(mul(normal_sk.xyz,World));
	if(Edge){
		vout.Color.rgb=(normal+1)/2;//法線を0-1の間にして保存
		vout.Color.w=vout.PositionPS.z/vout.PositionPS.w;//深度を0-1にしてアルファ値に保存
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
	//スフィアマップ
	if(UseSphere==1)//【注意点】ここでバグる if elseを下のシェーダも含めて消すとバグらない
		color*=tex2D(SphereSampler,pin.SphereCoord);//スフィアマップを乗算
	else if(UseSphere==2)//スフィアマップを加算
		color+=tex2D(SphereSampler,pin.SphereCoord);
	//トゥーン
	if(UseToon)//こちらの条件分岐は何故かバグらない
		color*=tex2D(ToonTexSampler,pin.ToonTexCoord);
	return color;//+dummy*0;
}


float4 PSBasicTx(VertexLightingPSInputTx pin) : COLOR
{
	float4 color = tex2D(TextureSampler, pin.TexCoord) * pin.Diffuse + float4(pin.Specular.rgb, 0);
	//スフィアマップ
	if(UseSphere==1)
		color*=tex2D(SphereSampler,pin.SphereCoord);//スフィアマップを乗算
	else if(UseSphere==2)//スフィアマップを加算
		color+=tex2D(SphereSampler,pin.SphereCoord);
	//トゥーン
	if(UseToon)
		color*=tex2D(ToonTexSampler,pin.ToonTexCoord);
	return color;
}

float4 PSEdge(float4 color : COLOR0) : COLOR
{
	return color;//エッジ用に情報をそのまま出力
}

//-----------------------------------------------------------------------------
// シェーダー
//-----------------------------------------------------------------------------



VertexShader VSArray[4] =
{
	compile vs_3_0 VSBasicNm(),
	compile vs_3_0 VSBasicNmVc(),
	compile vs_3_0 VSBasicNmTx(),
	compile vs_3_0 VSBasicNmTxVc(),
};


PixelShader PSArray[4] =
{
	compile ps_3_0 PSBasic(),
	compile ps_3_0 PSBasic(),
	compile ps_3_0 PSBasicTx(),
	compile ps_3_0 PSBasicTx(),
};

VertexShader VSEdgeArray[4] =
{
	compile vs_3_0 VSEdgeNm(),
	compile vs_3_0 VSEdgeNm(),
	compile vs_3_0 VSEdgeNm(),
	compile vs_3_0 VSEdgeNm(),
};


PixelShader PSEdgeArray[4] =
{
	compile ps_3_0 PSEdge(),
	compile ps_3_0 PSEdge(),
	compile ps_3_0 PSEdge(),
	compile ps_3_0 PSEdge(),
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
