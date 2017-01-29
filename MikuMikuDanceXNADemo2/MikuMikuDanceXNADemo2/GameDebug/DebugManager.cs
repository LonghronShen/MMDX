#region Using ステートメント

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DebugSample
{
    /// <summary>
    /// デバッグ用のグラフィクスコンテントを格納する為のデバッグ用マネージャー
    /// </summary>
    public class DebugManager : DrawableGameComponent
    {
        #region プロパティ

        /// <summary>
        /// デバッグ用コンテントマネージャーの取得
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// デバッグ用SpriteBatchの取得
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// 白テクスチャの取得
        /// </summary>
        public Texture2D WhiteTexture { get; private set; }

        /// <summary>
        /// デバッグ用フォント
        /// </summary>
        public SpriteFont DebugFont { get; private set; }

        #endregion

        #region 初期化

        public DebugManager(Game game)
            : base(game)
        {
            // サービスとして登録する
            Game.Services.AddService(typeof(DebugManager), this);

            Content = new ContentManager(game.Services);
            Content.RootDirectory = "Content/Debug";

            // このコンポーネント自体はUpdate、Drawが呼ばれる必要はない
            this.Enabled = false;
            this.Visible = false;
        }

        protected override void LoadContent()
        {
            // デバッグ用コンテントの読み込み
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            DebugFont = Content.Load<SpriteFont>("DebugFont");

            // 白テクスチャの生成
            WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            Color[] whitePixels = new Color[] { Color.White };
            WhiteTexture.SetData<Color>(whitePixels);

            base.LoadContent();
        }

        #endregion
    }
}