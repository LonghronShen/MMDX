using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using SlimDX.Windows;
using SlimDX;
using System.Drawing;
using MikuMikuDance.SlimDX;
using System.IO;
using MikuMikuDance.Core.Model;
using System.Diagnostics;
using MikuMikuDance.Core.Motion;
using System.Threading;
using MikuMikuDance.SlimDX.Misc;
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.SlimDX.Accessory;

namespace SlimMMDXDemo1
{
    static class Program
    {
        public static bool PlayFlag = false;
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //C#+フォームの標準準備
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            //フォームの準備(SlimDXではRenderFormも使用可能)
            var form = new FrmMain();// new RenderForm();
            //終了イベントを捕捉
            form.FormClosed += new FormClosedEventHandler(form_FormClosed);
            PresentParameters pp;
            //SlimDXのDeviceを準備
            var device = new Device(new Direct3D(), 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing, pp = new PresentParameters()
            {
                BackBufferWidth = form.ClientSize.Width,
                BackBufferHeight = form.ClientSize.Height
            });
            //トゥーンテクスチャのパスを準備(SlimMMDXではトゥーンフォルダを別に用意する必要がある)
            string[] toonTexPath = new string[10];
            string baseDir = Path.GetDirectoryName(Application.ExecutablePath);
            for (int i = 1; i <= 10; ++i)
            {
                toonTexPath[i - 1] = Path.Combine(baseDir, Path.Combine("toons", "toon" + i.ToString("00") + ".bmp"));
            }
            //SlimMMDXのセットアップ(他の機能よりも先に使用する)
            SlimMMDXCore.Setup(device, toonTexPath);
            //SlimMMDXCore.Instance.UsePhysics = false;
            //モデルの読み込み
            MMDModel model = SlimMMDXCore.Instance.LoadModelFromFile("Models/miku.pmd");
            //モーションの読み込み
            MMDMotion motion = SlimMMDXCore.Instance.LoadMotionFromFile("Motions/TrueMyHeart.vmd");
            //モーションのセットアップ
            model.AnimationPlayer.AddMotion("TrueMyHeart", motion, MMDMotionTrackOptions.UpdateWhenStopped);
            //時間管理フラグ
            long beforeCount = -1;
            bool deviceLost = false;
            //メインループ
            MessagePump.Run(form, () =>
            {
                //経過時間を計算
                float timeStep;
                if (beforeCount < 0)
                {
                    timeStep = 0.0f;
                    beforeCount = Stopwatch.GetTimestamp();
                }
                else
                {
                    timeStep = ((float)(Stopwatch.GetTimestamp() - beforeCount)) / (float)Stopwatch.Frequency;
                    beforeCount = Stopwatch.GetTimestamp();
                }
                if (PlayFlag)
                {
                    model.AnimationPlayer["TrueMyHeart"].Reset();
                    model.PhysicsManager.Reset();
                    model.AnimationPlayer["TrueMyHeart"].Start();
                    PlayFlag = false;
                }
                //SlimMMDXCoreのUpdate処理
                SlimMMDXCore.Instance.Update(timeStep);
                if (deviceLost)
                {
                    if (device.TestCooperativeLevel() == ResultCode.DeviceNotReset)
                    {
                        device.Reset(pp);
                        SlimMMDXCore.Instance.OnResetDevice();
                        deviceLost = false;
                    }
                }
                if (!deviceLost)
                {
                    try
                    {
                        //描画処理
                        device.BeginScene();
                        //画面のクリア
                        device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
                        //モデルの描画
                        model.Draw();
                        //描画処理の終了
                        device.EndScene();
                        device.Present(Present.None);
                    }
                    catch (Direct3D9Exception e)
                    {
                        if (e.ResultCode == ResultCode.DeviceLost)
                        {
                            SlimMMDXCore.Instance.OnLostDevice();
                            deviceLost = true;
                        }
                        else
                            throw;
                    }
                }
                //速度合わせ
                if (timeStep < 0.016666)
                    Thread.Sleep((int)(16.66666 - timeStep * 1000.0f));
            });
            //SlimMMDXの解放処理
            foreach (var item in ObjectTable.Objects)
                item.Dispose();
        }

        static void form_FormClosed(object sender, FormClosedEventArgs e)
        {
            SlimMMDXCore.Instance.Dispose();
        }

        
    }
}
