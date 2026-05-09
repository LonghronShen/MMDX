using System;
using System.Threading;
using System.Threading.Tasks;
using BulletX.BulletDynamics.Dynamics;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.Core.MultiThreads
{
    /// <summary>
    /// 物理エンジンスレッドマネージャ
    /// DI対応、非シングルトン版
    /// </summary>
    public class PhysicsThreadManager
        : IDisposable
    {
        //XBOX用ハードウェアスレッド番号
        public const int XBoxCPUCores = 3;

        //フレーム落ち用タイムアウト時間
        public readonly TimeSpan PhysicsThreadTimeout = TimeSpan.FromMilliseconds(3);//3ms待って帰って来ないようなら物理はフレーム落ち

        //タイムアウトした分のフレーム落ち
        float timeStepTO = 0;
        //フレーム落ち数
        int DFCount = 0;

        //スレッドオブジェクト
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;

        //マルチスレッドモード
        bool bMultiThread = true;
        bool bNextThreadMode = false;

        //シグナル
        AutoResetEvent CalcStart;
        AutoResetEvent CalcFinished;

        //スレッド受け渡し変数
        float m_timeStep = 0;
        //バッファ番号
        int bufferNum = 0;

        /// <summary>DI: 物理ワールド</summary>
        private DiscreteDynamicsWorld m_physicsWorld;
        /// <summary>DI: 物理使用フラグへのアクセス</summary>
        private Func<bool> m_usePhysicsGetter;
        /// <summary>DI: デバッグ描画用コールバック（任意）</summary>
        private Action<float> m_debugDraw;

        /// <summary>
        /// マルチスレッドモードかどうか
        /// </summary>
        public bool IsMultiThread { get { return bMultiThread; } set { bNextThreadMode = value; } }

        /// <summary>
        /// バッファ番号
        /// </summary>
        public int BufferNum { get { return bufferNum; } }

        /// <summary>
        /// 物理エンジン同期処理用イベント
        /// </summary>
        public event Action Synchronize;

        /// <summary>
        /// 物理エンジンスレッドフレーム落ち処理用イベント
        /// </summary>
        public event Action<int> DropFrame;

        /// <summary>
        /// DI コンストラクタ
        /// </summary>
        /// <param name="physicsWorld">物理ワールド</param>
        /// <param name="usePhysicsGetter">UsePhysicsフラグ取得用デリゲート</param>
        /// <param name="debugDraw">デバッグ描画コールバック（任意）</param>
        public PhysicsThreadManager(DiscreteDynamicsWorld physicsWorld, Func<bool> usePhysicsGetter, Action<float> debugDraw = null)
        {
            if (physicsWorld == null) throw new ArgumentNullException(nameof(physicsWorld));
            if (usePhysicsGetter == null) throw new ArgumentNullException(nameof(usePhysicsGetter));

            m_physicsWorld = physicsWorld;
            m_usePhysicsGetter = usePhysicsGetter;
            m_debugDraw = debugDraw;

            CalcFinished = new AutoResetEvent(true);
            CalcStart = new AutoResetEvent(false);

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;

            this.RunPhysicsThread();
        }

        /// <summary>
        /// 簡易コンストラクタ（MMDCoreから呼ばれる）
        /// </summary>
        internal PhysicsThreadManager(DiscreteDynamicsWorld physicsWorld, MMDCore core)
            : this(physicsWorld, () => core.UsePhysics, (ts) => { if (core.Physics.DebugDrawer != null) core.Physics.debugDrawWorld(); })
        {
        }

        private void RunPhysicsThread()
        {
#if NET40
            Task.Factory.StartNew(PhysicsThread);
#else
            Task.Run(PhysicsThread);
#endif
        }

        private void StopPhysicsThread()
        {
            this.cancellationTokenSource.Cancel();
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
                        this.Sync(0);
                        bMultiThread = false;
                        this.StopPhysicsThread();
                    }
                    else
                    {
                        this.Sync(timeStep);
                        CalcStart.Set();
                    }

                    timeStepTO = 0;
                    DFCount = 0;
                }
                else
                {
                    ++DFCount;
                    DropFrame?.Invoke(DFCount);
                    timeStepTO = timeStep;
                }
            }

            if (!bMultiThread)
            {
                if (bNextThreadMode)
                {
                    bMultiThread = true;
                    Sync(timeStep);

                    this.RunPhysicsThread();
                }
                else
                {
                    if (timeStep > 0.0f)
                    {
                        if (m_usePhysicsGetter())
                            m_physicsWorld.stepSimulation(timeStep);
                    }
                }
            }
        }

        private void Sync(float timeStep)
        {
            m_timeStep = timeStep;
            bufferNum = (++bufferNum) % 2;
            if (timeStep > 0.0f && m_debugDraw != null)
            {
                m_debugDraw(timeStep);
            }
            if (timeStep > 0.0f && Synchronize != null)
            {
                Synchronize();
            }
        }

        private void PhysicsThread()
        {
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(XboxCoreNum);
#endif

            while (!this.cancellationToken.IsCancellationRequested)
            {
                if (bMultiThread)
                {
                    if (m_timeStep > 0.0f)
                    {
                        if (m_usePhysicsGetter())
                            m_physicsWorld.stepSimulation(m_timeStep);
                    }
                }
                CalcFinished.Set();
                CalcStart.WaitOne();
            }
        }

        #region IDisposable メンバー
        /// <summary>
        /// スレッドの破棄
        /// </summary>
        public void Dispose()
        {
            this.StopPhysicsThread();
        }
        #endregion
    }
}
