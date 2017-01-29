using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.Windows;
using SlimDX.Direct3D9;
using SlimDX;
using System.Drawing;
using System.Threading;

namespace SlimMMDXDemoFramework
{
    public class DemoFramework : IDisposable
    {
        Form form;
        Control targetControl;
        Device device;
        PresentParameters pp;

        protected Device GraphicsDevice { get { return device; } }
        protected Control TargetControl { get { return targetControl; } }
        
        public DemoFramework(Control targetControl)
        {
            form = targetControl.FindForm();
            this.targetControl = targetControl;
        }
        Device CreateDevice()
        {
            return new Device(new Direct3D(), 0, DeviceType.Hardware, targetControl.Handle, CreateFlags.HardwareVertexProcessing, pp = new PresentParameters
            {
                BackBufferWidth = targetControl.Width,
                BackBufferHeight = targetControl.Height
            });
        }
        protected virtual void Initialize() { }
        protected virtual void LoadContent() { }

        protected virtual void OnLostDevice() { }
        protected virtual void OnResetDevice() { }

        public void Run()
        {
            Clock clock = new Clock();
            bool isFormClosed = false;
            bool deviceLost = false;
            form.FormClosed += (o, args) => isFormClosed = true;

            form.Show();
            device = CreateDevice();
            Initialize();
            LoadContent();

            clock.Start();
            MessagePump.Run(form, () =>
            {
                if (isFormClosed)
                    return;
                float FrameDelta = clock.Update();
                Update(FrameDelta);
                if (deviceLost)
                {
                    if (device.TestCooperativeLevel() == ResultCode.DeviceNotReset)
                    {
                        device.Reset(pp);
                        OnResetDevice();
                        deviceLost = false;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        return;
                    }
                }
                //描画処理
                if (!deviceLost)
                {
                    try
                    {
                        device.BeginScene();
                        Draw(FrameDelta);
                        DrawEnd();
                    }
                    catch (Direct3D9Exception e)
                    {
                        if (e.ResultCode == ResultCode.DeviceLost)
                        {
                            OnLostDevice();
                            deviceLost = true;
                        }
                        else
                            throw;
                    }
                }
                clock.Sync();
            });
            //COMオブジェクトの解放処理
            foreach (var item in ObjectTable.Objects)
                item.Dispose();
        }

        protected virtual void Update(float frameDelta) { }
        protected virtual void Draw(float frameDelta) { }
        protected virtual void DrawEnd()
        {
            //描画処理の終了
            device.EndScene();
            device.Present();
        }
        #region IDisposable メンバー

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
        protected virtual void Dispose(bool disposeManagedResources)
        {
            form.Dispose();
        }
    }
}
