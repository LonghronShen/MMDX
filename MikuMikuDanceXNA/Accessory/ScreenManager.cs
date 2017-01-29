using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// スクリーンマネージャ
    /// </summary>
    public class ScreenManager: IDisposable
    {
        RenderTarget2D[] screen = new RenderTarget2D[2];
        int bufferIndex = 1;
        bool bStarted = false;
        GraphicsDevice graphics;
        GameWindow window;
        /// <summary>
        /// このインスタンスが保持しているテクスチャ
        /// </summary>
        public Texture2D Screen { get { return screen[bufferIndex]; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="window">GameWindowオブジェクト</param>
        /// <param name="graphics">GraphicsDevice</param>
        public ScreenManager(GameWindow window, GraphicsDevice graphics)
        {
            UpdateRenderTarget(window, graphics);
            this.window = window;
            this.graphics = graphics;
            window.ClientSizeChanged += new EventHandler<EventArgs>(ClientSizeChanged);
        }

        void ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateRenderTarget(window, graphics);
        }

        void UpdateRenderTarget(GameWindow window, GraphicsDevice graphics)
        {
            PresentationParameters pp = graphics.PresentationParameters;
            for (int i = 0; i < 2; i++)
            {
                if (screen[i] != null)
                    screen[i].Dispose();
                screen[i] = new RenderTarget2D(graphics, pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                       pp.BackBufferFormat, pp.DepthStencilFormat);
            }
        }
        /// <summary>
        /// スクリーンキャプチャスタート
        /// </summary>
        /// <param name="clearColor">初期化色</param>
        public void StartCapture(Color clearColor)
        {
            if (bStarted)
                throw new InvalidOperationException("StartCaptureはすでに開始しています");
            //レンダリングターゲットを退避……要らない
            //レンダリングターゲットを変更
            graphics.SetRenderTarget(screen[bufferIndex]);
            bufferIndex = (bufferIndex + 1) % 2;
            //レンダーターゲットをクリア
            graphics.Clear(clearColor);
            bStarted = true;
        }
        /// <summary>
        /// キャプチャ終了
        /// </summary>
        public void EndCapture()
        {
            //レンダーターゲットを元に戻す
            graphics.SetRenderTarget(null);
            bStarted = false;
        }

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
