using System;
using System.Collections.Generic;
using BulletX.BulletDynamics.Dynamics;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public abstract class TypedConstraint : TypedObject<TypedConstraintType>
    {
        const float DEFAULT_DEBUGDRAW_SIZE = 0.3f;

        public class ConstraintInfo1 : ObjPoolBase<ConstraintInfo1>
        {


            public int m_numConstraintRows, nub;

            internal override void Free()
            {
                ObjPool.Enqueue(this);
            }
        }
        public class ConstraintInfo2 : ObjPoolBase<ConstraintInfo2>
        {
            internal override void Free()
            {
                ObjPool.Enqueue(this);
            }

            // integrator parameters: frames per second (1/stepsize), default error
            // reduction parameter (0..1).
            public float fps, erp;

            //下のポインタの代用
            public IList<SolverConstraint> Constraints;
            public int CurrentRow;
#if false
		    // for the first and second body, pointers to two (linear and angular)
		    // n*3 jacobian sub matrices, stored by rows. these matrices will have
		    // been initialized to 0 on entry. if the second body is zero then the
		    // J2xx pointers may be 0.
		    public float* m_J1linearAxis,m_J1angularAxis,m_J2linearAxis,m_J2angularAxis;

		    // elements to jump from one row to the next in J's
		    //public int rowskip;

		    // right hand sides of the equation J*v = c + cfm * lambda. cfm is the
		    // "constraint force mixing" vector. c is set to zero on entry, cfm is
		    // set to a constant value (typically very small or zero) value on entry.
		    public float* m_constraintError,cfm;

		    // lo and hi limits for variables (set to -/+ infinity on entry).
		    public float* m_lowerLimit,m_upperLimit;

		    // findex vector for variables. see the LCP solver interface for a
		    // description of what this does. this is set to -1 on entry.
		    // note that the returned indexes are relative to the first index of
		    // the constraint.
		    public int *findex;
#endif
            // number of solver iterations
            public int m_numIterations;
        }
        private int m_userConstraintType;
        private int m_userConstraintId;
        private bool m_needsFeedback;
        protected RigidBody m_rbA;
        protected RigidBody m_rbB;
        protected float m_appliedImpulse;
        internal float internalAppliedImpulse { get { return m_appliedImpulse; } set { m_appliedImpulse = value; } }
        protected float m_dbgDrawSize;
        public float DbgDrawSize { get { return m_dbgDrawSize; } set { m_dbgDrawSize = value; } }
        public TypedConstraintType ConstraintType { get { return m_objectType; } }

        public int UserConstraintType { get { return m_userConstraintType; } set { m_userConstraintType = value; } }
        public bool needsFeedback { get { return m_needsFeedback; } set { m_needsFeedback = value; } }
        public int userConstraintID { get { return m_userConstraintId; } set { m_userConstraintId = value; } }
        public RigidBody RigidBodyA { get { return m_rbA; } }
        public RigidBody RigidBodyB { get { return m_rbB; } }

        public TypedConstraint(TypedConstraintType type, RigidBody rbA)
            : base(type)
        {
            m_userConstraintType = -1;
            m_userConstraintId = -1;
            m_needsFeedback = false;
            m_rbA = rbA;
            m_rbB = getFixedBody();
            m_appliedImpulse = 0f;
            m_dbgDrawSize = DEFAULT_DEBUGDRAW_SIZE;
        }
        public TypedConstraint(TypedConstraintType type, RigidBody rbA, RigidBody rbB)
            : base(type)
        {
            m_userConstraintType = -1;
            m_userConstraintId = -1;
            m_needsFeedback = false;
            m_rbA = rbA;
            m_rbB = rbB;
            m_appliedImpulse = 0f;
            m_dbgDrawSize = DEFAULT_DEBUGDRAW_SIZE;

        }

        static RigidBody s_fixed = new RigidBody(0, null, null, btVector3.Zero);
        protected static RigidBody getFixedBody()
        {
            s_fixed.setMassProps(0f, new btVector3(0f, 0f, 0f));
            return s_fixed;
        }

        ///internal method used by the constraint solver, don't use them directly
        public abstract void buildJacobian();

        public abstract void getInfo1(ConstraintInfo1 info);
        public abstract void getInfo2(ConstraintInfo2 info);

        public static float AdjustAngleToLimits(float angleInRadians, float angleLowerLimitInRadians, float angleUpperLimitInRadians)
        {
            if (angleLowerLimitInRadians >= angleUpperLimitInRadians)
            {
                return angleInRadians;
            }
            else if (angleInRadians < angleLowerLimitInRadians)
            {
                float diffLo = BulletGlobal.NormalizeAngle(angleLowerLimitInRadians - angleInRadians); // this is positive
                float diffHi = (float)Math.Abs(BulletGlobal.NormalizeAngle(angleUpperLimitInRadians - angleInRadians));
                return (diffLo < diffHi) ? angleInRadians : (angleInRadians + BulletGlobal.SIMD_2_PI);
            }
            else if (angleInRadians > angleUpperLimitInRadians)
            {
                float diffHi = BulletGlobal.NormalizeAngle(angleInRadians - angleUpperLimitInRadians); // this is positive
                float diffLo = (float)Math.Abs(BulletGlobal.NormalizeAngle(angleInRadians - angleLowerLimitInRadians));
                return (diffLo < diffHi) ? (angleInRadians - BulletGlobal.SIMD_2_PI) : angleInRadians;
            }
            else
            {
                return angleInRadians;
            }
        }
        protected float getMotorFactor(float pos, float lowLim, float uppLim, float vel, float timeFact)
        {
            if (lowLim > uppLim)
            {
                return 1.0f;
            }
            else if (lowLim == uppLim)
            {
                return 0.0f;
            }
            float lim_fact = 1.0f;
            float delta_max = vel / timeFact;
            if (delta_max < 0.0f)
            {
                if ((pos >= lowLim) && (pos < (lowLim - delta_max)))
                {
                    lim_fact = (lowLim - pos) / delta_max;
                }
                else if (pos < lowLim)
                {
                    lim_fact = 0.0f;
                }
                else
                {
                    lim_fact = 1.0f;
                }
            }
            else if (delta_max > 0.0f)
            {
                if ((pos <= uppLim) && (pos > (uppLim - delta_max)))
                {
                    lim_fact = (uppLim - pos) / delta_max;
                }
                else if (pos > uppLim)
                {
                    lim_fact = 0.0f;
                }
                else
                {
                    lim_fact = 1.0f;
                }
            }
            else
            {
                lim_fact = 0.0f;
            }
            return lim_fact;
        }
        //internal method used by the constraint solver, don't use them directly
        public virtual void solveConstraintObsolete(RigidBody bodyA, RigidBody bodyB, float timeStep) { }
    }
}
