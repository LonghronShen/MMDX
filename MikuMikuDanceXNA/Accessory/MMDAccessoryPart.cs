using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// アクセサリパーツ
    /// </summary>
    public class MMDAccessoryPart
    {
        int vertexCount;//頂点バッファの頂点数
        int triangleCount;//三角形の個数
        int baseVertex;//インデックスバッファのオフセット
        internal IndexBuffer indices { get; private set; }
        bool Screen;

        Effect m_effect;
        /// <summary>
        /// エフェクト
        /// </summary>
        public Effect Effect { get { return m_effect; } internal set { m_effect = value; } }
        /// <summary>
        /// このパーツのエッジOn/Off
        /// </summary>
        public bool Edge { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="vertexCount">頂点数</param>
        /// <param name="indices">インデックスバッファ</param>
        /// <param name="baseVertex">頂点開始位置</param>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="screen">スクリーンタイプのパーツかどうか</param>
        /// <param name="edge">エッジOn/Off</param>
        public MMDAccessoryPart(int vertexCount, IndexBuffer indices,  int baseVertex, int triangleCount, bool screen, bool edge)
        {
            this.vertexCount = vertexCount;
            this.indices = indices;
            this.baseVertex = baseVertex;
            this.triangleCount = triangleCount;
            Screen = screen;
            Edge = edge;
        }

        static BlendState ModelBlendState = BlendState.NonPremultiplied;
        static BlendState EdgeBlendState = BlendState.Opaque;

        static void SetUpRenderState(MMDDrawingMode mode, GraphicsDevice GraphicsDevice)
        {
            switch (mode)
            {
                case MMDDrawingMode.Normal:
                    GraphicsDevice.BlendState = ModelBlendState;
                    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
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
        /// エフェクトにマトリックスを適用
        /// </summary>
        /// <param name="mode">モデル描画モード</param>
        /// <param name="world">ワールド</param>
        public void SetParams(MMDDrawingMode mode, ref Matrix world)
        {
            Matrix view, projection;
            //カメラ情報の取得
            Viewport viewport = Effect.GraphicsDevice.Viewport;
            float aspectRatio = viewport.AspectRatio;
            MMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out view, out projection);
            
            //マトリクス処理
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["EyePosition"].SetValue(MMDXCore.Instance.Camera.Position);
            
            //ライティング処理
            Vector3 color, dir;
            MMDXCore.Instance.Light.GetLightParam(out color, out dir);
            Effect.Parameters["AmbientLightColor"].SetValue(color);
            Effect.Parameters["DirLight0Direction"].SetValue(dir);
            //ここでエッジ設定
            Effect.Parameters["Edge"].SetValue(Edge);
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
        /// <summary>
        /// アクセサリパーツの描画
        /// </summary>
        /// <param name="mode">描画モード</param>
        public void Draw(MMDDrawingMode mode)
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;
            SetUpRenderState(mode, graphics);
            if (Screen && MMDXCore.Instance.ScreenManager != null)
                Effect.Parameters["Texture"].SetValue(MMDXCore.Instance.ScreenManager.Screen);
            graphics.Indices = indices;
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVertex, 0, vertexCount, 0, triangleCount);
            }
        }
    }
}
