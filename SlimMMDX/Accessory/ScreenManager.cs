using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using SlimDX;

namespace MikuMikuDance.SlimDX.Accessory
{
    /// <summary>
    /// スクリーンマネージャ
    /// </summary>
    public class ScreenManager: IDisposable
    {
        Texture[] screen = new Texture[2];
        Surface[] renderSurface = new Surface[2];
        Surface[] depthBuffer = new Surface[2];
        int bufferIndex = 1;
        Surface oldTarget = null;
        Surface oldDepth = null;

        int width, height;
        /// <summary>
        /// このインスタンスが保持しているテクスチャ
        /// </summary>
        public Texture Screen { get { return screen[bufferIndex]; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="width">バックバッファ幅</param>
        /// <param name="height">バックバッファ高さ</param>
        public ScreenManager(int width, int height)
        {
            ChangeSize(width, height);
            SlimMMDXCore.Instance.LostDevice += OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice += OnResetDevice;
        }
        /// <summary>
        /// 画面サイズ変更
        /// </summary>
        /// <param name="width">バックバッファ幅</param>
        /// <param name="height">バックバッファ高さ</param>
        public void ChangeSize(int width, int height)
        {
            for (int i = 0; i < 2; i++)
            {
                screen[i] = new Texture(SlimMMDXCore.Instance.Device, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                renderSurface[i] = screen[i].GetSurfaceLevel(0);
                depthBuffer[i] = Surface.CreateDepthStencil(SlimMMDXCore.Instance.Device, width, height, Format.D16, MultisampleType.None, 0, true);
            }
            this.width = width;
            this.height = height;
        }
        void OnLostDevice()
        {
            for (int i = 0; i < 2; i++)
            {
                screen[i].Dispose();
                renderSurface[i].Dispose();
                depthBuffer[i].Dispose();
            }
        }
        void OnResetDevice()
        {
            ChangeSize(width, height);
        }
        /// <summary>
        /// スクリーンキャプチャスタート
        /// </summary>
        /// <param name="clearColor">初期化色</param>
        public void StartCapture(Color4 clearColor)
        {
            if (oldTarget != null || oldDepth != null)
                throw new InvalidOperationException("StartCaptureはすでに開始しています");
            //レンダリングターゲットを退避
            oldTarget = SlimMMDXCore.Instance.Device.GetRenderTarget(0);
            oldDepth = SlimMMDXCore.Instance.Device.DepthStencilSurface;

            //レンダリングターゲットを変更
            SlimMMDXCore.Instance.Device.SetRenderTarget(0, renderSurface[bufferIndex]);
            SlimMMDXCore.Instance.Device.DepthStencilSurface = depthBuffer[bufferIndex];
            bufferIndex = (bufferIndex + 1) % 2;

            //レンダーターゲットをクリア
            SlimMMDXCore.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, clearColor, 1.0f, 0);
        }
        /// <summary>
        /// キャプチャ終了
        /// </summary>
        public void EndCapture()
        {
            //レンダーターゲットを元に戻す
            SlimMMDXCore.Instance.Device.SetRenderTarget(0, oldTarget);
            SlimMMDXCore.Instance.Device.DepthStencilSurface = oldDepth;
            oldTarget.Dispose();
            oldDepth.Dispose();
            oldDepth = null;
            oldTarget = null;
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose()
        {
            SlimMMDXCore.Instance.LostDevice -= OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice -= OnResetDevice;
        }
    }
}
