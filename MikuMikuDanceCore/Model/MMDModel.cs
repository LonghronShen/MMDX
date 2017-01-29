using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Motion;
using System.Collections.ObjectModel;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.MultiThreads;
using MikuMikuDance.Core.Model.Physics;
using MikuMikuDance.Core.Accessory;

#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif
#if !XNA
using System.Drawing;
#endif

namespace MikuMikuDance.Core.Model
{
    /// <summary>
    /// MMDモデルクラス
    /// </summary>
    public abstract class MMDModel : IDisposable
    {
        private readonly List<IMMDModelPart> modelParts;
        //private readonly SkinningData skinningData;
        readonly MMDBoneManager boneManager;
        readonly Dictionary<string, MMDMotion> attachedMotion;
        private AnimationPlayer animationPlayer;
        readonly PhysicsManager physicsManager;
        private IMMDFaceManager faceManager;

        /// <summary>
        /// ボーンマネージャ
        /// </summary>
        public MMDBoneManager BoneManager { get { return boneManager; } }
        /// <summary>
        /// フェースマネージャ
        /// </summary>
        public IMMDFaceManager FaceManager { get { return faceManager; } }
        /// <summary>
        /// アニメーションプレイヤー
        /// </summary>
        public AnimationPlayer AnimationPlayer { get { return animationPlayer; } }

        /// <summary>
        /// このモデルのメッシュパーツ
        /// </summary>
        public ReadOnlyCollection<IMMDModelPart> Parts { get; protected set; }

        /// <summary>
        /// 物理マネージャ
        /// </summary>
        public PhysicsManager PhysicsManager { get { return physicsManager; } }
        /// <summary>
        /// このモデルのワールド座標
        /// </summary>
        public Matrix Transform = Matrix.Identity;
        /// <summary>
        /// カリングを行うか
        /// </summary>
        /// <remarks>エッジの描画ロジックが本家と違うため、一部モデルでカリングをオフにする必要がある</remarks>
        public bool Culling { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modelParts">モデルパーツ</param>
        /// <param name="boneManager">ボーンマネージャ</param>
        /// <param name="faceManager">表情マネージャ</param>
        /// <param name="attachedMotion">付随モーション</param>
        /// <param name="rigids">剛体情報</param>
        /// <param name="joints">関節情報</param>
        public MMDModel(List<IMMDModelPart> modelParts, MMDBoneManager boneManager, IMMDFaceManager faceManager, Dictionary<string, MMDMotion> attachedMotion, MMDRigid[] rigids, MMDJoint[] joints)
        {
            Transform = Matrix.Identity;
            this.modelParts = modelParts;
            this.boneManager = boneManager;
            this.faceManager = faceManager;
            this.attachedMotion = attachedMotion;
            Culling = true;
            boneManager.CalcGlobalTransform();
            this.physicsManager = new PhysicsManager(rigids, joints, this);

            foreach (var part in modelParts)
                part.SetModel(this);
            Parts = new ReadOnlyCollection<IMMDModelPart>(modelParts);
            animationPlayer = new AnimationPlayer(boneManager, faceManager);

            //イベントフック
            MMDCore.Instance.OnBoneUpdate += new Action<float>(BoneUpdate);
            MMDCore.Instance.OnSkinUpdate += new Action<float>(SkinUpdate);
        }
        /// <summary>
        /// ボーンの更新処理
        /// </summary>
        /// <remarks>MMDXCoreから呼ばれるので呼ぶ必要はない</remarks>
        public void BoneUpdate(float elapsedSeconds)
        {
            MMDXProfiler.BeginMark("ModelUpdate", MMDXMath.CreateColor(20, 255, 0));
            //アニメーションをボーンに適用
            AnimationPlayer.Update(elapsedSeconds);
            //ボーンのグローバル行列更新
            BoneManager.CalcGlobalTransform();
            //IK更新
            BoneManager.CalcIK();
        }
        /// <summary>
        /// 頂点の更新処理
        /// </summary>
        /// <remarks>MMDXCoreから呼ばれるので呼ぶ必要はない</remarks>
        public void SkinUpdate(float elapsedSeconds)
        {
            //物理更新(シングルスレッド用)
            if (!PhysicsThreadManager.Instanse.IsMultiThread)
                PhysicsManager.Update();
            //表情適用
            MMDXProfiler.BeginMark("ModelPart.SetFace", MMDXMath.CreateColor(60, 65, 0));
            FaceManager.Update();
            SetFace();
            MMDXProfiler.EndMark("ModelPart.SetFace");
            MMDXProfiler.BeginMark("ModelPart.SetBone", MMDXMath.CreateColor(60, 255, 0));
            //スキンの更新
            BoneManager.CalcSkinTransform();
            //ボーントランスフォーム適用
            SetBone();
            MMDXProfiler.EndMark("ModelPart.SetBone");
            MMDXProfiler.EndMark("ModelUpdate");
        }
        /// <summary>
        /// 表情をモデル頂点に適用
        /// </summary>
        protected abstract void SetFace();
        /// <summary>
        /// スキニング行列を頂点に適用
        /// </summary>
        protected abstract void SetBone();
        /// <summary>
        /// モデルの描画
        /// </summary>
        public void Draw()
        {
            MMDDrawingMode mode = MMDDrawingMode.Normal;
            if (MMDCore.Instance.EdgeManager != null && MMDCore.Instance.EdgeManager.IsEdgeDetectionMode)
            {
                mode = MMDDrawingMode.Edge;
            }
            BeforeDraw(mode);
            foreach (var part in modelParts)
            {
                part.SetParams(mode, ref Transform);
                part.Draw(mode);
            }
            AfterDraw(mode);
        }
        /// <summary>
        /// 各パーツを描画する前に呼ばれる
        /// </summary>
        /// <param name="mode">モデル描画モード</param>
        protected virtual void BeforeDraw(MMDDrawingMode mode) { }
        /// <summary>
        /// 各パーツを描画した後に呼ばれる
        /// </summary>
        /// <param name="mode">モデル描画モード</param>
        protected virtual void AfterDraw(MMDDrawingMode mode) { }

        /// <summary>
        /// アクセサリーをバインドする
        /// </summary>
        /// <param name="accessory">アクセサリー</param>
        /// <param name="bonename">ボーン名</param>
        /// <param name="transform">トランスフォーム</param>
        public void BindAccessory(MMDAccessoryBase accessory, string bonename, Matrix transform)
        {
            BindAccessory(accessory, new MMD_VAC { BoneName = bonename, Transform = transform });
        }
        /// <summary>
        /// アクセサリーをバインドする
        /// </summary>
        /// <param name="accessory">アクセサリー</param>
        /// <param name="vac">MikuMikuDance VACデータ</param>
        public void BindAccessory(MMDAccessoryBase accessory, MMD_VAC vac)
        {
            accessory.VAC = vac;
            accessory.Model = this;
        }

        #region IDisposable メンバー
        bool disposed = false;
        /// <summary>
        /// モデルの破棄
        /// </summary>
        public virtual void Dispose()
        {
            if (!disposed)
            {
                //イベントをアンフック
                MMDCore.Instance.OnBoneUpdate -= new Action<float>(BoneUpdate);
                MMDCore.Instance.OnSkinUpdate -= new Action<float>(SkinUpdate);
                foreach (var part in modelParts)
                {
                    part.Dispose();
                }
                physicsManager.Dispose();
                disposed = true;
            }
        }

        #endregion
    }
}
