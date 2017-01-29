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
using MikuMikuDance.SlimDX.Misc;
using System.Drawing;
using SlimDX.Direct3D9;

namespace SlimMMDXDemo3
{
    class Demo3 : DemoFramework
    {
        MMDModel model;
        MMDMotion camera, light;
        EdgeManager edgeManager;

        public Demo3(Control targetControl)
            : base(targetControl) { }
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
            //カメラとライトモーションの読み込み
            camera = SlimMMDXCore.Instance.LoadMotionFromFile("motions/Camera.vmd");
            light = SlimMMDXCore.Instance.LoadMotionFromFile("motions/Light.vmd");
            //ステージプレイヤーにモーションをセット
            SlimMMDXCore.Instance.StageAnimationPlayer.AddMotion("Camera", camera);
            SlimMMDXCore.Instance.StageAnimationPlayer.AddMotion("Light", light);
            //ループ再生
            SlimMMDXCore.Instance.StageAnimationPlayer["Camera"].Start(true);
            SlimMMDXCore.Instance.StageAnimationPlayer["Light"].Start(true);
            //エッジマネージャの作成
            edgeManager = new EdgeManager(TargetControl.Width, TargetControl.Height);
            //エッジマネージャの登録
            SlimMMDXCore.Instance.EdgeManager = edgeManager;
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
            //MMDのUpdateを呼び出す
            SlimMMDXCore.Instance.Update(frameDelta);
            base.Update(frameDelta);
        }
        protected override void Draw(float frameDelta)
        {
            //エッジ検出モードの開始
            edgeManager.StartEdgeDetection();
            //モデルのエッジを検出
            model.Draw();
            //エッジ検出モードの終了
            edgeManager.EndEdgeDetection();
            //画面の消去
            GraphicsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            //モデルの描画
            model.Draw();
            //エッジの描画
            edgeManager.DrawEdge();
            base.Draw(frameDelta);
        }
        protected override void Dispose(bool disposeManagedResources)
        {
            SlimMMDXCore.Instance.Dispose();
            base.Dispose(disposeManagedResources);
        }
    }
}
