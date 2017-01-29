using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Model.Physics;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;
using SlimDX;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.SlimDX.Misc;
using System.Collections.ObjectModel;

namespace MikuMikuDance.SlimDX.Model
{
    /// <summary>
    /// SlimDX用のモデル
    /// </summary>
    public class SlimMMDModel : MMDModel
    {
        MMDVertexNmTx[] m_vertex;
        VertexPNmTx[] verticesSource;
        VertexBuffer vertexBuffer;
        VertexDeclaration vertexDec;

        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="vertex">頂点データ</param>
        /// <param name="modelParts">モデルパーツ</param>
        /// <param name="boneManager">ボーンマネージャ</param>
        /// <param name="faceManager">表情マネージャ</param>
        /// <param name="attachedMotion">付随モーション</param>
        /// <param name="rigids">剛体情報</param>
        /// <param name="joints">関節情報</param>
        public SlimMMDModel(MMDVertexNmTx[] vertex, List<IMMDModelPart> modelParts, MMDBoneManager boneManager, MMDFaceManager faceManager, Dictionary<string, MMDMotion> attachedMotion, MMDRigid[] rigids, MMDJoint[] joints)
            : base(modelParts, boneManager, faceManager, attachedMotion, rigids, joints)
        {
            m_vertex = vertex;
            //データのコピー
            verticesSource = new VertexPNmTx[m_vertex.LongLength];
            for (long i = 0; i < m_vertex.LongLength; ++i)
            {
                verticesSource[i].Position = m_vertex[i].Position;
                verticesSource[i].Normal = m_vertex[i].Normal;
                verticesSource[i].Texture = m_vertex[i].TextureCoordinate;
            }

            InitGraphicsResource();

            SlimMMDXCore.Instance.LostDevice += OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice += OnResetDevice;
        }
        void InitGraphicsResource()
        {
            vertexBuffer = new VertexBuffer(SlimMMDXCore.Instance.Device, verticesSource.Length * Marshal.SizeOf(typeof(VertexPNmTx)), Usage.Dynamic, VertexFormat.None, Pool.Default);
            DataStream stream = vertexBuffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(verticesSource);
            vertexBuffer.Unlock();
            vertexDec = new VertexDeclaration(SlimMMDXCore.Instance.Device, VertexPNmTx.VertexElements);
        
        }
        /// <summary>
        /// ロストデバイス時に呼び出される
        /// </summary>
        protected void OnLostDevice()
        {
            vertexBuffer.Dispose();
            vertexBuffer = null;
            vertexDec.Dispose();
            vertexDec = null;
            foreach (MMDModelPart part in Parts)
                part.OnLostDevice();
        }
        /// <summary>
        /// デバイスリセット時に呼び出される
        /// </summary>
        protected void OnResetDevice()
        {
            InitGraphicsResource();
            foreach (MMDModelPart part in Parts)
                part.OnResetDevice();
        }

        /// <summary>
        /// 各パーツを描画する前に呼ばれる
        /// </summary>
        protected override void BeforeDraw(MMDDrawingMode mode)
        {
            //頂点バッファの更新
            DataStream stream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
            stream.WriteRange(verticesSource);
            vertexBuffer.Unlock();
            
            //頂点バッファのセット処理
            SlimMMDXCore.Instance.Device.VertexDeclaration = vertexDec;
            SlimMMDXCore.Instance.Device.SetStreamSource(0, vertexBuffer, 0, Marshal.SizeOf(typeof(VertexPNmTx)));
            
            //カリング

            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.CullMode, Culling ? Cull.Counterclockwise : Cull.None);
            //透過設定
            switch (mode)
            {
                case MMDDrawingMode.Normal:
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaTestEnable, true);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaRef, 0);
                    break;
                case MMDDrawingMode.Edge:
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaBlendEnable, false);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaFunc, Compare.Always);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaTestEnable, false);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaRef, 0);
                    break;
                default:
                    throw new NotImplementedException();
            }
            base.BeforeDraw(mode);
        }
        /// <summary>
        /// 表情をモデルに適用
        /// </summary>
        protected override void SetFace()
        {
            MMDFaceManager.ApplyToVertex((MMDFaceManager)FaceManager, m_vertex);
        }
        /// <summary>
        /// スキニング行列をモデルに適用
        /// </summary>
        protected override void SetBone()
        {
            System.Threading.Tasks.Parallel.For(0, m_vertex.Length,
                (i) => {
                        Vector4 pos;
                        SkinningHelpers.SkinVertex(BoneManager.SkinTransforms, m_vertex[i], out pos, out verticesSource[i].Normal);
                        verticesSource[i].Position = new Vector3(pos.X, pos.Y, pos.Z);
                }
                );
            //base.SetBone(skinTransforms);//こっちでは呼ばない
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public override void Dispose()
        {
            SlimMMDXCore.Instance.LostDevice -= OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice -= OnResetDevice;
            vertexBuffer.Dispose();
            vertexBuffer = null;
            vertexDec.Dispose();
            vertexDec = null;
            base.Dispose();
        }
    }
}
