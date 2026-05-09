using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.Stages;
using MikuMikuDance.Core.MultiThreads;
using BulletX.BulletDynamics.Dynamics;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletDynamics.ConstraintSolver;
using BulletX.LinerMath;
using MikuMikuDance.Core.Accessory;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.Core
{
    /// <summary>
    /// MikuMikuDanceコアクラス
    /// </summary>
    public class MMDCore : IDisposable
    {
        /// <summary>
        /// 明示的に設定可能なアンビエントコンテキスト（DI 移行用）。
        /// 従来の static Instance を置き換え、自動生成は行わない。
        /// 呼び出し元が明示的に設定する必要がある。
        /// </summary>
        public static MMDCore Current { get; set; }
        /// <summary>
        /// DI: グラフィックデバイス抽象
        /// </summary>
        protected IMMDGraphicsDevice m_graphicsDevice;
        /// <summary>
        /// DI: コンテンツローダー抽象
        /// </summary>
        protected IMMDContentLoader m_contentLoader;
        //物理エンジンの設定データ
        ICollisionConfiguration config = null;
        CollisionDispatcher dispatcher = null;
        IBroadphaseInterface pairCache = null;
        IConstraintSolver solver = null;

        /// <summary>
        /// 物理スレッドマネージャ（DI対応、非シングルトン）
        /// </summary>
        protected PhysicsThreadManager m_physicsThreadManager;

        internal event Action<float> OnBoneUpdate;
        internal event Action<float> OnSkinUpdate;

        //不透明データ
        Dictionary<string, object> opaqueData = new Dictionary<string, object>();
        
        /// <summary>
        /// MMDXで使用するライト
        /// </summary>
        public IMMDXLight Light { get; set; }
        /// <summary>
        /// MMDXで使用するカメラ
        /// </summary>
        public IMMDXCamera Camera { get; set; }
        /// <summary>
        /// ステージのアニメーションプレイヤー
        /// </summary>
        public StagePlayer StageAnimationPlayer { get; private set; }
        /// <summary>
        /// エッジマネージャ
        /// </summary>
        public IEdgeManager EdgeManager { get; set; }
        /// <summary>
        /// MMDXで使用するIKソルバー
        /// </summary>
        public IIKSolver IKSolver { get; set; }
        /// <summary>
        /// MMDXで使用するIKリミッター
        /// </summary>
        public IIKLimitter IKLimitter { get; set; }
        /// <summary>
        /// 物理演算のワールド
        /// </summary>
        /// <remarks>差し替える場合は他の処理の前に差し替えること。
        /// また、これの処理はマルチスレッドで行われるため、同期処理はUpdate関数を使用すること</remarks>
        public DiscreteDynamicsWorld Physics { get; set; }
        /// <summary>
        /// 物理演算を使用するかどうか。
        /// </summary>
        public bool UsePhysics { get; set; }
#if !XBOX
        /// <summary>
        /// ファイルからモデルを読むファクトリー
        /// </summary>
        /// <remarks>ファイルからモデルを作成する独自機能を拡張する場合に使用</remarks>
        public IMMDModelFactory ModelFactoryFromFile { get; set; }
        /// <summary>
        /// ファイルからモーションを読むファクトリー
        /// </summary>
        public IMMDMotionFactory MotionFactoryFromFile { get; set; }
        /// <summary>
        /// ファイルからアクセサリーを読むファクトリー
        /// </summary>
        public IMMDAccessoryFactory AccessoryFactoryFromFile { get; set; }
        /// <summary>
        /// ファイルからVACを読むファクトリー
        /// </summary>
        public IMMDVACFactory VACFactoryFromFile { get; set; }
#endif
        /// <summary>
        /// 不透明データ
        /// </summary>
        public Dictionary<string, object> OpaqueData { get { return opaqueData; } }

        /// <summary>
        /// DI コンストラクタ（完全版：物理依存を外部注入可能）
        /// </summary>
        /// <param name="device">グラフィックデバイス抽象</param>
        /// <param name="contentLoader">コンテンツローダー抽象</param>
        /// <param name="ikSolver">IKソルバー（省略時はCCDSolver）</param>
        /// <param name="ikLimitter">IKリミッター（省略時はDefaltIKLimitter）</param>
        /// <param name="physicsWorld">物理ワールド（省略時はデフォルト生成）</param>
        /// <param name="usePhysics">物理演算を使用するか（省略時はtrue）</param>
        public MMDCore(
            IMMDGraphicsDevice device, 
            IMMDContentLoader contentLoader,
            IIKSolver ikSolver = null,
            IIKLimitter ikLimitter = null,
            DiscreteDynamicsWorld physicsWorld = null,
            bool? usePhysics = null)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (contentLoader == null) throw new ArgumentNullException(nameof(contentLoader));
            m_graphicsDevice = device;
            m_contentLoader = contentLoader;

            InitCore(ikSolver, ikLimitter, physicsWorld, usePhysics);

            // アンビエントコンテキストとして自動設定
            Current = this;
        }

        /// <summary>
        /// 内部初期化（コンストラクタチェーン用）
        /// </summary>
        private void InitCore(
            IIKSolver ikSolver = null,
            IIKLimitter ikLimitter = null,
            DiscreteDynamicsWorld physicsWorld = null,
            bool? usePhysics = null)
        {
            //カメラとライト
            Camera = new MMDXDefaultCamera();
            Light = new MMDXDefaultLight();
            StageAnimationPlayer = new StagePlayer();
            //IKソルバとリミッター
            IKSolver = ikSolver ?? new CCDSolver();
            IKLimitter = ikLimitter ?? new DefaltIKLimitter();
            //デフォルトファクトリー
#if !(XBOX || PORTABLE)
            ModelFactoryFromFile = null;// new MMDModelPartFromFileFactory(ModelPartFactory);
            MotionFactoryFromFile = new MMDMotionFactory();
            AccessoryFactoryFromFile = null;
            VACFactoryFromFile = new MMDVACFactory();
#endif
            //物理作成（注入可能）
            if (physicsWorld != null)
            {
                Physics = physicsWorld;
            }
            else
            {
                config = new DefaultCollisionConfiguration();
                dispatcher = new CollisionDispatcher(config);
                pairCache = new AxisSweep3(new btVector3(-10000, -10000, -10000), new btVector3(10000, 10000, 10000), 5 * 5 * 5 + 1024, null, false);
                solver = new SequentialImpulseConstraintSolver();
                Physics = new DiscreteDynamicsWorld(dispatcher, pairCache, solver, config);
                Physics.Gravity = new btVector3(0, -9.81f * 5.0f, 0);
            }
#if !XBOX
            UsePhysics = usePhysics ?? true;
#else
            UsePhysics = usePhysics ?? false;
#endif

            // 非シングルトンPhysicsThreadManager
            m_physicsThreadManager = new PhysicsThreadManager(Physics, this);
        }
        
        /// <summary>
        /// 物理スレッドマネージャへのアクセス
        /// </summary>
        public PhysicsThreadManager PhysicsThreadManager
        {
            get { return m_physicsThreadManager; }
        }

#if !XBOX
        /// <summary>
        /// モデルをファイルから読み込む
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>MMDモデル</returns>
        /// <remarks>ファイルから独自手法で読み込む場合に使用。</remarks>
        public MMDModel LoadModelFromFile(string filename)
        {
            return LoadModelFromFile(filename, new Dictionary<string, object>());
        }
        /// <summary>
        /// モデルをファイルから読み込む
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="opaqueData">ファクトリーに渡す不透明データ</param>
        /// <returns>MMDモデル</returns>
        /// <remarks>ファイルから独自手法で読み込む場合に使用。不透明データにはファクトリーに渡すデータを渡す。</remarks>
        public MMDModel LoadModelFromFile(string filename, Dictionary<string, object> opaqueData)
        {
            if (ModelFactoryFromFile == null)
                return null;
            if (opaqueData == null)
                opaqueData = new Dictionary<string, object>();
            return ModelFactoryFromFile.Load(filename, opaqueData);
        }
        /// <summary>
        /// モーションをファイルから読み込む
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>MMDMotion</returns>
        public MMDMotion LoadMotionFromFile(string filename)
        {
            return MotionFactoryFromFile.Load(filename, 1.0f);
        }
        /// <summary>
        /// アクセサリをファイルから読み込む
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>MMDAccessoryBase</returns>
        public MMDAccessoryBase LoadAccessoryFromFile(string filename)
        {
            return AccessoryFactoryFromFile.Load(filename);
        }
        /// <summary>
        /// VACをファイルから読み込む
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>MikuMikuDance VAC</returns>
        public MMD_VAC LoadVACFromFile(string filename)
        {
            return LoadVACFromFile(filename, true);
        }
        /// <summary>
        /// VACをファイルから読み込む
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="leftHanded">左手座標系</param>
        /// <returns>MikuMikuDance VAC</returns>
        public MMD_VAC LoadVACFromFile(string filename, bool leftHanded)
        {
            return VACFactoryFromFile.Load(filename, leftHanded);
        }
#endif
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="timeStep">経過時間</param>
        public void Update(float timeStep)
        {
            StageAnimationPlayer.Update(timeStep);
            if (OnBoneUpdate != null)
                OnBoneUpdate(timeStep);
            m_physicsThreadManager.Update(timeStep);
            if (OnSkinUpdate != null)
                OnSkinUpdate(timeStep);
        }

        #region IDisposable メンバー
        #region 抽象へのアクセサ

        /// <summary>
        /// グラフィックデバイス抽象
        /// </summary>
        public IMMDGraphicsDevice GraphicsDevice
        {
            get { return m_graphicsDevice; }
        }

        /// <summary>
        /// コンテンツローダー抽象
        /// </summary>
        public IMMDContentLoader ContentLoader
        {
            get { return m_contentLoader; }
        }

        /// <summary>
        /// DI: ゲーム起動後にグラフィックデバイスを注入（XNA GraphicsDevice は Game.Run 後でないと生成されない）
        /// </summary>
        /// <param name="device">グラフィックデバイス抽象</param>
        public void SetGraphicsDevice(IMMDGraphicsDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            m_graphicsDevice = device;
        }

        #endregion

        /// <summary>
        /// 終了時に呼び出す。
        /// </summary>
        public virtual void Dispose()
        {
            if (m_physicsThreadManager != null)
            {
                m_physicsThreadManager.Dispose();
                m_physicsThreadManager = null;
            }
        }

        #endregion
    }
}
