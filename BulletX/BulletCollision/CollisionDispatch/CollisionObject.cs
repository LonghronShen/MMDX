using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{

    public abstract class CollisionObject
    {
        public btTransform WorldTransform;

        ///m_interpolationWorldTransform is used for CCD and interpolation
        ///it can be either previous or future (predicted) transform
        protected btTransform m_interpolationWorldTransform;

        //those two are experimental: just added for bullet time effect, so you can still apply impulses (directly modifying velocities) 
        //without destroying the continuous interpolated motion (which uses this interpolation velocities)
        protected btVector3 m_interpolationLinearVelocity;
        protected btVector3 m_interpolationAngularVelocity;

        protected btVector3 m_anisotropicFriction;
        protected bool m_hasAnisotropicFriction;
        protected float m_contactProcessingThreshold;

        protected BroadphaseProxy m_broadphaseHandle;
        protected CollisionShape m_collisionShape;

        ///m_rootCollisionShape is temporarily used to store the original collision shape
        ///The m_collisionShape might be temporarily replaced by a child collision shape during collision detection purposes
        ///If it is NULL, the m_collisionShape is not temporarily replaced.
        protected CollisionShape m_rootCollisionShape;

        protected CollisionFlags m_collisionFlags;

        protected int m_islandTag1;
        protected int m_companionId;

        protected ActivationStateFlags m_activationState1;
        protected float m_deactivationTime;

        protected float m_friction;
        protected float m_restitution;

        ///m_internalType is reserved to distinguish Bullet's btCollisionObject, btRigidBody, btSoftBody, btGhostObject etc.
        ///do not assign your own m_internalType unless you write a new dynamics object class.
        //int m_internalType;//InternalTypeに吸収
        protected virtual CollisionObjectTypes InternalType { get { return CollisionObjectTypes.CO_COLLISION_OBJECT; } }

        ///users can point to their objects, m_userPointer is not used by Bullet, see setUserPointer/getUserPointer
        protected object m_userObjectPointer;

        ///time of impact calculation
        protected float m_hitFraction;

        ///Swept sphere radius (0.0 by default), see btConvexConvexAlgorithm::
        protected float m_ccdSweptSphereRadius;

        /// Don't do continuous collision detection if the motion (in one step) is less then m_ccdMotionThreshold
        protected float m_ccdMotionThreshold;

        /// If some object should have elaborate collision filtering by sub-classes
        protected bool m_checkCollideWith;

        public virtual bool checkCollideWithOverride(CollisionObject co)
        {
            return true;
        }

        public bool mergesSimulationIslands
        {
            get
            {
                ///static objects, kinematic and object without contact response don't merge islands
                return ((CollisionFlags & (CollisionFlags.CF_STATIC_OBJECT | CollisionFlags.CF_KINEMATIC_OBJECT | CollisionFlags.CF_NO_CONTACT_RESPONSE)) == 0);
            }
        }

        public btVector3 AnisotropicFriction
        {
            get { return m_anisotropicFriction; }
            set
            {
                m_anisotropicFriction = value;
                m_hasAnisotropicFriction = (value.X != 1f) || (value.Y != 1f) || (value.Z != 1f);
            }
        }
        public bool hasAnisotropicFriction { get { return m_hasAnisotropicFriction; } }


        ///the constraint solver can discard solving contacts, if the distance is above this threshold. 0 by default.
        ///Note that using contacts with positive distance can improve stability. It increases, however, the chance of colliding with degerate contacts, such as 'interior' triangle edges
        public float ContactProcessingThreshold { get { return m_contactProcessingThreshold; } set { m_contactProcessingThreshold = value; } }

        public bool isStaticObject { get { return (CollisionFlags & CollisionFlags.CF_STATIC_OBJECT) != 0; } }
        public bool isKinematicObject { get { return (CollisionFlags & CollisionFlags.CF_KINEMATIC_OBJECT) != 0; } }
        public bool isStaticOrKinematicObject { get { return (CollisionFlags & (CollisionFlags.CF_KINEMATIC_OBJECT | CollisionFlags.CF_STATIC_OBJECT)) != 0; } }
        public bool hasContactResponse { get { return (CollisionFlags & CollisionFlags.CF_NO_CONTACT_RESPONSE) == 0; } }

        public CollisionObject()
        {
            m_anisotropicFriction = new btVector3(1f, 1f, 1f);
            m_hasAnisotropicFriction = false;
            m_contactProcessingThreshold = BulletGlobal.BT_LARGE_FLOAT;
            m_broadphaseHandle = null;
            m_collisionShape = null;
            m_rootCollisionShape = null;
            m_collisionFlags = CollisionFlags.CF_STATIC_OBJECT;
            m_islandTag1 = -1;
            m_companionId = -1;
            m_activationState1 = ActivationStateFlags.ACTIVE_TAG;
            m_deactivationTime = 0f;
            m_friction = 0.5f;
            m_restitution = 0f;
            m_userObjectPointer = null;
            m_hitFraction = 1f;
            m_ccdSweptSphereRadius = 0f;
            m_ccdMotionThreshold = 0f;
            m_checkCollideWith = false;
            WorldTransform.setIdentity();
        }

        public virtual CollisionShape CollisionShape
        {
            get { return m_collisionShape; }
            set
            {
                m_collisionShape = value;
                m_rootCollisionShape = value;
            }
        }
        public CollisionShape RootColisionShape { get { return m_rootCollisionShape; } }

        ///Avoid using this internal API call
        ///internalSetTemporaryCollisionShape is used to temporary replace the actual collision shape by a child collision shape.
        public CollisionShape internalTemporaryCollisionShape { set { m_collisionShape = value; } }

        public ActivationStateFlags ActivationState
        {
            get { return m_activationState1; }
            set
            {
                if ((m_activationState1 != ActivationStateFlags.DISABLE_DEACTIVATION) &&
                    (m_activationState1 != ActivationStateFlags.DISABLE_SIMULATION))
                    m_activationState1 = value;
            }
        }

        public float DeactivationTime { get { return m_deactivationTime; } set { m_deactivationTime = value; } }

        public void forceActivationState(ActivationStateFlags newState) { m_activationState1 = newState; }

        public void activate(bool forceActivation)
        {
            if (forceActivation || (CollisionFlags & (CollisionFlags.CF_STATIC_OBJECT | CollisionFlags.CF_KINEMATIC_OBJECT)) == 0)
            {
                ActivationState = ActivationStateFlags.ACTIVE_TAG;
                m_deactivationTime = 0f;
            }
        }
        public bool isActive
        {
            get
            {
                return ((ActivationState != ActivationStateFlags.ISLAND_SLEEPING) && (ActivationState != ActivationStateFlags.DISABLE_SIMULATION));
            }
        }
        public float Restitution { get { return m_restitution; } set { m_restitution = value; } }
        public float Friction { get { return m_friction; } set { m_friction = value; } }
        //public btTransform WorldTransform { get { return m_worldTransform; } set { m_worldTransform = value; } }
        public BroadphaseProxy BroadphaseHandle { get { return m_broadphaseHandle; } set { m_broadphaseHandle = value; } }
        public btTransform InterpolationWorldTransform { get { return m_interpolationWorldTransform; } set { m_interpolationWorldTransform = value; } }
        public btVector3 InterpolationLinearVelocity { get { return m_interpolationLinearVelocity; } set { m_interpolationLinearVelocity = value; } }
        public btVector3 InterpolationAngularVelocity { get { return m_interpolationAngularVelocity; } set { m_interpolationAngularVelocity = value; } }
        public int IslandTag { get { return m_islandTag1; } set { m_islandTag1 = value; } }
        public int CompanionId { get { return m_companionId; } set { m_companionId = value; } }
        public float HitFraction { get { return m_hitFraction; } set { m_hitFraction = value; } }
        public CollisionFlags CollisionFlags { get { return m_collisionFlags; } set { m_collisionFlags = value; } }
        public float CcdSweptSphereRadius { get { return m_ccdSweptSphereRadius; } set { m_ccdSweptSphereRadius = value; } }
        public float CcdMotionThreshold { get { return m_ccdMotionThreshold; } set { m_ccdMotionThreshold = value; } }
        public float CcdSquareMotionThreshold { get { return m_ccdMotionThreshold * m_ccdMotionThreshold; } }
        public object UserData { get { return m_userObjectPointer; } set { m_userObjectPointer = value; } }

        public bool checkCollideWith(CollisionObject co)
        {
            if (m_checkCollideWith)
                return checkCollideWithOverride(co);

            return true;
        }
    }
}
