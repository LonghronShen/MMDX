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
using DebugSample;
using MikuMikuDance.XNA.Misc;
using MikuMikuDance.XNA.Accessory;
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.XNA;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDanceXNADemo2
{
    /// <summary>
    /// エッジ描画とデバッグシステム
    /// </summary>
    /// <remarks>ひげねこさん(伊藤 雄一)のデバッグシステムの組み込み例となります</remarks>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        //モデル
        MMDModel model;
        //モーション
        MMDMotion motion;
        //前回のキーボードの入力を保持
        KeyboardState beforeState;
        
        //エッジマネージャ
        EdgeManager edgeManager;

        //物理エンジンのデバッグ描画
        PhysicsDebugDraw debugDraw;
        bool DebugDrawVisible = false;

        // デバッグマネージャー
        DebugManager debugManager;
        // デバッグコマンドUI
        DebugCommandUI debugCommandUI;
        // FPSカウンター
        FpsCounter fpsCounter;
        // タイムルーラー
        TimeRuler timerRuler;

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
            // デバッグマネージャーの初期化と追加
            debugManager = new DebugManager(this);
            Components.Add(debugManager);

            // デバッグマコマンドUIの初期化と追加
            debugCommandUI = new DebugCommandUI(this);

            // デバッグコマンドUIを最上面に表示させる為にDrawOrderを変更する
            debugCommandUI.DrawOrder = 100;

            Components.Add(debugCommandUI);

            // FPSカウンターの初期化と追加
            fpsCounter = new FpsCounter(this);
            Components.Add(fpsCounter);
            fpsCounter.Visible = true;

            // タイムルーラーの初期化と追加
            timerRuler = new TimeRuler(this);
            Components.Add(timerRuler);
            timerRuler.Visible = true;
            timerRuler.ShowLog = true;

#if WINDOWS || XBOX
            // リモートデバッグコマンド「remote」の追加
            Components.Add(new RemoteDebugCommand(this));
#endif
            //デバッグコマンドに物理エンジンの剛体描画のコマンド追加
            debugCommandUI.RegisterCommand("physics", "Phisics Debug Draw", (host, command, arguments) =>
            {
                if (arguments.Count == 0)
                    DebugDrawVisible = !DebugDrawVisible;
                else
                {
                    foreach (string arg in arguments)
                    {
                        switch (arg.ToLower())
                        {
                            case "on":
                                DebugDrawVisible = true;
                                break;
                            case "off":
                                DebugDrawVisible = false;
                                break;
                        }
                    }
                }
            });
            base.Initialize();
        }

        /// <summary>
        /// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// 読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            //モデルをパイプラインより読み込み
            model = MMDXCore.Instance.LoadModel("Miku-metal", Content);
            //サンプルモデルはカリングを行わない。(他のモデルはカリングを行う)
            model.Culling = false;
            //モーションをパイプラインより読み込み
            motion = MMDXCore.Instance.LoadMotion("TrueMyHeart", Content);
            //モデルにモーションをセット
            model.AnimationPlayer.AddMotion("TrueMyHeart", motion, MMDMotionTrackOptions.UpdateWhenStopped);
            //エッジマネージャの作成
            edgeManager = new EdgeManager(Window, GraphicsDevice);
            //エッジマネージャの登録
            MMDXCore.Instance.EdgeManager = edgeManager;
            //物理エンジンデバッグの作成
            debugDraw = new PhysicsDebugDraw(GraphicsDevice);
            //MMDXにセット
            MMDXCore.Instance.Physics.DebugDrawer = debugDraw;
            //MMDXのProfileイベントとtimeRulerとを接続
            MMDXProfiler.MMDBeginMark += (bar, name, color) => timerRuler.BeginMark(bar, name, color);
            MMDXProfiler.MMDEndMark += (bar, name) => timerRuler.EndMark(bar, name);
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
            //timeRulerにフレーム開始を伝える
            timerRuler.StartFrame();
            //Updateの計測開始
            timerRuler.BeginMark(1, "Update", Color.Blue);
            // ゲームの終了条件をチェックします。
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                (!beforeState.IsKeyDown(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape)))
                this.Exit();//ゲーム終了
            //エンターを入力すると
            if (!debugCommandUI.Focused && (!beforeState.IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.Enter) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)))
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
            //Updateの計測終了
            timerRuler.EndMark(1, "Update");
            
        }

        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Draw(GameTime gameTime)
        {
            //Drawの計測開始
            timerRuler.BeginMark(1, "Draw", Color.Yellow);
            //エッジ検出モードの開始
            edgeManager.StartEdgeDetection(GraphicsDevice);
            //モデルのエッジを検出
            model.Draw();
            //エッジ検出モードの終了
            edgeManager.EndEdgeDetection(GraphicsDevice);
            //画面を消去する
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //モデルを描画する
            model.Draw();
            //検出したエッジを描画する
            edgeManager.DrawEdge(GraphicsDevice);
            //物理エンジンの剛体を描画
            if (DebugDrawVisible)
                debugDraw.Draw(GraphicsDevice);
            else
                debugDraw.Reset();
            base.Draw(gameTime);
            //Drawの計測終了
            timerRuler.EndMark(1, "Draw");
        }
        /// <summary>
        /// 破棄処理
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            //エッジマネージャの破棄
            edgeManager.Dispose();
            model.Dispose();
            //MMDの破棄処理を実行
            MMDXCore.Instance.Dispose();
            base.Dispose(disposing);
        }
    }
}
