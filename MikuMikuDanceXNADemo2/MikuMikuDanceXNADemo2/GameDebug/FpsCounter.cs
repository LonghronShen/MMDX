#region Using ステートメント

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DebugSample
{
    /// <summary>
    /// FPSの測定と表示用コンポーネント
    /// </summary>
    public class FpsCounter : DrawableGameComponent
    {
        #region プロパティ

        /// <summary>
        /// 現在のFPS値
        /// </summary>
        public float Fps { get; private set; }

        /// <summary>
        /// FPS測定間隔の所得と設定
        /// </summary>
        public TimeSpan SampleSpan { get; set; }

        #endregion

        #region フィールド

        // デバッグマネージャー
        private DebugManager debugManager;

        // 測定用のストップウォッチ
        private Stopwatch stopwatch;

        // 測定中のフレーム数
        private int sampleFrames;

        // FPS表示用の文字バッファ
        private StringBuilder stringBuilder = new StringBuilder(16);

        #endregion

        #region 初期化

        public FpsCounter(Game game)
            : base(game)
        {
            SampleSpan = TimeSpan.FromSeconds(1);
        }

        public override void Initialize()
        {
            // デバッグマネージャーをゲームのサービスから取得
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManaerが登録されていません");

            // デバッグコマンドがサービスに登録されているなら、FPSコマンドを登録する
            IDebugCommandHost host =
                                Game.Services.GetService(typeof(IDebugCommandHost))
                                                                as IDebugCommandHost;

            if (host != null)
            {
                host.RegisterCommand("fps", "FPS Counter", this.CommandExecute);
                Visible = false;
            }

            // パラメーターの初期化
            Fps = 0;
            sampleFrames = 0;
            stopwatch = Stopwatch.StartNew();
            stringBuilder.Length = 0;

            base.Initialize();
        }

        #endregion

        /// <summary>
        /// FPSコマンド処理
        /// </summary>
        private void CommandExecute(IDebugCommandHost host,
                                    string command, IList<string> arguments)
        {
            if (arguments.Count == 0)
                Visible = !Visible;

            foreach (string arg in arguments)
            {
                switch (arg.ToLower())
                {
                    case "on":
                        Visible = true;
                        break;
                    case "off":
                        Visible = false;
                        break;
                }
            }
        }

        #region 更新と描画

        public override void Update(GameTime gameTime)
        {
            if (stopwatch.Elapsed > SampleSpan)
            {
                // FPSの更新と次の測定期間の開始
                Fps = (float)sampleFrames / (float)stopwatch.Elapsed.TotalSeconds;

                stopwatch.Reset();
                stopwatch.Start();
                sampleFrames = 0;

                // 表示文字列の更新
                stringBuilder.Length = 0;
                stringBuilder.Append("FPS: ");
                stringBuilder.AppendNumber(Fps);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            sampleFrames++;

            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            SpriteFont font = debugManager.DebugFont;

            // FPS表示の周りに半透明の黒い矩形のサイズ計算と配置
            Vector2 size = font.MeasureString("X");
            Rectangle rc =
                new Rectangle(0, 0, (int)(size.X * 14f), (int)(size.Y * 1.3f));

            Layout layout = new Layout(spriteBatch.GraphicsDevice.Viewport);
            rc = layout.Place(rc, 0.01f, 0.01f, Alignment.TopLeft);

            // FPS表示を矩形の中で配置
            size = font.MeasureString(stringBuilder);
            layout.ClientArea = rc;
            Vector2 pos = layout.Place(size, 0, 0.1f, Alignment.Center);

            // 描画
            spriteBatch.Begin();
            spriteBatch.Draw(debugManager.WhiteTexture, rc, new Color(0, 0, 0, 128));
            spriteBatch.DrawString(font, stringBuilder, pos, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

    }
}
