using BulletX.BulletDynamics.Dynamics;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public class SolverConstraint : ObjPoolBase<SolverConstraint>
    {
        internal override void Free()
        {
            ObjPool.Enqueue(this);
        }

        //BT_DECLARE_ALIGNED_ALLOCATOR();

        public btVector3 m_relpos1CrossNormal;
        public btVector3 m_contactNormal;

        public btVector3 m_relpos2CrossNormal;
        //btVector3		m_contactNormal2;//usually m_contactNormal2 == -m_contactNormal

        public btVector3 m_angularComponentA;
        public btVector3 m_angularComponentB;

        public float m_appliedPushImpulse;
        public float m_appliedImpulse;


        public float m_friction;
        public float m_jacDiagABInv;
        public int m_numConsecutiveRowsPerKernel;
        public int m_frictionIndex;
        public RigidBody m_solverBodyA;
        public RigidBody m_solverBodyB;
        public object m_originalContactPoint;

        public float m_rhs;
        public float m_cfm;
        public float m_lowerLimit;
        public float m_upperLimit;

        public float m_rhsPenetration;

        enum SolverConstraintType
        {
            BT_SOLVER_CONTACT_1D = 0,
            BT_SOLVER_FRICTION_1D
        };



        internal void SetZero()
        {
            m_relpos1CrossNormal = btVector3.Zero;
            m_contactNormal = btVector3.Zero;

            m_relpos2CrossNormal = btVector3.Zero;
            //btVector3		m_contactNormal2;//usually m_contactNormal2 == -m_contactNormal

            m_angularComponentA = btVector3.Zero;
            m_angularComponentB = btVector3.Zero;

            m_appliedPushImpulse = 0f;
            m_appliedImpulse = 0f;


            m_friction = 0f;
            m_jacDiagABInv = 0f;
            m_numConsecutiveRowsPerKernel = 0;
            m_frictionIndex = 0;
            m_solverBodyA = null;
            m_solverBodyB = null;
            m_originalContactPoint = null;

            m_rhs = 0f;
            m_cfm = 0f;
            m_lowerLimit = 0f;
            m_upperLimit = 0f;

            m_rhsPenetration = 0f;
        }
    };
}