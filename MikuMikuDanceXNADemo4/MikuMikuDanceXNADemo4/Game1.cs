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
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.XNA.Misc;
using MikuMikuDance.XNA.Accessory;
using MikuMikuDance.XNA;

namespace MikuMikuDanceXNADemo4
{
    /// <summary>
    /// アクセサリサンプル。アクセサリの保持やスクリーン機能
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        //モデル
        MMDModel model;
        //モーション
        MMDMotion motion;
        //アクセサリ(ネギとステージ)
        MMDAccessoryBase negi, stage;
        //vac情報
        MMD_VAC vac;
        //エッジマネージャ
        EdgeManager edgeManager;
        //スクリーンマネージャ
        ScreenManager screenManager;
        //スクリーン描画用
        SpriteBatch screenDraw;
        //前フレームのキーボード状態
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
            //モーションの読み込み
            motion = MMDXCore.Instance.LoadMotion("TrueMyHeart", Content);
            //モデルにモーションをセット
            model.AnimationPlayer.AddMotion("TrueMyHeart", motion, MMDMotionTrackOptions.UpdateWhenStopped);
            //ネギの読み込み
            negi = MMDXCore.Instance.LoadAccessory("negi", Content);
            //vac情報(モデルとアクセサリの接続情報)
            vac = MMDXCore.Instance.LoadVAC("negi-vac", Content);
            //モデルにアクセサリを持たせる
            model.BindAccessory(negi, vac);
            //ステージの読み込み
            stage = MMDXCore.Instance.LoadAccessory("stage01", Content);
            //エッジマネージャの作成
            edgeManager = new EdgeManager(Window, GraphicsDevice);
            //エッジマネージャの登録
            MMDXCore.Instance.EdgeManager = edgeManager;
            //スクリーンマネージャの作成
            screenManager = new ScreenManager(Window, GraphicsDevice);
            //スクリーンマネージャの登録(登録するとキャプチャしたスクリーンがアクセサリに投影される)
            MMDXCore.Instance.ScreenManager = screenManager;
            //キャプチャしたスクリーンの描画用
            screenDraw = new SpriteBatch(GraphicsDevice);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                (!beforeState.IsKeyDown(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape)))
                this.Exit();//ゲーム終了
            //エンターを入力すると
            if ((!beforeState.IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.Enter)) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
            {
                //再生した後ならリセットをかける
                if (model.AnimationPlayer["TrueMyHeart"].NowFrame > 0)
                {
                    //停止
                    model.AnimationPlayer["TrueMyHeart"].Stop();
                    //巻き戻し
                    model.AnimationPlayer["TrueMyHeart"].Reset();
                    //剛体位置のリセット
                    model.PhysicsManager.Reset();
                }
                //モーションの再生
                model.AnimationPlayer["TrueMyHeart"].Start();
            }
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
            //モデルのエッジ検出
            model.Draw();
            //アクセサリにはエッジは出さないが、エッジの前に来る場合があるのでエッジ検出モードで描画する必要がある
            negi.Draw();
            stage.Draw();
            //エッジ検出モードの終了
            edgeManager.EndEdgeDetection(GraphicsDevice);
            //スクリーンキャプチャの開始
            screenManager.StartCapture(Color.CornflowerBlue);
            //モデルを描画
            model.Draw();
            //ネギを描画
            negi.Draw();
            //ステージを描画
            stage.Draw();
            //エッジを描画
            edgeManager.DrawEdge(GraphicsDevice);
            //スクリーンキャプチャの終了
            screenManager.EndCapture();
            //画面を消去
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //キャプチャした画像を画面に描画
            screenDraw.Begin();
            screenDraw.Draw(screenManager.Screen, Vector2.Zero, Color.White);
            screenDraw.End();
            base.Draw(gameTime);
        }
        /// <summary>
        /// 破棄処理
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            model.Dispose();
            //スクリーンマネージャの破棄
            screenManager.Dispose();
            //エッジマネージャの破棄
            edgeManager.Dispose();
            //MMDの破棄処理を実行
            MMDXCore.Instance.Dispose();
            base.Dispose(disposing);
        }
    }
}
