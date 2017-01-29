using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletDynamics.ConstraintSolver;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.Dynamics
{
    public delegate void InternalTickCallback(DynamicsWorld world, float timeStep);
    public enum DynamicsWorldType
    {
        BT_SIMPLE_DYNAMICS_WORLD = 1,
        BT_DISCRETE_DYNAMICS_WORLD = 2,
        BT_CONTINUOUS_DYNAMICS_WORLD = 3
    }
    public abstract class DynamicsWorld : CollisionWorld
    {
        public event InternalTickCallback InternalTickCallback;
        public event InternalTickCallback InternalPreTickCallback;
        protected object m_worldUserInfo;
        protected ContactSolverInfo m_solverInfo = new ContactSolverInfo();

        public DynamicsWorld(IDispatcher dispatcher, IBroadphaseInterface broadphase, ICollisionConfiguration collisionConfiguration)
            : base(dispatcher, broadphase, collisionConfiguration)
        {
            InternalTickCallback = null;
            InternalPreTickCallback = null;
            m_worldUserInfo = null;
        }


        ///stepSimulation proceeds the simulation over 'timeStep', units in preferably in seconds.
        ///By default, Bullet will subdivide the timestep in constant substeps of each 'fixedTimeStep'.
        ///in order to keep the simulation real-time, the maximum number of substeps can be clamped to 'maxSubSteps'.
        ///You can disable subdividing the timestep/substepping by passing maxSubSteps=0 as second argument to stepSimulation, but in that case you have to keep the timeStep constant.
        public abstract int stepSimulation(float timeStep);
        public abstract int stepSimulation(float timeStep, int maxSubSteps);
        public abstract int stepSimulation(float timeStep, int maxSubSteps, float fixedTimeStep);

        //public abstract void debugDrawWorld();

        public virtual void addConstraint(TypedConstraint constraint) { }
        public virtual void addConstraint(TypedConstraint constraint, bool disableCollisionsBetweenLinkedBodies) { }

        public virtual void removeConstraint(TypedConstraint constraint) { }

        public abstract void addAction(ActionInterface action);

        public abstract void removeAction(ActionInterface action);

        //once a rigidbody is added to the dynamics world, it will get this gravity assigned
        //existing rigidbodies in the world get gravity assigned too, during this method
        public abstract btVector3 Gravity { get; set; }

        public abstract void synchronizeMotionStates();

        public abstract void addRigidBody(RigidBody body);

        public abstract void removeRigidBody(RigidBody body);

        public abstract IConstraintSolver ConstraintSolver { get; set; }

        public virtual int NumConstraints { get { return 0; } }

        public virtual TypedConstraint getConstraint(int index) { return null; }

        public abstract DynamicsWorldType WorldType { get; }

        public abstract void clearForces();

        /// Set the callback for when an internal tick (simulation substep) happens, optional user info
        //eventに切り替え
        /*public void setInternalTickCallback(InternalTickCallback cb, object worldUserInfo, bool isPreTick)
        {
            if (isPreTick)
            {
                m_internalPreTickCallback = cb;
            }
            else
            {
                m_internalTickCallback = cb;
            }
            m_worldUserInfo = worldUserInfo;
        }*/
        //イベントコール用ルーチン
        protected void OnInternalPreTickCallback(DynamicsWorld world, float timeStep)
        {
            if (InternalPreTickCallback != null)
            {
                InternalPreTickCallback(this, timeStep);
            }
        }
        protected void OnInternalTickCallback(DynamicsWorld world, float timeStep)
        {
            if (InternalTickCallback != null)
            {
                InternalTickCallback(this, timeStep);
            }
        }

        public object WorldUserInfo
        {
            set
            {
                m_worldUserInfo = value;
            }

            get
            {
                return m_worldUserInfo;
            }
        }

        public ContactSolverInfo SolverInfo { get { return m_solverInfo; } }

    }
}
