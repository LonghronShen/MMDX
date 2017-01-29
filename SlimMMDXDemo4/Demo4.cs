using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMMDXDemoFramework;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.SlimDX.Accessory;
using SlimDX.Direct3D9;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SlimDX;
using System.IO;
using MikuMikuDance.SlimDX;
using MikuMikuDance.SlimDX.Misc;
using System.Drawing;
using MikuMikuDance.Core.Accessory;

namespace SlimMMDXDemo4
{
    [StructLayout(LayoutKind.Sequential)]
    struct CustomVertex
    {
        public Vector4 Position;
        public Vector2 Texture;

        public static VertexElement[] VertexElements = new[]
        {
            new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
            new VertexElement(0, 16,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,0),
            VertexElement.VertexDeclarationEnd
        };
    }
    class Demo4 : DemoFramework
    {
        //モデル
        MMDModel model;
        //モーション
        MMDMotion motion;
        //アクセサリ
        MMDAccessory negi, stage;
        //アクセサリ接続情報
        MMD_VAC vac;
        //スクリーンマネージャ
        ScreenManager screenManager;
        //エッジマネージャ
        EdgeManager edgeManager;
        //画面貼りつけ用
        VertexBuffer vertex;
        CustomVertex[] screenVertex;
        VertexDeclaration vertexDec;
        //ターゲットフォーム
        FrmMain form;
        public Demo4(Control targetControl)
            : base(targetControl)
        {

        }
        protected override void Initialize()
        {
            form = (FrmMain)TargetControl.FindForm();
            form.tsMIPlay.Click += (e, args) =>
            {
                model.AnimationPlayer["TrueMyHeart"].Reset();
                model.PhysicsManager.Reset();
                model.AnimationPlayer["TrueMyHeart"].Start();
            };
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
            //モーションの登録
            model.AnimationPlayer.AddMotion("TrueMyHeart", motion, MMDMotionTrackOptions.UpdateWhenStopped);
            //アクセサリの読み込み
            negi = (MMDAccessory)SlimMMDXCore.Instance.LoadAccessoryFromFile("Accessories/negi.x");
            stage = (MMDAccessory)SlimMMDXCore.Instance.LoadAccessoryFromFile("Accessories/stage01.x");
            //アクセサリ接続情報(VAC)の読み込み
            vac = SlimMMDXCore.Instance.LoadVACFromFile("Accessories/negi-vac.vac");
            //モデルにアクセサリを持たせる
            model.BindAccessory(negi, vac);
            //スクリーンマネージャの作成
            screenManager = new ScreenManager(TargetControl.Width, TargetControl.Height);
            //スクリーンマネージャの登録
            SlimMMDXCore.Instance.ScreenManager = screenManager;
            //エッジマネージャの作成
            edgeManager = new EdgeManager(TargetControl.Width, TargetControl.Height);
            SlimMMDXCore.Instance.EdgeManager = edgeManager;
            //スクリーンを画面に描画する用の頂点を作成
            screenVertex = new CustomVertex[6];
            screenVertex[0].Position = new Vector4(0, 0, 0.5f, 1.0f);
            screenVertex[0].Texture = new Vector2(0, 0);
            screenVertex[1].Position = new Vector4(TargetControl.Width, 0, 0.5f, 1.0f);
            screenVertex[1].Texture = new Vector2(1, 0);
            screenVertex[2].Position = new Vector4(TargetControl.Width, TargetControl.Height, 0.5f, 1.0f);
            screenVertex[2].Texture = new Vector2(1, 1);
            screenVertex[3].Position = new Vector4(0, 0, 0.5f, 1.0f);
            screenVertex[3].Texture = new Vector2(0, 0);
            screenVertex[4].Position = new Vector4(TargetControl.Width, TargetControl.Height, 0.5f, 1.0f);
            screenVertex[4].Texture = new Vector2(1, 1);
            screenVertex[5].Position = new Vector4(0, TargetControl.Height, 0.5f, 1.0f);
            screenVertex[5].Texture = new Vector2(0, 1);
            CreateScreenVertex();
            base.LoadContent();
        }
        void CreateScreenVertex()
        {
            vertex = new VertexBuffer(SlimMMDXCore.Instance.Device, 6 * Marshal.SizeOf(typeof(CustomVertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);
            DataStream stream = vertex.Lock(0, 0, LockFlags.None);
            stream.WriteRange(screenVertex);
            vertex.Unlock();
            vertexDec = new VertexDeclaration(SlimMMDXCore.Instance.Device, CustomVertex.VertexElements);
            
        }
        protected override void OnLostDevice()
        {
            vertex.Dispose();
            vertexDec.Dispose();
            SlimMMDXCore.Instance.OnLostDevice();
            base.OnLostDevice();
        }
        protected override void OnResetDevice()
        {
            CreateScreenVertex();
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
            //エッジ検出モードの開始
            edgeManager.StartEdgeDetection();
            model.Draw();
            negi.Draw();
            //stage.Draw();
            edgeManager.EndEdgeDetection();
            //スクリーンキャプチャの開始
            screenManager.StartCapture(Color.CornflowerBlue);
            //モデルの描画
            model.Draw();
            negi.Draw();
            stage.Draw();
            edgeManager.DrawEdge();
            //スクリーンキャプチャの終了
            screenManager.EndCapture();
            //スクリーンの描画
            GraphicsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            SlimMMDXCore.Instance.Device.VertexDeclaration = vertexDec;
            SlimMMDXCore.Instance.Device.SetStreamSource(0, vertex, 0, Marshal.SizeOf(typeof(CustomVertex)));

            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaTestEnable, false);
            SlimMMDXCore.Instance.Device.SetTexture(0, screenManager.Screen);
            SlimMMDXCore.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            base.Draw(frameDelta);
        }
        protected override void Dispose(bool disposeManagedResources)
        {
            SlimMMDXCore.Instance.Dispose();
            base.Dispose(disposeManagedResources);
        }
    }
}
