//-----------------------------------------------------------
// MMDEdgeEffect.fx
//
// MMDX
// Copyright (C) Wilfrem
//-----------------------------------------------------------

//-----------------------------------------------------------------------------
// エフェクト設定値
//-----------------------------------------------------------------------------
float EdgeWidth=0.1;//エッジ幅
float EdgeSensitivity=1;//エッジをどのぐらい暗くするか
float NormalThreashold=1.25;//法線差の検出限界
float DepthThreashold=0.1;//デプス差の検出限界
float NormalSensitivity=1;//法線差によりどの位エッジを暗くするか
float DepthSensitivity=10;//デプス差によりどの位エッジを暗くするか

//解像度
float2 ScreenResolution;

//-----------------------------------------------------------------------------
// テスクチャ
//-----------------------------------------------------------------------------
texture Texture;		// テクスチャ
sampler EdgeSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;

	AddressU = Clamp;
	AddressV = Clamp;
};

float4 PSDrawEdge(float2 texCoord : TEXCOORD0) : COLOR
{
	//エッジデータのサンプル位置をズラすオフセットを計算
	float2 offset = EdgeWidth/ScreenResolution;
	//サンプリング
	float4 sample1=tex2D(EdgeSampler,texCoord+float2(-1,-1)*offset);
	float4 sample2=tex2D(EdgeSampler,texCoord+float2(1,1)*offset);
	float4 sample3=tex2D(EdgeSampler,texCoord+float2(1,-1)*offset);
	float4 sample4=tex2D(EdgeSampler,texCoord+float2(-1,1)*offset);
	//return float4(sample1.x,sample1.y,sample1.z,1);
	//サンプリングに非エッジフィルタ(x,y,zが1)が入っていたらエッジを描画しない
	if(!any(sample1-1) || !any(sample2-1) || !any(sample3-1) || !any(sample4-1))
	{
		return 0;
	}
	//法線と深度の変化量を取得
	float4 delta=abs(sample1-sample2)+abs(sample3-sample4);
	float normalDelta=dot(delta.xyz,1);//xyz...もう後が無い。って古いか。
	float depthDelta=delta.w;
	//微小変化量をフィルタし、変化量をSensitivityで大きくする
	normalDelta=saturate((normalDelta-NormalThreashold)*NormalSensitivity);
	depthDelta=saturate((depthDelta-DepthThreashold)*DepthSensitivity);
	//最終的なエッジの濃さを取得
	float edgeAmount=saturate(normalDelta+depthDelta)*EdgeSensitivity;
	//return sample1;
	return float4(0,0,0,edgeAmount);
}

Technique MMDEdgeEffect
{
	Pass
	{
		PixelShader = compile ps_2_0 PSDrawEdge();
	}
}
