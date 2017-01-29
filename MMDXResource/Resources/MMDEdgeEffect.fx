//-----------------------------------------------------------
// MMDEdgeEffect.fx
//
// MMDX
// Copyright (C) Wilfrem
//-----------------------------------------------------------

//-----------------------------------------------------------------------------
// �G�t�F�N�g�ݒ�l
//-----------------------------------------------------------------------------
float EdgeWidth=0.1;//�G�b�W��
float EdgeSensitivity=1;//�G�b�W���ǂ̂��炢�Â����邩
float NormalThreashold=1.25;//�@�����̌��o���E
float DepthThreashold=0.1;//�f�v�X���̌��o���E
float NormalSensitivity=1;//�@�����ɂ��ǂ̈ʃG�b�W���Â����邩
float DepthSensitivity=10;//�f�v�X���ɂ��ǂ̈ʃG�b�W���Â����邩

//�𑜓x
float2 ScreenResolution;

//-----------------------------------------------------------------------------
// �e�X�N�`��
//-----------------------------------------------------------------------------
texture Texture;		// �e�N�X�`��
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
	//�G�b�W�f�[�^�̃T���v���ʒu���Y�����I�t�Z�b�g���v�Z
	float2 offset = EdgeWidth/ScreenResolution;
	//�T���v�����O
	float4 sample1=tex2D(EdgeSampler,texCoord+float2(-1,-1)*offset);
	float4 sample2=tex2D(EdgeSampler,texCoord+float2(1,1)*offset);
	float4 sample3=tex2D(EdgeSampler,texCoord+float2(1,-1)*offset);
	float4 sample4=tex2D(EdgeSampler,texCoord+float2(-1,1)*offset);
	//return float4(sample1.x,sample1.y,sample1.z,1);
	//�T���v�����O�ɔ�G�b�W�t�B���^(x,y,z��1)�������Ă�����G�b�W��`�悵�Ȃ�
	if(!any(sample1-1) || !any(sample2-1) || !any(sample3-1) || !any(sample4-1))
	{
		return 0;
	}
	//�@���Ɛ[�x�̕ω��ʂ��擾
	float4 delta=abs(sample1-sample2)+abs(sample3-sample4);
	float normalDelta=dot(delta.xyz,1);//xyz...�����オ�����B���ČÂ����B
	float depthDelta=delta.w;
	//�����ω��ʂ��t�B���^���A�ω��ʂ�Sensitivity�ő傫������
	normalDelta=saturate((normalDelta-NormalThreashold)*NormalSensitivity);
	depthDelta=saturate((depthDelta-DepthThreashold)*DepthSensitivity);
	//�ŏI�I�ȃG�b�W�̔Z�����擾
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
