using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Misc;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// モデルパーツ
    /// </summary>
    public abstract class MMDModelPart : IMMDModelPart
    {
        /// <summary>
        /// XNA Effect (内部バッキング)
        /// </summary>
        protected Microsoft.Xna.Framework.Graphics.Effect xnaEffect;
        /// <summary>
        /// XNA グラフィックデバイス (内部描画用)
        /// </summary>
        protected Microsoft.Xna.Framework.Graphics.GraphicsDevice xnaGraphicsDevice;
        /// <summary>
        /// このパーツに関連付けられているモデル
        /// </summary>
        protected MMDXModel model;
        /// <summary>
        /// モデルのトライアングル数
        /// </summary>
        protected readonly int triangleCount;
        /// <summary>
        /// モデルの頂点数
        /// </summary>
        protected readonly int vertexCount;
        /// <summary>
        /// インデックスバッファ (抽象)
        /// </summary>
        protected IMMDIndexBuffer indexBuffer;
        /// <summary>
        /// 頂点バッファ (抽象)
        /// </summary>
        protected IMMDVertexBuffer vertexBuffer;
        /// <summary>
        /// グラフィックデバイス抽象
        /// </summary>
        protected IMMDGraphicsDevice graphicsDevice;
        //protected DynamicVertexBuffer vertexBuffer;//このフィールドは継承先で代入
        /// <summary>
        /// 現在の頂点バッファのオフセット
        /// </summary>
        protected int vertexOffset;
        /// <summary>
        /// 元の頂点番号との対応表
        /// </summary>
        protected Dictionary<long, int[]> VertMap;
        /// <summary>
        /// エフェクト (抽象)
        /// </summary>
        public IMMDEffect Effect { get; internal set; }
        /// <summary>
        /// このパーツに関連付けられているXNAグラフィックデバイス
        /// </summary>
        public Microsoft.Xna.Framework.Graphics.GraphicsDevice XnaGraphicsDevice { get { return xnaGraphicsDevice; } }

        static Microsoft.Xna.Framework.Graphics.BlendState ModelBlendState = Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied;
        static Microsoft.Xna.Framework.Graphics.BlendState EdgeBlendState = Microsoft.Xna.Framework.Graphics.BlendState.Opaque;
        /// <summary>
        /// レンダリングモード設定
        /// </summary>
        /// <param name="mode">描画モード</param>
        /// <param name="Culling">カリングを行うか</param>
        /// <param name="device">グラフィックデバイス</param>
        protected static void SetUpRenderState(MMDDrawingMode mode, bool Culling, Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            switch (mode)
            {
                case MMDDrawingMode.Normal:
                    device.BlendState = ModelBlendState;
                    device.RasterizerState = Culling ? Microsoft.Xna.Framework.Graphics.RasterizerState.CullCounterClockwise : Microsoft.Xna.Framework.Graphics.RasterizerState.CullNone;
                    break;
                case MMDDrawingMode.Edge:
                    device.BlendState = EdgeBlendState;
                    device.RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState.CullCounterClockwise;
                    break;
                default:
                    throw new NotImplementedException();
            }
            device.DepthStencilState = Microsoft.Xna.Framework.Graphics.DepthStencilState.Default;
            device.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.LinearWrap;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertexCount">頂点数</param>
        /// <param name="vertMap">モデルの頂点とMMD頂点の対応</param>
        /// <param name="indexBuffer">インデックスバッファ抽象</param>
        /// <param name="device">グラフィックデバイス抽象</param>
        /// <param name="xnaDevice">XNA グラフィックデバイス</param>
        public MMDModelPart(int triangleCount, int vertexCount, Dictionary<long, int[]> vertMap,
            IMMDIndexBuffer indexBuffer, IMMDGraphicsDevice device,
            Microsoft.Xna.Framework.Graphics.GraphicsDevice xnaDevice)
        {
            this.triangleCount = triangleCount;
            this.vertexCount = vertexCount;
            this.indexBuffer = indexBuffer;
            this.VertMap = vertMap;
            this.graphicsDevice = device;
            this.xnaGraphicsDevice = xnaDevice;
        }
        /// <summary>
        /// モデル追加時に親モデルを取得
        /// </summary>
        /// <param name="model">親モデル</param>
        public void SetModel(MMDModel model)
        {
            this.model = (MMDXModel)model;
        }
        /// <summary>
        /// 行列を float[16] 配列に変換
        /// </summary>
        private static float[] MatrixToArray(ref Matrix m)
        {
            return new float[] {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }
        /// <summary>
        /// Vector3 を float[4] 配列に変換
        /// </summary>
        private static float[] VectorToArray(ref Vector3 v)
        {
            return new float[] { v.X, v.Y, v.Z, 1f };
        }
        /// <summary>
        /// エフェクトに各種値を適用
        /// </summary>
        /// <param name="world">ワールド</param>
        /// <param name="mode">モデル描画モード</param>
        public virtual void SetParams(MMDDrawingMode mode, ref Matrix world)
        {
            Matrix view, projection;
            //カメラ情報の取得
            Vector2 viewportSize = new Vector2(xnaGraphicsDevice.Viewport.Width, xnaGraphicsDevice.Viewport.Height);
            float aspectRatio = viewportSize.X / viewportSize.Y;
            MMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out view, out projection);
            
            //マトリクス処理
            // IMMDEffect を通じて設定 (ビルトインエフェクトの場合も互換)
            var eff = Effect;
            if (eff != null)
            {
                float[] worldArr = MatrixToArray(ref world);
                float[] viewArr = MatrixToArray(ref view);
                float[] projArr = MatrixToArray(ref projection);
                eff.SetMatrix("World", worldArr);
                eff.SetMatrix("View", viewArr);
                eff.SetMatrix("Projection", projArr);
                Vector3 camPos = MMDXCore.Instance.Camera.Position;
                float[] eyePos = VectorToArray(ref camPos);
                eff.SetVector("EyePosition", eyePos);

                //ライティング処理
                Vector3 color, dir;
                MMDXCore.Instance.Light.GetLightParam(out color, out dir);
                float[] ambientArr = VectorToArray(ref color);
                eff.SetVector("AmbientLightColor", ambientArr);
                float[] dirArr = VectorToArray(ref dir);
                eff.SetVector("DirLight0Direction", dirArr);
            }
            
            // XNA Effect が直接設定されている場合 (ビルトインシェーダー対応)
            // xnaEffect フィールドは内部で XNA Effect にアクセスする場合に使用
        }
        
        /// <summary>
        /// モデルの描画
        /// </summary>
        public virtual void Draw(MMDDrawingMode mode)
        {
            //レンダーステートセットアップ
            SetUpRenderState(mode, model.Culling, xnaGraphicsDevice);
            //バッファセット (抽象経由)
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.SetIndexBuffer(indexBuffer);

            Effect.Apply();
            // DrawIndexedPrimitives を抽象経由で呼ぶ
            graphicsDevice.DrawIndexedPrimitives(0, 0, vertexCount, 0, triangleCount);

            // XNA ネイティブ状態後始末
            xnaGraphicsDevice.Indices = null;
            xnaGraphicsDevice.SetVertexBuffer(null);
        }



        /// <summary>
        /// スキン行列を元に頂点を修正
        /// </summary>
        /// <param name="skinTransforms">スキン行列配列</param>
        public abstract void SetSkinMatrix(Matrix[] skinTransforms);
        /// <summary>
        /// 表情を元に頂点を修正
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        public abstract void SetFace(IMMDFaceManager faceManager);



        #region IDisposable メンバー
        /// <summary>
        /// 破棄処理
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion
    }
    
}
