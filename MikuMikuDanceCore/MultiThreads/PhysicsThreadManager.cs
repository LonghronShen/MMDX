using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MikuMikuDance.Core.MultiThreads
{
    /// <summary>
    /// 物理エンジンスレッドマネージャ
    /// </summary>
    public class PhysicsThreadManager : IDisposable
    {
        //フレーム落ち用タイムアウト時間
        readonly TimeSpan PhysicsThreadTimeout = TimeSpan.FromMilliseconds(3);//3ms待って帰って来ないようなら物理はフレーム落ち
        //タイムアウトした分のフレーム落ち
        float timeStepTO = 0;
        //フレーム落ち数
        int DFCount = 0;
        //XBOX用ハードウェアスレッド番号
        const int XboxCoreNum = 3;
        //スレッドオブジェクト
        Thread thread;
        //マルチスレッドモード
        bool bMultiThread = true;
        bool bNextThreadMode = true;
        //シグナル
        AutoResetEvent CalcStart;
        AutoResetEvent CalcFinished;
        //スレッド受け渡し変数
        float m_timeStep = 0;
        //バッファ番号
        int bufferNum = 0;
        //シングルトン用
        static PhysicsThreadManager m_instanse = null;
        /// <summary>
        /// マルチスレッドモードかどうか
        /// </summary>
        public bool IsMultiThread { get { return bMultiThread; } set { bNextThreadMode = value; } }
        /// <summary>
        /// バッファ番号
        /// </summary>
        public int BufferNum { get { return bufferNum; } }
        /// <summary>
        /// インスタンス
        /// </summary>
        public static PhysicsThreadManager Instanse
        {
            get
            {
                if (m_instanse == null)
                    m_instanse = new PhysicsThreadManager();
                return m_instanse;
            }
        }
        /// <summary>
        /// 物理エンジン同期処理用イベント
        /// </summary>
        public event Action Synchronize;
        /// <summary>
        /// 物理エンジンスレッドフレーム落ち処理用イベント
        /// </summary>
        public event Action<int> DropFrame;

        private PhysicsThreadManager()
        {
            CalcFinished = new AutoResetEvent(true);
            CalcStart = new AutoResetEvent(false);
            thread = new Thread(new ThreadStart(threadFunc));

            thread.Start();
        }
        internal void Update(float timeStep)
        {
            if (bMultiThread)
            {
                timeStep += timeStepTO;
                if (CalcFinished.WaitOne(PhysicsThreadTimeout))
                {
                    if (!bNextThreadMode)
                    {
                        Sync(0);
                        bMultiThread = false;
                        thread.Abort();
                        thread = null;
                    }
                    else
                    {
                        Sync(timeStep);
                        CalcStart.Set();
                    }
                    timeStepTO = 0;
                    DFCount = 0;
                }
                else
                {
                    ++DFCount;
                    if (DropFrame != null)
                        DropFrame(DFCount);
                    timeStepTO = timeStep;
                }
            }
            if (!bMultiThread)
            {
                if (bNextThreadMode)
                {
                    bMultiThread = true;
                    Sync(timeStep);
                    thread = new Thread(new ThreadStart(threadFunc));
                    thread.Start();
                }
                else
                {
                    if (timeStep > 0.0f)
                    {
                        if (MMDCore.Instance.UsePhysics)
                            MMDCore.Instance.Physics.stepSimulation(timeStep);
                    }
                }
            }
        }

        private void Sync(float timeStep)
        {
            m_timeStep = timeStep;
            bufferNum = (++bufferNum) % 2;
            if (timeStep > 0.0f && MMDCore.Instance.Physics.DebugDrawer != null)
            {
                MMDCore.Instance.Physics.debugDrawWorld();
            }
            if (timeStep > 0.0f && Synchronize != null)
            {
                Synchronize();
            }
        }

        private void threadFunc()
        {
#if XBOX360
            thread.SetProcessorAffinity(XboxCoreNum);
#endif
            try
            {
                while (true)
                {
                    if (bMultiThread)
                    {
                        if (m_timeStep > 0.0f)
                        {
                            if (MMDCore.Instance.UsePhysics)
                                MMDCore.Instance.Physics.stepSimulation(m_timeStep);
                        }
                    }
                    CalcFinished.Set();
                    CalcStart.WaitOne();
                }
            }
            catch (ThreadAbortException) { }
        }

        #region IDisposable メンバー
        /// <summary>
        /// スレッドの破棄
        /// </summary>
        public void Dispose()
        {
            if (thread == null)
                return;
            thread.Abort();
            thread = null;
        }

        #endregion
    }
}
