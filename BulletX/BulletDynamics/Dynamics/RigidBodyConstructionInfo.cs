using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.Dynamics
{
    ///The btRigidBodyConstructionInfo structure provides information to create a rigid body. Setting mass to zero creates a fixed (non-dynamic) rigid body.
	///For dynamic objects, you can use the collision shape to approximate the local inertia tensor, otherwise use the zero vector (default argument)
	///You can use the motion state to synchronize the world transform between physics and graphics objects. 
	///And if the motion state is provided, the rigid body will initialize its initial world transform from the motion state,
	///m_startWorldTransform is only used when you don't provide a motion state.
    public class RigidBodyConstructionInfo
    {
        public float m_mass;

        ///When a motionState is provided, the rigid body will initialize its world transform from the motion state
        ///In this case, m_startWorldTransform is ignored.
        public IMotionState m_motionState;
        public btTransform m_startWorldTransform;

        public CollisionShape m_collisionShape;
        public btVector3 m_localInertia;
        public float m_linearDamping;
        public float m_angularDamping;

        ///best simulation results when friction is non-zero
        public float m_friction;
        ///best simulation results using zero restitution.
        public float m_restitution;

        public float m_linearSleepingThreshold;
        public float m_angularSleepingThreshold;

        //Additional damping can help avoiding lowpass jitter motion, help stability for ragdolls etc.
        //Such damping is undesirable, so once the overall simulation quality of the rigid body dynamics system has improved, this should become obsolete
        public bool m_additionalDamping;
        public float m_additionalDampingFactor;
        public float m_additionalLinearDampingThresholdSqr;
        public float m_additionalAngularDampingThresholdSqr;
        public float m_additionalAngularDampingFactor;

        public RigidBodyConstructionInfo(float mass, IMotionState motionState, CollisionShape collisionShape, btVector3 localInertia)
        {
            m_mass = mass;
            m_motionState = motionState;
            m_collisionShape = collisionShape;
            m_localInertia = localInertia;
            m_linearDamping = 0f;
            m_angularDamping = 0f;
            m_friction = 0.5f;
            m_restitution = 0f;
            m_linearSleepingThreshold = 0.8f;
            m_angularSleepingThreshold = 1f;
            m_additionalDamping = false;
            m_additionalDampingFactor = 0.005f;
            m_additionalLinearDampingThresholdSqr = 0.01f;
            m_additionalAngularDampingThresholdSqr = 0.01f;
            m_additionalAngularDampingFactor = 0.01f;
            m_startWorldTransform.setIdentity();
        }
    }
}
