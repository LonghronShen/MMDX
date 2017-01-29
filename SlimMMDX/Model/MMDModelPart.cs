using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using SlimDX;
using SlimDX.Direct3D9;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.SlimDX.Model
{
    /// <summary>
    /// モデルパーツ
    /// </summary>
    public class MMDModelPart : IMMDModelPart
    {
        /// <summary>
        /// このパーツに関連付けられているモデル
        /// </summary>
        protected SlimMMDModel model;
        /// <summary>
        /// エフェクト
        /// </summary>
        protected Effect effect;
        internal IndexBuffer indexbuffer = null;
        /// <summary>
        /// 頂点数
        /// </summary>
        protected int vertexCount;
        /// <summary>
        /// 頂点開始位置
        /// </summary>
        protected int startIndex;
        /// <summary>
        /// ポリゴン数
        /// </summary>
        protected int triangleCount;
        /// <summary>
        /// モデルパーツ
        /// </summary>
        /// <param name="vertexCount">頂点数</param>
        /// <param name="startIndex">頂点開始位置</param>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="effect">エフェクト</param>
        /// <param name="indexbuffer">インデックスバッファ</param>
        public MMDModelPart(int vertexCount,int startIndex, int triangleCount, Effect effect,IndexBuffer indexbuffer)
        {
            this.vertexCount = vertexCount;
            this.triangleCount = triangleCount;
            this.startIndex = startIndex;
            this.effect = effect;
            this.indexbuffer = indexbuffer;
        }
        #region IMMDModelPart メンバー

        /// <summary>
        /// モデル追加時に呼ばれる
        /// </summary>
        /// <param name="model">親モデル</param>
        public void SetModel(MMDModel model)
        {
            this.model = (SlimMMDModel)model;
        }
        /// <summary>
        /// エフェクトにマトリックスを適用
        /// </summary>
        /// <param name="mode">描画モード</param>
        /// <param name="world">ワールド</param>
        public virtual void SetParams(MMDDrawingMode mode, ref Matrix world)
        {
            Matrix view, projection;
            //カメラ情報の取得
            Viewport viewport = effect.Device.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;
            SlimMMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out view, out projection);

            //マトリクス処理
            effect.SetValue("World", world);
            effect.SetValue("View", view);
            effect.SetValue("Projection", projection);
            effect.SetValue("EyePosition", SlimMMDXCore.Instance.Camera.Position);
            //ライティング処理
            Vector3 color, dir;
            SlimMMDXCore.Instance.Light.GetLightParam(out color, out dir);
            effect.SetValue("AmbientLightColor", color);
            effect.SetValue("DirLight0Direction", dir);
            switch (mode)
            {
                case MMDDrawingMode.Normal:
                    effect.Technique = "MMDEffect";
                    break;
                case MMDDrawingMode.Edge:
                    effect.Technique = "MMDNormalDepth";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// スキニング行列をモデルに適用
        /// </summary>
        /// <param name="skinmatrix">スキニング行列</param>
        public virtual void SetSkinMatrix(Matrix[] skinmatrix) { }//こちらではなにもしない
        /// <summary>
        /// 表情をモデルに適用
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        public virtual void SetFace(IMMDFaceManager faceManager) { }//こちらではなにもしない
        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="mode">モデル描画モード</param>
        public virtual void Draw(MMDDrawingMode mode)
        {
            SlimMMDXCore.Instance.Device.Indices = indexbuffer;
            effect.Begin();
            effect.BeginPass(0);
            SlimMMDXCore.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, startIndex, triangleCount);
            effect.EndPass();
            effect.End();
        }

        #endregion

        #region IDisposable メンバー
        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose()
        {
            effect.Dispose();
            indexbuffer.Dispose();
        }

        #endregion

        internal void OnLostDevice()
        {
            effect.OnLostDevice();
        }
        internal void OnResetDevice()
        {
            effect.OnResetDevice();
        }
    }
}
