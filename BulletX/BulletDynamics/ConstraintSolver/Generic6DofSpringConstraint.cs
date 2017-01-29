using System;
using System.Diagnostics;
using BulletX.BulletDynamics.Dynamics;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    /// <summary>
    /// 6自由度バネ付き拘束
    /// </summary>
    public class Generic6DofSpringConstraint : Generic6DofConstraint
    {
        protected bool[] m_springEnabled = new bool[6];
        protected float[] m_equilibriumPoint = new float[6];
        protected float[] m_springStiffness = new float[6];
        protected float[] m_springDamping = new float[6]; // between 0 and 1 (1 == no damping)

        public Generic6DofSpringConstraint(RigidBody rbA, RigidBody rbB, btTransform frameInA, btTransform frameInB, bool useLinearReferenceFrameA)
            : base(rbA, rbB, frameInA, frameInB, useLinearReferenceFrameA)
        {
            for (int i = 0; i < 6; i++)
            {
                m_springEnabled[i] = false;
                m_equilibriumPoint[i] = 0f;
                m_springStiffness[i] = 0f;
                m_springDamping[i] = 1f;
            }
        }
        public void enableSpring(int index, bool onOff)
        {
            Debug.Assert((index >= 0) && (index < 6));
            m_springEnabled[index] = onOff;
            if (index < 3)
            {
                m_linearLimits.m_enableMotor[index] = onOff;
            }
            else
            {
                m_angularLimits[index - 3].m_enableMotor = onOff;
            }
        }
        public void setStiffness(int index, float stiffness)
        {
            Debug.Assert((index >= 0) && (index < 6));
            m_springStiffness[index] = stiffness;
        }
        public void setDamping(int index, float damping)
        {
            Debug.Assert((index >= 0) && (index < 6));
            m_springDamping[index] = damping;
        }
        
        public void setEquilibriumPoint() // set the current constraint position/orientation as an equilibrium point for all DOF
        {
            calculateTransforms();
            int i;

            for (i = 0; i < 3; i++)
            {
                m_equilibriumPoint[i] = m_calculatedLinearDiff[i];
            }
            for (i = 0; i < 3; i++)
            {
                m_equilibriumPoint[i + 3] = m_calculatedAxisAngleDiff[i];
            }
        }
        public void setEquilibriumPoint(int index)  // set the current constraint position/orientation as an equilibrium point for given DOF
        {
            Debug.Assert((index >= 0) && (index < 6));
            calculateTransforms();
            if (index < 3)
            {
                m_equilibriumPoint[index] = m_calculatedLinearDiff[index];
            }
            else
            {
                m_equilibriumPoint[index] = m_calculatedAxisAngleDiff[index - 3];
            }
        }
        public override void getInfo2(ConstraintInfo2 info)
        {
            // this will be called by constraint solver at the constraint setup stage
	        // set current motor parameters
	        internalUpdateSprings(info);
	        // do the rest of job for constraint setup
	        base.getInfo2(info);
        }
        protected void internalUpdateSprings(ConstraintInfo2 info)
        {
            // it is assumed that calculateTransforms() have been called before this call
            int i;
            btVector3 relVel = m_rbB.LinearVelocity - m_rbA.LinearVelocity;
            for (i = 0; i < 3; i++)
            {
                if (m_springEnabled[i])
                {
                    // get current position of constraint
                    float currPos = m_calculatedLinearDiff[i];
                    // calculate difference
                    float delta = currPos - m_equilibriumPoint[i];
                    // spring force is (delta * m_stiffness) according to Hooke's Law
                    float force = delta * m_springStiffness[i];
                    float velFactor = info.fps * m_springDamping[i] / info.m_numIterations;
                    m_linearLimits.m_targetVelocity[i] = velFactor * force;
                    m_linearLimits.m_maxMotorForce[i] = (float)Math.Abs(force) / info.fps;
                }
            }
            for (i = 0; i < 3; i++)
            {
                if (m_springEnabled[i + 3])
                {
                    // get current position of constraint
                    float currPos = m_calculatedAxisAngleDiff[i];
                    // calculate difference
                    float delta = currPos - m_equilibriumPoint[i + 3];
                    // spring force is (-delta * m_stiffness) according to Hooke's Law
                    float force = -delta * m_springStiffness[i + 3];
                    float velFactor = info.fps * m_springDamping[i + 3] / info.m_numIterations;
                    m_angularLimits[i].m_targetVelocity = velFactor * force;
                    m_angularLimits[i].m_maxMotorForce = (float)Math.Abs(force) / info.fps;
                }
            }
        }
        

    }
}
