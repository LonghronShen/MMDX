using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// モデルパーツ
    /// </summary>
    public abstract class MMDModelPart : IMMDModelPart
    {

        Effect effect;
        IEffectMatrices effectMartices;
        IEffectLights effectLights;
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
        /// インデックスバッファ
        /// </summary>
        protected IndexBuffer indexBuffer;
        /// <summary>
        /// 頂点バッファ
        /// </summary>
        protected WritableVertexBuffer vertexBuffer; //このフィールドは継承先で代入
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
        /// エフェクト
        /// </summary>
        public Effect Effect { 
            get { return effect; }
            internal set
            {
                effectMartices = value as IEffectMatrices;
                effectLights = value as IEffectLights;
                effect = value;
            }
        }
        /// <summary>
        /// このパーツに関連付けられているグラフィックデバイス
        /// </summary>
        public GraphicsDevice GraphicsDevice { get { return indexBuffer.GraphicsDevice; } }

        static BlendState ModelBlendState = BlendState.NonPremultiplied;
        static BlendState EdgeBlendState = BlendState.Opaque;
        /// <summary>
        /// レンダリングモード設定
        /// </summary>
        /// <param name="mode">描画モード</param>
        /// <param name="Culling">カリングを行うか</param>
        /// <param name="GraphicsDevice">グラフィックデバイス</param>
        protected static void SetUpRenderState(MMDDrawingMode mode, bool Culling, GraphicsDevice GraphicsDevice)
        {
            switch (mode)
            {
                case MMDDrawingMode.Normal:
                    GraphicsDevice.BlendState = ModelBlendState;
                    GraphicsDevice.RasterizerState = Culling ? RasterizerState.CullCounterClockwise : RasterizerState.CullNone;
                    break;
                case MMDDrawingMode.Edge:
                    GraphicsDevice.BlendState = EdgeBlendState;
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                    break;
                default:
                    throw new NotImplementedException();
            }
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertexCount">頂点数</param>
        /// <param name="vertMap">モデルの頂点とMMD頂点の対応</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDModelPart(int triangleCount, int vertexCount, Dictionary<long, int[]> vertMap, IndexBuffer indexBuffer)
        {
            this.triangleCount = triangleCount;
            this.vertexCount = vertexCount;
            this.indexBuffer = indexBuffer;
            this.VertMap = vertMap;

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
        /// エフェクトに各種値を適用
        /// </summary>
        /// <param name="world">ワールド</param>
        /// <param name="mode">モデル描画モード</param>
        public virtual void SetParams(MMDDrawingMode mode, ref Matrix world)
        {
            Matrix view, projection;
            //カメラ情報の取得
            Viewport viewport = Effect.GraphicsDevice.Viewport;
            float aspectRatio = viewport.AspectRatio;
            MMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out view, out projection);
            
            //マトリクス処理
            if (effectMartices != null)
            {
                effectMartices.World = world;
                effectMartices.View = view;
                effectMartices.Projection = projection;
            }
            else
            {
                Effect.Parameters["World"].SetValue(world);
                Effect.Parameters["View"].SetValue(view);
                Effect.Parameters["Projection"].SetValue(projection);
                Effect.Parameters["EyePosition"].SetValue(MMDXCore.Instance.Camera.Position);
            }
            //ライティング処理
            Vector3 color, dir;
            MMDXCore.Instance.Light.GetLightParam(out color, out dir);
            if (effectLights != null)
            {
                effectLights.AmbientLightColor = color;
                effectLights.DirectionalLight0.Direction = dir;
                effectLights.DirectionalLight0.DiffuseColor = color / 4;
            }
            else
            {
                Effect.Parameters["AmbientLightColor"].SetValue(color);
                Effect.Parameters["DirLight0Direction"].SetValue(dir);
                //ここでエフェクト設定
                switch (mode)
                {
                    case MMDDrawingMode.Normal:
                        Effect.CurrentTechnique = Effect.Techniques["MMDEffect"];
                        break;
                    case MMDDrawingMode.Edge:
                        Effect.CurrentTechnique = Effect.Techniques["MMDNormalDepth"];
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            
        }
        
        /// <summary>
        /// モデルの描画
        /// </summary>
        public virtual void Draw(MMDDrawingMode mode)
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;
            //レンダーステートセットアップ
            SetUpRenderState(mode, model.Culling, graphics);
            //バッファセット
            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffer(vertexBuffer.VertexBuffer, vertexOffset);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }

            graphics.Indices = null;
            graphics.SetVertexBuffer(null);
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
            indexBuffer.Dispose();
        }

        #endregion
    }
    
}
