using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Model.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MikuMikuDance for XNA用に拡張したモデル
    /// </summary>
    public class MMDXModel : MMDModel
    {
#if XBOX
        readonly MMDXBoxBoneManager boneManager;
        WritableVertexBuffer skinVertexBuffer = null;
        internal DynamicVertexBuffer SkinVertBuffer { get { return skinVertexBuffer.VertexBuffer; } }
        internal int BufferOffset { get; private set; }
        readonly MMDXBoxFaceManager faceManager;
        internal MMDXBoxFaceManager FaceManagerXBox { get { return faceManager; } }
#endif
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modelParts">モデルパーツ</param>
        /// <param name="boneManager">ボーンマネージャ</param>
        /// <param name="faceManager">表情マネージャ</param>
        /// <param name="attachedMotion">付属モーション</param>
        /// <param name="rigids">剛体</param>
        /// <param name="joints">ジョイント</param>
        public MMDXModel(List<IMMDModelPart> modelParts, MMDBoneManager boneManager, IMMDFaceManager faceManager, Dictionary<string, MMDMotion> attachedMotion, MMDRigid[] rigids, MMDJoint[] joints)
            : base(modelParts, boneManager, faceManager, attachedMotion, rigids, joints)
        {
#if XBOX
            //ボーンマネージャ・表情マネージャを変換しておく
            this.boneManager = (MMDXBoxBoneManager)boneManager;
            this.faceManager = (MMDXBoxFaceManager)faceManager;
            //グラフィックデバイスの抜き出し
            if (modelParts.Count > 0)
            {
                GraphicsDevice graphics = ((MMDModelPart)modelParts[0]).GraphicsDevice;
                //頂点バッファの作成
                skinVertexBuffer = new WritableVertexBuffer(graphics, this.boneManager.SKinTransformXBox.Length * 4, typeof(VertexSkinning));
                this.faceManager.SetUp(graphics);
            }
#endif
        }
        /// <summary>
        /// 表情をモデルに適用
        /// </summary>
        protected override void SetFace()
        {
            for (int i = 0; i < Parts.Count;++i )
            {
                Parts[i].SetFace(FaceManager);
            }
        }
        /// <summary>
        /// スキニング行列をモデルに適用
        /// </summary>
        protected override void SetBone()
        {
#if !XBOX
            System.Threading.Tasks.Parallel.ForEach(Parts, (part) => part.SetSkinMatrix(BoneManager.SkinTransforms));
#else
            //スキニング行列をバッファに書き込み
            if (skinVertexBuffer != null)
                BufferOffset = skinVertexBuffer.SetData(boneManager.SKinTransformXBox);
#endif

        }
    }
}
