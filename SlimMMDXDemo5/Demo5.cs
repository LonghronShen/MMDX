using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMMDXDemoFramework;
using System.Windows.Forms;
using MikuMikuDance.SlimDX;
using System.IO;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using SlimDX.Direct3D9;
using System.Drawing;

namespace SlimMMDXDemo5
{
    class Demo5 : DemoFramework
    {
        //モデル
        MMDModel model;
        //モーション
        MMDMotion motion;
        
        public Demo5(Control control) : base(control) {  }

        protected override void Initialize()
        {
            //トゥーンテクスチャのパスを準備(SlimMMDXではトゥーンフォルダを別に用意する必要がある)
            string[] toonTexPath = new string[10];
            string baseDir = Path.GetDirectoryName(Application.ExecutablePath);
            for (int i = 1; i <= 10; ++i)
            {
                toonTexPath[i - 1] = Path.Combine(baseDir, Path.Combine("toons", "toon" + i.ToString("00") + ".bmp"));
            }
            SlimMMDXCore.Setup(GraphicsDevice, toonTexPath);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            //モデルの読み込み
            model = SlimMMDXCore.Instance.LoadModelFromFile("models/Miku.pmd");
            //モーションの読み込み
            motion = SlimMMDXCore.Instance.LoadMotionFromFile("motions/TrueMyHeart.vmd");
            //モーションのセット
            model.AnimationPlayer.AddMotion("TrueMyHeart", motion, MMDMotionTrackOptions.UpdateWhenStopped);
            //モーション終了時のコールバックをセット
            model.AnimationPlayer["TrueMyHeart"].OnMotionEnd += new Action<string>(GotSays);
            base.LoadContent();
        }
        protected override void OnLostDevice()
        {
            SlimMMDXCore.Instance.OnLostDevice();
            base.OnLostDevice();
        }
        protected override void OnResetDevice()
        {
            SlimMMDXCore.Instance.OnResetDevice();
            base.OnResetDevice();
        }
        protected override void Update(float frameDelta)
        {
            if (model.AnimationPlayer["TrueMyHeart"].NowFrame == 0)
            {//そんなモーションで大丈夫か？
                //大丈夫だ。問題ない。
                model.AnimationPlayer["TrueMyHeart"].Reverse = false;
                model.AnimationPlayer["TrueMyHeart"].FramePerSecond = MMDMotionTrack.DefaultFPS;
                model.AnimationPlayer["TrueMyHeart"].Start();
            }
            SlimMMDXCore.Instance.Update(frameDelta);
            base.Update(frameDelta);
        }
        private void GotSays(string trackName)
        {
            if (model.AnimationPlayer[trackName].NowFrame == model.AnimationPlayer[trackName].MaxFrame)
            {
                //神は言っている……、ここで再生終了する定めではないと。
                model.AnimationPlayer[trackName].Stop();
                model.AnimationPlayer[trackName].Reverse = true;
                model.AnimationPlayer[trackName].FramePerSecond = MMDMotionTrack.DefaultFPS * 3m;
                model.AnimationPlayer[trackName].Start();
            }
        }
        protected override void Draw(float frameDelta)
        {
            GraphicsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            //モデルの描画
            model.Draw();
            base.Draw(frameDelta);
        }
        protected override void Dispose(bool disposeManagedResources)
        {
            SlimMMDXCore.Instance.Dispose();
            base.Dispose(disposeManagedResources);
        }
    }
}
