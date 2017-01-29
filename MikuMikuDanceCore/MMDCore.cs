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
#if XNA
using Microsoft.Xna.Framework.Content;
#endif

namespace MikuMikuDance.Core
{
    /// <summary>
    /// MikuMikuDanceコアクラス
    /// </summary>
    public class MMDCore : IDisposable
    {
        /// <summary>
        /// シングルトンオブジェクト
        /// </summary>
        protected static MMDCore m_inst;
        //物理エンジンの設定データ
        ICollisionConfiguration config = null;
        CollisionDispatcher dispatcher = null;
        IBroadphaseInterface pairCache = null;
        IConstraintSolver solver = null;

        internal event Action<float> OnBoneUpdate;
        internal event Action<float> OnSkinUpdate;

        //不透明データ
        Dictionary<string, object> opaqueData = new Dictionary<string, object>();

        /// <summary>
        /// Singletonインスタンス
        /// </summary>
        public static MMDCore Instance
        {
            get
            {
                if (m_inst == null)
                    throw new MMDXException("MMDCore.Instanceは各継承先(MMDXCore, SlimMMDXCore等)のInstanceより先に使用することは出来ません。各継承先のInstanceを先に使用してください");
                return m_inst;
            }
        }
        
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
        /// コンストラクタ
        /// </summary>
        protected MMDCore()
        {
            //外部からつくらせない
            //カメラとライト
            Camera = new MMDXDefaultCamera();
            Light = new MMDXDefaultLight();
            StageAnimationPlayer = new StagePlayer();
            //IKソルバとリミッター
            IKSolver = new CCDSolver();
            IKLimitter = new DefaltIKLimitter();
            //デフォルトファクトリー
#if !XBOX
            ModelFactoryFromFile = null;// new MMDModelPartFromFileFactory(ModelPartFactory);
            MotionFactoryFromFile = new MMDMotionFactory();
            AccessoryFactoryFromFile = null;
            VACFactoryFromFile = new MMDVACFactory();
#endif
            //物理作成
            //物理エンジンの作成
            config = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(config);
            pairCache = new AxisSweep3(new btVector3(-10000, -10000, -10000), new btVector3(10000, 10000, 10000), 5 * 5 * 5 + 1024, null, false);
            solver = new SequentialImpulseConstraintSolver();
            Physics = new DiscreteDynamicsWorld(dispatcher, pairCache, solver, config);
            Physics.Gravity = new btVector3(0, -9.81f * 5.0f, 0);
#if !XBOX
            UsePhysics = true;
#else
            UsePhysics = false;//XBoxのパフォーマンス問題(物理はやっぱり重たいので入れないほうが早い)
#endif

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
            PhysicsThreadManager.Instanse.Update(timeStep);
            if (OnSkinUpdate != null)
                OnSkinUpdate(timeStep);
        }

        #region IDisposable メンバー
        /// <summary>
        /// 終了時に呼び出す。
        /// </summary>
        public virtual void Dispose()
        {
            PhysicsThreadManager.Instanse.Dispose();
            m_inst = null;
        }

        #endregion
    }
}
