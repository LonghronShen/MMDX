using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.XNA;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDanceXNADemo3
{
    /// <summary>
    /// カメラとライト
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        //モデル
        MMDModel model;
        //モーション
        MMDMotion camera, light;
        //エッジマネージャ
        EdgeManager edgeManager;

        KeyboardState beforeState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// ゲームが実行を開始する前に必要な初期化を行います。
        /// ここで、必要なサービスを照会して、関連するグラフィック以外のコンテンツを
        /// 読み込むことができます。base.Initialize を呼び出すと、使用するすべての
        /// コンポーネントが列挙されるとともに、初期化されます。
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// 読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            //モデルの読み込み
            model = MMDXCore.Instance.LoadModel("Miku", Content);
            //サンプルモデルはカリングを行わない。(他のモデルはカリングを行う)
            model.Culling = false;
            //カメラとライトモーションの読み込み
            camera = MMDXCore.Instance.LoadMotion("Camera", Content);
            light = MMDXCore.Instance.LoadMotion("Light", Content);
            //ステージプレイヤーにモーションをセット
            MMDXCore.Instance.StageAnimationPlayer.AddMotion("Camera", camera);
            MMDXCore.Instance.StageAnimationPlayer.AddMotion("Light", light);
            //ループ再生
            MMDXCore.Instance.StageAnimationPlayer["Camera"].Start(true);
            MMDXCore.Instance.StageAnimationPlayer["Light"].Start(true);
            //エッジマネージャの作成
            edgeManager = new EdgeManager(Window, GraphicsDevice);
            //エッジマネージャの登録
            MMDXCore.Instance.EdgeManager = edgeManager;
        }

        /// <summary>
        /// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// アンロードします。
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// ワールドの更新、衝突判定、入力値の取得、オーディオの再生などの
        /// ゲーム ロジックを、実行します。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Update(GameTime gameTime)
        {
            // ゲームの終了条件をチェックします。
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                (!beforeState.IsKeyDown(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape)))
                this.Exit();//ゲーム終了

            //MMDのUpdateを呼び出す
            MMDXCore.Instance.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            
            base.Update(gameTime);
            //キーボードの状態を記録
            beforeState = Keyboard.GetState();
        }

        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Draw(GameTime gameTime)
        {
            //エッジ検出モードの開始
            edgeManager.StartEdgeDetection(GraphicsDevice);
            //モデルのエッジを検出
            model.Draw();
            //エッジ検出モードの終了
            edgeManager.EndEdgeDetection(GraphicsDevice);
            //画面の消去
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //モデルの描画
            model.Draw();
            //エッジの描画
            edgeManager.DrawEdge(GraphicsDevice);
            base.Draw(gameTime);
        }
        /// <summary>
        /// 破棄処理
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            //MMDの破棄処理を実行
            model.Dispose();
            MMDXCore.Instance.Dispose();
            base.Dispose(disposing);
        }
    }
}
