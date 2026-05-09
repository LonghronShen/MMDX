using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Motion;

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// ステージ用モーショントラック
    /// </summary>
    public class MMDStageMotionTrack
    {
        bool bLoopPlay = false;
        bool bReverse = false;
         decimal NowFrame = 0;
        decimal MaxFrame = 0;
        bool bStart = false;
        List<MMDCameraKeyFrame> cameraFrames;
        List<MMDLightKeyFrame> lightFrames;

        int cameraPos;
        int lightPos;

        /// <summary>
        /// カメラ（DI対応、null時はMMDCore.Current.Camera）
        /// </summary>
        public IMMDXCamera Camera { get; set; }
        /// <summary>
        /// ライト（DI対応、null時はMMDCore.Current.Light）
        /// </summary>
        public IMMDXLight Light { get; set; }

        /// <summary>
        /// モーション再生用FPS
        /// </summary>
        public decimal FramePerSecond { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionData">モーションデータ</param>
        /// <param name="camera">カメラ（省略時はMMDCore.Current.Camera）</param>
        /// <param name="light">ライト（省略時はMMDCore.Current.Light）</param>
        public MMDStageMotionTrack(MMDMotion motionData, IMMDXCamera camera = null, IMMDXLight light = null)
        {
            Camera = camera;
            Light = light;
            cameraFrames = motionData.CameraFrames;
            lightFrames = motionData.LightFrames;
            //モーションのFPS=30
            FramePerSecond = 30m;
            foreach (var frame in cameraFrames)
            {
                MaxFrame = Math.Max(MaxFrame, frame.FrameNo);
            }
            foreach (var frame in lightFrames)
                MaxFrame = Math.Max(MaxFrame, frame.FrameNo);
        }

        private IMMDXCamera ResolveCamera()
        {
            return Camera ?? (MMDCore.Current != null ? MMDCore.Current.Camera : null);
        }

        private IMMDXLight ResolveLight()
        {
            return Light ?? (MMDCore.Current != null ? MMDCore.Current.Light : null);
        }

        /// <summary>
        /// モーションの再生
        /// </summary>
        public void Start()
        {
            Start(false);
        }
        /// <summary>
        /// モーションの再生
        /// </summary>
        /// <param name="LoopPlay">ループ再生</param>
        public void Start(bool LoopPlay)
        {
            bLoopPlay = LoopPlay;
            bStart = true;
        }
        /// <summary>
        /// モーションの停止
        /// </summary>
        public void Stop()
        {
            InnerStop();
            //TimeUpdate();
        }
        void InnerStop()
        {
            bStart = false;
        }
        /// <summary>
        /// 巻き戻し
        /// </summary>
        public void Reset()
        {
            Reset(false);
        }
        /// <summary>
        /// 巻き戻し及び逆再生設定
        /// </summary>
        /// <param name="Reverse">逆再生</param>
        public void Reset(bool Reverse)
        {
            bReverse = Reverse;
            bStart = false;
            if (Reverse)
                NowFrame = MaxFrame;
            else
                NowFrame = 0;
            Rewind();
        }
        //更新
        internal void Update(float elapsedSeconds)
        {
            TimeUpdate(elapsedSeconds);
            IMMDXCamera camera = ResolveCamera();
            IMMDXLight light = ResolveLight();
            //カメラの更新
            //カーソル位置の更新
            int CursorPos = cameraPos;
            if (!bReverse)
            {
                for (; CursorPos < cameraFrames.Count && cameraFrames[CursorPos].FrameNo < NowFrame; ++CursorPos) ;
                for (; CursorPos > 0 && cameraFrames[CursorPos - 1].FrameNo > NowFrame; --CursorPos) ;
            }
            else
            {
                for (; CursorPos > 0 && cameraFrames[CursorPos - 1].FrameNo > NowFrame; --CursorPos) ;
                for (; CursorPos < cameraFrames.Count && cameraFrames[CursorPos].FrameNo < NowFrame; ++CursorPos) ;
            }
            cameraPos = CursorPos;
            if (!(CursorPos == 0 || CursorPos == cameraFrames.Count) && camera != null)
            {
                //時間経過取得
                float Progress = ((float)NowFrame - (float)cameraFrames[CursorPos - 1].FrameNo) / ((float)cameraFrames[CursorPos].FrameNo - (float)cameraFrames[CursorPos - 1].FrameNo);
                //差分を適用
                MMDCameraKeyFrame camera1 = cameraFrames[CursorPos - 1], camera2 = cameraFrames[CursorPos];
                MMDCameraKeyFrame.Lerp(camera1, camera2, Progress, camera);
            }
            CursorPos = lightPos;
            if (!bReverse)
            {
                for (; CursorPos < lightFrames.Count && lightFrames[CursorPos].FrameNo < NowFrame; ++CursorPos) ;
                for (; CursorPos > 0 && lightFrames[CursorPos - 1].FrameNo > NowFrame; --CursorPos) ;
            }
            else
            {
                for (; CursorPos > 0 && lightFrames[CursorPos - 1].FrameNo > NowFrame; --CursorPos) ;
                for (; CursorPos < lightFrames.Count && lightFrames[CursorPos].FrameNo < NowFrame; ++CursorPos) ;
            }
            lightPos = CursorPos;
            if (!(CursorPos == 0 || CursorPos == lightFrames.Count) && light != null)
            {
                //時間経過取得
                float Progress = ((float)NowFrame - (float)lightFrames[CursorPos - 1].FrameNo) / ((float)lightFrames[CursorPos].FrameNo - (float)lightFrames[CursorPos - 1].FrameNo);
                MMDLightKeyFrame light1 = lightFrames[CursorPos - 1], light2 = lightFrames[CursorPos];
                MMDLightKeyFrame.Lerp(light1, light2, Progress, light);
            }
        }

        private void TimeUpdate(float elapsedSeconds)
        {
            if (!bStart)
                return;
            //decimal ElapsedTime;
            //ElapsedTime = (decimal)(stopwatch.ElapsedTicks - LastUpdate) * 1000m / (decimal)Stopwatch.Frequency;
            if (!bReverse)
            {
                NowFrame += ((decimal)elapsedSeconds) * FramePerSecond;
            }
            else
            {
                NowFrame -= ((decimal)elapsedSeconds) * FramePerSecond;
            }
            if (NowFrame > MaxFrame)
            {
                if (bLoopPlay)
                    Rewind();
                else
                    InnerStop();
            }
            if (NowFrame < 0)
            {
                if (bLoopPlay)
                    Rewind();
                else
                    InnerStop();
            }
            //LastUpdate = stopwatch.ElapsedTicks;

        }
        //巻き戻し処理
        private void Rewind()
        {
            if (!bReverse)
            {
                while (NowFrame >= MaxFrame)
                    NowFrame -= MaxFrame;
                //ボーンの更新
                cameraPos = 0;
                lightPos = 0;
            }
            else
            {
                while (NowFrame <= 0)
                    NowFrame += MaxFrame;
                //ボーンの更新
                cameraPos = cameraFrames.Count - 1;
                lightPos = lightFrames.Count - 1;
            }
        }
    }
}
