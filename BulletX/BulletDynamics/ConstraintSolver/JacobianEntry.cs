using System.Diagnostics;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public struct JacobianEntry
    {
        public btVector3 m_linearJointAxis;
        public btVector3 m_aJ;
        public btVector3 m_bJ;
        public btVector3 m_0MinvJt;
        public btVector3 m_1MinvJt;
        //Optimization: can be stored in the w/last component of one of the vectors
        public float m_Adiag;

        public JacobianEntry(
        ref btMatrix3x3 world2A,
        ref btMatrix3x3 world2B,
        ref btVector3 rel_pos1,ref btVector3 rel_pos2,
        ref btVector3 jointAxis,
        ref btVector3 inertiaInvA,
        float massInvA,
        ref btVector3 inertiaInvB,
        float massInvB)
        {
            m_linearJointAxis = jointAxis;
            #region m_aJ = world2A * (rel_pos1.cross(m_linearJointAxis));
            {
                btVector3 temp;// = (rel_pos1.cross(m_linearJointAxis));
                rel_pos1.cross(ref m_linearJointAxis, out temp);
                btMatrix3x3.Multiply(ref world2A, ref temp, out m_aJ);
            }
            #endregion
            #region m_bJ = world2B * (rel_pos2.cross(-m_linearJointAxis));
            {
                btVector3 temp;// = (rel_pos2.cross(-m_linearJointAxis));
                btVector3 temp2;
                btVector3.Minus(ref m_linearJointAxis, out temp2);
                rel_pos2.cross(ref temp2, out temp);
                btMatrix3x3.Multiply(ref world2B, ref temp, out m_bJ);
            }
            #endregion
            m_0MinvJt = inertiaInvA * m_aJ;
            m_1MinvJt = inertiaInvB * m_bJ;
            m_Adiag = massInvA + m_0MinvJt.dot(m_aJ) + massInvB + m_1MinvJt.dot(m_bJ);

            Debug.Assert(m_Adiag > 0.0f);
        }

        //angular constraint between two different rigidbodies
        public JacobianEntry(ref btVector3 jointAxis,
            ref btMatrix3x3 world2A,
            ref btMatrix3x3 world2B,
            ref btVector3 inertiaInvA,
            ref btVector3 inertiaInvB)
        {
            m_linearJointAxis = btVector3.Zero;//(btScalar(0.),btScalar(0.),btScalar(0.));
            //m_aJ = world2A * jointAxis;
            //m_bJ = world2B * -jointAxis;
            btMatrix3x3.Multiply(ref  world2A, ref jointAxis, out m_aJ);
            btVector3 temp = -jointAxis;
            btMatrix3x3.Multiply(ref world2B,ref temp, out m_bJ);

            m_0MinvJt = inertiaInvA * m_aJ;
            m_1MinvJt = inertiaInvB * m_bJ;
            m_Adiag = m_0MinvJt.dot(m_aJ) + m_1MinvJt.dot(m_bJ);

            Debug.Assert(m_Adiag > 0.0f);
        }

        //angular constraint between two different rigidbodies
        public JacobianEntry(ref btVector3 axisInA,
            ref btVector3 axisInB,
            ref btVector3 inertiaInvA,
            ref btVector3 inertiaInvB)
        {
            m_linearJointAxis = btVector3.Zero;//(btScalar(0.),btScalar(0.),btScalar(0.));
            m_aJ = axisInA;
            m_bJ = -axisInB;
            m_0MinvJt = inertiaInvA * m_aJ;
            m_1MinvJt = inertiaInvB * m_bJ;
            m_Adiag = m_0MinvJt.dot(m_aJ) + m_1MinvJt.dot(m_bJ);

            Debug.Assert(m_Adiag > 0.0f);
        }

        //constraint on one rigidbody
        public JacobianEntry(
            ref btMatrix3x3 world2A,
            ref btVector3 rel_pos1,ref btVector3 rel_pos2,
            ref btVector3 jointAxis,
            ref btVector3 inertiaInvA,
            float massInvA)
        {
            m_linearJointAxis = jointAxis;
            #region m_aJ = world2A * (rel_pos1.cross(jointAxis));
            {
                btVector3 temp = (rel_pos1.cross(jointAxis));
                btMatrix3x3.Multiply(ref world2A, ref temp, out m_aJ);
            }
            #endregion
            #region m_bJ = world2A * (rel_pos2.cross(-jointAxis));
            {
                btVector3 temp = (rel_pos2.cross(-jointAxis));
                btMatrix3x3.Multiply(ref world2A, ref temp, out m_bJ);
            }
            #endregion
            m_0MinvJt = inertiaInvA * m_aJ;
            m_1MinvJt = btVector3.Zero;//(btScalar(0.),btScalar(0.),btScalar(0.));
            m_Adiag = massInvA + m_0MinvJt.dot(m_aJ);

            Debug.Assert(m_Adiag > 0.0f);
        }

        float Diagonal { get { return m_Adiag; } }

        // for two constraints on the same rigidbody (for example vehicle friction)
        float getNonDiagonal(ref JacobianEntry jacB, float massInvA)
        {
            float lin = massInvA * this.m_linearJointAxis.dot(jacB.m_linearJointAxis);
            float ang = this.m_0MinvJt.dot(jacB.m_aJ);
            return lin + ang;
        }



        // for two constraints on sharing two same rigidbodies (for example two contact points between two rigidbodies)
        float getNonDiagonal(ref JacobianEntry jacB, float massInvA, float massInvB)
        {
            btVector3 lin = this.m_linearJointAxis * jacB.m_linearJointAxis;
            btVector3 ang0 = this.m_0MinvJt * jacB.m_aJ;
            btVector3 ang1 = this.m_1MinvJt * jacB.m_bJ;
            btVector3 lin0 = massInvA * lin;
            btVector3 lin1 = massInvB * lin;
            btVector3 sum;// = ang0 + ang1 + lin0 + lin1;
            {
                btVector3 temp1, temp2;
                btVector3.Add(ref ang0, ref ang1, out temp1);
                btVector3.Add(ref temp1, ref lin0, out temp2);
                btVector3.Add(ref temp2, ref lin1, out sum);
            }
            return sum.X + sum.Y + sum.Z;
        }

        float getRelativeVelocity(ref btVector3 linvelA,ref btVector3 angvelA,ref btVector3 linvelB,ref btVector3 angvelB)
        {
            btVector3 linrel = linvelA - linvelB;
            btVector3 angvela = angvelA * m_aJ;
            btVector3 angvelb = angvelB * m_bJ;
            linrel *= m_linearJointAxis;
            //angvela += angvelb;
            angvela.Add(ref angvelb);
            //angvela += linrel;
            angvela.Add(ref linrel);
            float rel_vel2 = angvela.X + angvela.Y + angvela.Z;
            return rel_vel2 + BulletGlobal.SIMD_EPSILON;
        }
        //private:


    }
}
