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
using MikuMikuDance.Core;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA;
using MikuMikuDance.XNA.Misc;
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.XNA.Accessory;

namespace MikuMikuDanceXNADemo1
{
    /// <summary>
    /// MikuMikuDance for XNAシンプルサンプル
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //XNAのデバイス
        GraphicsDeviceManager graphics;
        //MMDモデル
        MMDModel model;
        //MMDモーション
        MMDMotion motion;
        //前回のキーボードの入力を保持
        KeyboardState beforeState;
        GamePadButtons beforeButtons;
        /// <summary>
        /// コンストラクタ
        /// </summary>
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
            //モデルをパイプラインより読み込み
            model = MMDXCore.Instance.LoadModel("Miku", Content);
            //サンプルモデルはカリングを行わない。(他のモデルはカリングを行う)
            model.Culling = false;
            //モーションをパイプラインより読み込み
            motion = MMDXCore.Instance.LoadMotion("TrueMyHeart", Content);
            //モデルにモーションをセット
            model.AnimationPlayer.AddMotion("TrueMyHeart", motion, MMDMotionTrackOptions.UpdateWhenStopped);
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
            //エンターを入力すると
            if ((!beforeState.IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.Enter)) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.A== ButtonState.Pressed))
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
            beforeButtons = GamePad.GetState(PlayerIndex.One).Buttons;
            
        }
        
        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Draw(GameTime gameTime)
        {
            //画面を消去する
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //モデルを描画する
            model.Draw();
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
