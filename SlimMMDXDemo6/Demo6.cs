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

namespace SlimMMDXDemo6
{
    class Demo6 : DemoFramework
    {
        //モデル
        MMDModel model;
        //モーション
        MMDMotion motion1, motion2;

        public Demo6(Control control)
            : base(control)
        {
            FrmMain form = control.FindForm() as FrmMain;
            form.trackBar1.ValueChanged += new EventHandler(trackBar1_ValueChanged);
        }

        void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar bar=sender as TrackBar;
            model.AnimationPlayer["LeftHand"].BlendingFactor = ((float)bar.Value) / 10.0f;
            model.AnimationPlayer["RightBye"].BlendingFactor = 1.0f - ((float)bar.Value) / 10.0f;
        }

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
            motion1 = SlimMMDXCore.Instance.LoadMotionFromFile("motions/LeftHand.vmd");
            motion2 = SlimMMDXCore.Instance.LoadMotionFromFile("motions/RightBye.vmd");
            //モーションのセット
            model.AnimationPlayer.AddMotion("LeftHand", motion1, MMDMotionTrackOptions.UpdateWhenStopped | MMDMotionTrackOptions.ExtendedMode);
            model.AnimationPlayer.AddMotion("RightBye", motion2, MMDMotionTrackOptions.UpdateWhenStopped | MMDMotionTrackOptions.ExtendedMode);
            //最初のブレンディングはLeftHandの方を100%にする
            model.AnimationPlayer["LeftHand"].BlendingFactor = 1f;//最初から1なのだが、分り易くするために代入
            model.AnimationPlayer["RightBye"].BlendingFactor = 0f;//ブレンディングファクターを0にする。
            //ループ再生
            model.AnimationPlayer["LeftHand"].Start(true);
            model.AnimationPlayer["RightBye"].Start(true);
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
            SlimMMDXCore.Instance.Update(frameDelta);
            base.Update(frameDelta);
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
