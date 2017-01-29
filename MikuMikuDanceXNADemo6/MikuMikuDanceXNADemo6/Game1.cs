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

namespace MikuMikuDanceXNADemo6
{
    /// <summary>
    /// モーションブレンディングのデモ
    /// モーションブレンディングを行うことで２つのモーションをスムースにつなげることができます。
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        
        MMDModel model;
        MMDMotion motion1, motion2;

        decimal FactorPosition = 0m;
        decimal FactorVelocity = 0m;

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
            // TODO: ここに初期化ロジックを追加します。
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// 読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            //モデルとモーションをロード
            model = MMDXCore.Instance.LoadModel("Miku", Content);
            motion1 = MMDXCore.Instance.LoadMotion("LeftHand", Content);
            motion2 = MMDXCore.Instance.LoadMotion("RightBye", Content);
            //モーションをセット
            //UpdateWhenStoppedは再生停止中もトラック中のモーションを適用する
            //MMDMotionTrackOptions.Noneだと、再生停止中はモーショントラックを適用しない
            //ExtendedModeはキーフレームが無い再生位置でも最後のキーフレームを参照する
            model.AnimationPlayer.AddMotion("LeftHand", motion1, MMDMotionTrackOptions.UpdateWhenStopped | MMDMotionTrackOptions.ExtendedMode);
            model.AnimationPlayer.AddMotion("RightBye", motion2, MMDMotionTrackOptions.UpdateWhenStopped | MMDMotionTrackOptions.ExtendedMode);
            //最初のブレンディングはLeftHandの方を100%にする
            model.AnimationPlayer["LeftHand"].BlendingFactor = 1f;//最初から1なのだが、分り易くするために代入
            model.AnimationPlayer["RightBye"].BlendingFactor = 0f;//ブレンディングファクターを0にする。
            //両方ループ再生する
            model.AnimationPlayer["LeftHand"].Start(true);
            model.AnimationPlayer["RightBye"].Start(true);
            //ExtendedModeを指定する理由
            //モーションブレンディングは２つ以上のモーションを重ねあわせて行うもの
            //何もフレームがない場合は、ブレンディングを行わない
            //一方、フレームがあるとブレンディングを行うが、途中でフレームが終わるボーンは、そこでそのボーンのフレームが無くなるため、
            //途中でブレンディングが非連続的にそのボーンだけ無くなり、意図しない動作をすることがある。
        }

        /// <summary>
        /// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// アンロードします。
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: ここで ContentManager 以外のすべてのコンテンツをアンロードします。
        }

        /// <summary>
        /// ワールドの更新、衝突判定、入力値の取得、オーディオの再生などの
        /// ゲーム ロジックを、実行します。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Update(GameTime gameTime)
        {
            // ゲームの終了条件をチェックします。
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            //Enterキーでブレンディングファクター変化値を設定
            if ((Keyboard.GetState().IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed) && FactorVelocity == 0m)
            {
                if (FactorPosition == 0m)
                    FactorVelocity = 0.1m;
                else if (FactorPosition == 1m)
                    FactorVelocity = -0.1m;
            }
            //ブレンディングファクター値変化中
            if (FactorVelocity != 0m)
            {
                FactorPosition += FactorVelocity;
                if (FactorPosition <= 0m)
                {
                    FactorPosition = 0m;
                    FactorVelocity = 0m;
                }
                else if (FactorPosition >= 1m)
                {
                    FactorPosition = 1m;
                    FactorVelocity = 0m;
                }
                //ブレンディングファクターを設定し、モーションの切り替えをスムースに行う
                model.AnimationPlayer["LeftHand"].BlendingFactor = (float)(1m - FactorPosition);
                model.AnimationPlayer["RightBye"].BlendingFactor = (float)FactorPosition;
            }

            MMDXCore.Instance.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            // TODO: ここにゲームのアップデート ロジックを追加します。

            base.Update(gameTime);
        }

        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //モデルの描画
            model.Draw();

            base.Draw(gameTime);
        }
        protected override void Dispose(bool disposing)
        {
            model.Dispose();
            //MMDの破棄処理を実行
            MMDXCore.Instance.Dispose();
            base.Dispose(disposing);
        }
    }
}
