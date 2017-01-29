using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// エッジマネージャ
    /// </summary>
    public class EdgeManager : IEdgeManager, IDisposable
    {
        RenderTarget2D edgeMap = null;
        SpriteBatch spriteBatch;
        GameWindow window;
        GraphicsDevice graphics;
        bool bEdgeDetectionMode;
        
        #region IEdgeManager メンバー
        /// <summary>
        /// エッジ描画モード(エッジ描画のためのプリレンダリングモード)かどうか
        /// </summary>
        public bool IsEdgeDetectionMode
        {
            get { return bEdgeDetectionMode; }
        }
        /// <summary>
        /// エッジ太さ
        /// </summary>
        public float EdgeWidth { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="window">ゲームウィンドウ</param>
        /// <param name="graphics">GraphicsDevice</param>
        public EdgeManager(GameWindow window, GraphicsDevice graphics)
        {
            UpdateRenderTarget(window, graphics);
            spriteBatch = new SpriteBatch(graphics);
            EdgeWidth = 1f;
            window.ClientSizeChanged += new EventHandler<EventArgs>(ClientSizeChanged);
            this.window = window;
            this.graphics = graphics;
        }

        void ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateRenderTarget(window, graphics);
        }

        void UpdateRenderTarget(GameWindow window, GraphicsDevice graphics)
        {
            PresentationParameters pp = graphics.PresentationParameters;
            if (edgeMap != null)
                edgeMap.Dispose();
            edgeMap = new RenderTarget2D(graphics, pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                   pp.BackBufferFormat, pp.DepthStencilFormat);
        }
        /// <summary>
        /// エッジ描画モード(エッジ描画のためのプリレンダリングモード)の開始
        /// </summary>
        /// <param name="graphics">グラフィックデバイス</param>
        public void StartEdgeDetection(GraphicsDevice graphics)
        {
            if(bEdgeDetectionMode)
                throw new InvalidOperationException("すでにエッジ検出モードは開始しています");
            graphics.SetRenderTarget(edgeMap);
            graphics.Clear(new Color(0, 0, 0, 0));

            bEdgeDetectionMode = true;
        }
        /// <summary>
        /// エッジ描画モードの終了
        /// </summary>
        /// <param name="graphics">グラフィックデバイス</param>
        public void EndEdgeDetection(GraphicsDevice graphics)
        {
            if(!bEdgeDetectionMode)
                throw new InvalidOperationException("エッジ検出モードを開始していません");
            graphics.SetRenderTarget(null);
            bEdgeDetectionMode = false;
        }

        /// <summary>
        /// 検出したエッジを描画する
        /// </summary>
        /// <remarks>2Dで描画されるので注意</remarks>
        public void DrawEdge(GraphicsDevice graphics)
        {
            if (MMDXCore.Instance.EdgeEffect == null)
                return;//ここに無いってことはモデル一個も読み込まれてないってことだから描く必要ないよね
            Viewport viewport=graphics.Viewport;
            MMDXCore.Instance.EdgeEffect.Parameters["EdgeWidth"].SetValue(EdgeWidth);
            MMDXCore.Instance.EdgeEffect.Parameters["ScreenResolution"].SetValue(new Vector2(viewport.Width, viewport.Height));
            //MMDXCore.Instance.EdgeEffect.Parameters["Texture"].SetValue(edgeMap);
            MMDXCore.Instance.EdgeEffect.CurrentTechnique = MMDXCore.Instance.EdgeEffect.Techniques["MMDEdgeEffect"];
            spriteBatch.Begin(0, BlendState.NonPremultiplied, null, null, null, MMDXCore.Instance.EdgeEffect);
            //spriteBatch.Begin();
            spriteBatch.Draw(edgeMap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        #endregion

        #region IDisposable メンバー
        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {
            window.ClientSizeChanged -= new EventHandler<EventArgs>(ClientSizeChanged);
        }

        #endregion
    }
}
