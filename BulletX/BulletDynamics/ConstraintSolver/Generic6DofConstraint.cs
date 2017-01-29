using System;
using System.Diagnostics;
using BulletX.BulletDynamics.Dynamics;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    [Flags]
    public enum bt6DofFlags
    {
        BT_6DOF_FLAGS_NONE = 0,
        BT_6DOF_FLAGS_CFM_NORM = 1,
        BT_6DOF_FLAGS_CFM_STOP = 2,
        BT_6DOF_FLAGS_AXIS_SHIFT = BT_6DOF_FLAGS_CFM_NORM | BT_6DOF_FLAGS_CFM_STOP,
        BT_6DOF_FLAGS_ERP_STOP = 4
    }
    public class Generic6DofConstraint : TypedConstraint
    {
        //defines
        const bool D6_USE_OBSOLETE_METHOD = false;
        const bool D6_USE_FRAME_OFFSET = true;


        //! relative_frames
        //!@{
        protected btTransform m_frameInA;//!< the constraint space w.r.t body A
        btTransform m_frameInB;//!< the constraint space w.r.t body B
        //!@}
        //! Jacobians
        //!@{
        protected JacobianEntry[] m_jacLinear = new JacobianEntry[3];//!< 3 orthogonal linear constraints
        protected JacobianEntry[] m_jacAng = new JacobianEntry[3];//!< 3 orthogonal angular constraints
        //!@}

        //! Linear_Limit_parameters
        //!@{
        protected TranslationalLimitMotor m_linearLimits;
        //!@}


        //! hinge_parameters
        //!@{
        protected RotationalLimitMotor[] m_angularLimits;
        //!@}

#if false
        //! temporal variables
        //!@{
        protected float m_timeStep;
#endif
        protected btTransform m_calculatedTransformA;
        protected btTransform m_calculatedTransformB;
        protected btVector3 m_calculatedAxisAngleDiff;
        protected btVector3[] m_calculatedAxis = new btVector3[3];
        protected btVector3 m_calculatedLinearDiff;
        protected float m_factA;
        protected float m_factB;
        protected bool m_hasStaticBody;

        protected btVector3 m_AnchorPos; // point betwen pivots of bodies A and B to solve linear axes

        protected bool m_useLinearReferenceFrameA;
        protected bool m_useOffsetForConstraintFrame;

        protected bt6DofFlags m_flags;
        ///for backwards compatibility during the transition to 'getInfo/getInfo2'
        public bool m_useSolveConstraintObsolete;

        //!@}
        public Generic6DofConstraint(RigidBody rbA, RigidBody rbB, btTransform frameInA, btTransform frameInB, bool useLinearReferenceFrameA)
            : base(TypedConstraintType.D6_CONSTRAINT_TYPE, rbA, rbB)
        {
            m_linearLimits = new TranslationalLimitMotor();
            m_angularLimits = new RotationalLimitMotor[3];
            for (int i = 0; i < m_angularLimits.Length; i++)
                m_angularLimits[i] = new RotationalLimitMotor();
            m_frameInA = frameInA;
            m_frameInB = frameInB;
            m_useLinearReferenceFrameA = useLinearReferenceFrameA;
            m_useOffsetForConstraintFrame = D6_USE_FRAME_OFFSET;
            m_flags = bt6DofFlags.BT_6DOF_FLAGS_NONE;
            m_useSolveConstraintObsolete = D6_USE_OBSOLETE_METHOD;
            calculateTransforms();
        }
        public Generic6DofConstraint(RigidBody rbB, btTransform frameInB, bool useLinearReferenceFrameB)
            : base(TypedConstraintType.D6_CONSTRAINT_TYPE, getFixedBody(), rbB)
        {
            m_linearLimits = new TranslationalLimitMotor();
            m_angularLimits = new RotationalLimitMotor[3];
            for (int i = 0; i < m_angularLimits.Length; i++)
                m_angularLimits[i] = new RotationalLimitMotor();
            m_frameInB = frameInB;
            m_useLinearReferenceFrameA = useLinearReferenceFrameB;
            m_useOffsetForConstraintFrame = D6_USE_FRAME_OFFSET;
            m_flags = bt6DofFlags.BT_6DOF_FLAGS_NONE;
            m_useSolveConstraintObsolete = false;
            ///not providing rigidbody A means implicitly using worldspace for body A
            m_frameInA = rbB.CenterOfMassTransform * m_frameInB;
            calculateTransforms();
        }

        public void calculateTransforms()
        {
            calculateTransforms(m_rbA.CenterOfMassTransform, m_rbB.CenterOfMassTransform);
        }

        public void calculateTransforms(btTransform transA, btTransform transB)
        {
            m_calculatedTransformA = transA * m_frameInA;
            m_calculatedTransformB = transB * m_frameInB;
            calculateLinearInfo();
            calculateAngleInfo();
            if (m_useOffsetForConstraintFrame)
            {	//  get weight factors depending on masses
                float miA = RigidBodyA.InvMass;
                float miB = RigidBodyB.InvMass;
                m_hasStaticBody = (miA < BulletGlobal.SIMD_EPSILON) || (miB < BulletGlobal.SIMD_EPSILON);
                float miS = miA + miB;
                if (miS > 0f)
                {
                    m_factA = miB / miS;
                }
                else
                {
                    m_factA = 0.5f;
                }
                m_factB = 1.0f - m_factA;
            }
        }
        protected void calculateLinearInfo()
        {
            m_calculatedLinearDiff = m_calculatedTransformB.Origin - m_calculatedTransformA.Origin;
            #region m_calculatedLinearDiff = m_calculatedTransformA.Basis.inverse() * m_calculatedLinearDiff;
            {
                btMatrix3x3 temp1;
                btVector3 temp2;
                m_calculatedTransformA.Basis.inverse(out temp1);
                btMatrix3x3.Multiply(ref temp1, ref m_calculatedLinearDiff, out temp2);
                m_calculatedLinearDiff = temp2;
            }
            #endregion
            for (int i = 0; i < 3; i++)
            {
                m_linearLimits.m_currentLinearDiff[i] = m_calculatedLinearDiff[i];
                m_linearLimits.testLimitValue(i, m_calculatedLinearDiff[i]);
            }
        }
        protected void calculateAngleInfo()
        {
            btMatrix3x3 relative_frame;// = m_calculatedTransformA.Basis.inverse() * m_calculatedTransformB.Basis;
            {
                btMatrix3x3 temp;
                m_calculatedTransformA.Basis.inverse(out temp);
                btMatrix3x3.Multiply(ref temp, ref m_calculatedTransformB.Basis, out relative_frame);
            }
            BulletGlobal.matrixToEulerXYZ(relative_frame, out m_calculatedAxisAngleDiff);
            // in euler angle mode we do not actually constrain the angular velocity
            // along the axes axis[0] and axis[2] (although we do use axis[1]) :
            //
            //    to get			constrain w2-w1 along		...not
            //    ------			---------------------		------
            //    d(angle[0])/dt = 0	ax[1] x ax[2]			ax[0]
            //    d(angle[1])/dt = 0	ax[1]
            //    d(angle[2])/dt = 0	ax[0] x ax[1]			ax[2]
            //
            // constraining w2-w1 along an axis 'a' means that a'*(w2-w1)=0.
            // to prove the result for angle[0], write the expression for angle[0] from
            // GetInfo1 then take the derivative. to prove this for angle[2] it is
            // easier to take the euler rate expression for d(angle[2])/dt with respect
            // to the components of w and set that to 0.
            btVector3 axis0 ;//= m_calculatedTransformB.Basis.getColumn(0);
            m_calculatedTransformB.Basis.getColumn(0, out axis0);
            btVector3 axis2;// = m_calculatedTransformA.Basis.getColumn(2);
            m_calculatedTransformA.Basis.getColumn(2, out axis2);

            m_calculatedAxis[1] = axis2.cross(axis0);
            m_calculatedAxis[0] = m_calculatedAxis[1].cross(axis2);
            m_calculatedAxis[2] = axis0.cross(m_calculatedAxis[1]);

            m_calculatedAxis[0].normalize();
            m_calculatedAxis[1].normalize();
            m_calculatedAxis[2].normalize();

        }
        public void setLinearLowerLimit(btVector3 linearLower)
        {
            m_linearLimits.m_lowerLimit = linearLower;
        }

        public void setLinearUpperLimit(btVector3 linearUpper)
        {
            m_linearLimits.m_upperLimit = linearUpper;
        }

        public void setAngularLowerLimit(btVector3 angularLower)
        {
            for (int i = 0; i < 3; i++)
                m_angularLimits[i].m_loLimit = BulletGlobal.NormalizeAngle(angularLower[i]);
        }

        public void setAngularUpperLimit(btVector3 angularUpper)
        {
            for (int i = 0; i < 3; i++)
                m_angularLimits[i].m_hiLimit = BulletGlobal.NormalizeAngle(angularUpper[i]);
        }




        public override void buildJacobian()
        {
            if (m_useSolveConstraintObsolete)
            {

                // Clear accumulated impulses for the next simulation step
                m_linearLimits.m_accumulatedImpulse = new btVector3(0f, 0f, 0f);
                int i;
                for (i = 0; i < 3; i++)
                {
                    m_angularLimits[i].m_accumulatedImpulse = 0f;
                }
                //calculates transform
                calculateTransforms(m_rbA.CenterOfMassTransform, m_rbB.CenterOfMassTransform);

                //  const btVector3& pivotAInW = m_calculatedTransformA.getOrigin();
                //  const btVector3& pivotBInW = m_calculatedTransformB.getOrigin();
                calcAnchorPos();
                btVector3 pivotAInW = m_AnchorPos;
                btVector3 pivotBInW = m_AnchorPos;

                // not used here
                //    btVector3 rel_pos1 = pivotAInW - m_rbA.getCenterOfMassPosition();
                //    btVector3 rel_pos2 = pivotBInW - m_rbB.getCenterOfMassPosition();

                btVector3 normalWorld;
                //linear part
                for (i = 0; i < 3; i++)
                {
                    if (m_linearLimits.isLimited(i))
                    {
                        if (m_useLinearReferenceFrameA)
                            //normalWorld = m_calculatedTransformA.Basis.getColumn(i);
                            m_calculatedTransformA.Basis.getColumn(i, out normalWorld);
                        else
                            //normalWorld = m_calculatedTransformB.Basis.getColumn(i);
                            m_calculatedTransformB.Basis.getColumn(i, out normalWorld);

                        buildLinearJacobian(
                            ref normalWorld,
                            ref pivotAInW,ref pivotBInW,out m_jacLinear[i]);

                    }
                }

                // angular part
                for (i = 0; i < 3; i++)
                {
                    //calculates error angle
                    if (testAngularLimitMotor(i))
                    {
                        normalWorld = this.getAxis(i);
                        // Create angular atom
                        //m_jacAng[i]=buildAngularJacobian(normalWorld);
                        buildAngularJacobian(ref normalWorld, out m_jacAng[i]);
                    }
                }

            }
        }

        private void buildAngularJacobian(ref btVector3 jointAxisW,out JacobianEntry result)
        {
            /*return new JacobianEntry(jointAxisW,
                                      m_rbA.CenterOfMassTransform.Basis.transpose(),
                                      m_rbB.CenterOfMassTransform.Basis.transpose(),
                                      m_rbA.InvInertiaDiagLocal,
                                      m_rbB.InvInertiaDiagLocal);*/
            btMatrix3x3 temp1, temp2;
            m_rbA.CenterOfMassTransform.Basis.transpose(out temp1);
            m_rbB.CenterOfMassTransform.Basis.transpose(out temp2);
            result= new JacobianEntry(ref jointAxisW,
                                      ref temp1,
                                      ref temp2,
                                      ref m_rbA.InvInertiaDiagLocal,
                                      ref m_rbB.InvInertiaDiagLocal);
        }

        private btVector3 getAxis(int axis_index)
        {
            return m_calculatedAxis[axis_index];
        }

        private void buildLinearJacobian(ref btVector3 normalWorld,ref btVector3 pivotAInW, ref btVector3 pivotBInW,out JacobianEntry result )
        {
            /*return new JacobianEntry(
                m_rbA.CenterOfMassTransform.Basis.transpose(),
                m_rbB.CenterOfMassTransform.Basis.transpose(),
                pivotAInW - m_rbA.CenterOfMassPosition,
                pivotBInW - m_rbB.CenterOfMassPosition,
                normalWorld,
                m_rbA.InvInertiaDiagLocal,
                m_rbA.InvMass,
                m_rbB.InvInertiaDiagLocal,
                m_rbB.InvMass);*/
            btMatrix3x3 temp1, temp2;
            btVector3 temp3, temp4, temp5 = m_rbA.CenterOfMassPosition, temp6 = m_rbB.CenterOfMassPosition;
            m_rbA.CenterOfMassTransform.Basis.transpose(out temp1);
            m_rbB.CenterOfMassTransform.Basis.transpose(out temp2);
            btVector3.Subtract(ref pivotAInW, ref temp5, out temp3);
            btVector3.Subtract(ref pivotBInW, ref temp6, out temp4);
            result= new JacobianEntry(
                ref temp1,
                ref temp2,
                ref temp3,
                ref temp4,
                ref normalWorld,
                ref m_rbA.InvInertiaDiagLocal,
                m_rbA.InvMass,
                ref m_rbB.InvInertiaDiagLocal,
                m_rbB.InvMass);
        }
        public virtual void calcAnchorPos()
        {
            float imA = m_rbA.InvMass;
            float imB = m_rbB.InvMass;
            float weight;
            if (imB == 0.0f)
            {
                weight = 1.0f;
            }
            else
            {
                weight = imA / (imA + imB);
            }
            #region m_AnchorPos = m_calculatedTransformA.Origin * weight + m_calculatedTransformB.Origin * (1.0f - weight);
            {
                btVector3 temp1, temp2;
                btVector3.Multiply(ref m_calculatedTransformA.Origin, weight, out temp1);
                btVector3.Multiply(ref m_calculatedTransformB.Origin, (1.0f - weight), out temp2);
                btVector3.Add(ref temp1, ref temp2, out m_AnchorPos);
            }
            #endregion
            return;
        }
        public bool testAngularLimitMotor(int axis_index)
        {
            float angle = m_calculatedAxisAngleDiff[axis_index];
            angle = TypedConstraint.AdjustAngleToLimits(angle, m_angularLimits[axis_index].m_loLimit, m_angularLimits[axis_index].m_hiLimit);
            m_angularLimits[axis_index].m_currentPosition = angle;
            //test limits
            m_angularLimits[axis_index].testLimitValue(angle);
            return m_angularLimits[axis_index].needApplyTorques();
        }
        
        
        public override void getInfo2(TypedConstraint.ConstraintInfo2 info)
        {
            getInfo2NonVirtual(info, m_rbA.CenterOfMassTransform, m_rbB.CenterOfMassTransform, m_rbA.LinearVelocity, m_rbB.LinearVelocity, m_rbA.AngularVelocity, m_rbB.AngularVelocity);
        }
        void getInfo2NonVirtual(ConstraintInfo2 info, btTransform transA, btTransform transB, btVector3 linVelA, btVector3 linVelB, btVector3 angVelA, btVector3 angVelB)
        {
            Debug.Assert(!m_useSolveConstraintObsolete);
            //prepare constraint
            calculateTransforms(transA, transB);
            if (m_useOffsetForConstraintFrame)
            { // for stability better to solve angular limits first
                int row = setAngularLimits(info, 0, transA, transB, linVelA, linVelB, angVelA, angVelB);
                setLinearLimits(info, row, transA, transB, linVelA, linVelB, angVelA, angVelB);
            }
            else
            { // leave old version for compatibility
                int row = setLinearLimits(info, 0, transA, transB, linVelA, linVelB, angVelA, angVelB);
                setAngularLimits(info, row, transA, transB, linVelA, linVelB, angVelA, angVelB);
            }
        }
        RotationalLimitMotor limot = new RotationalLimitMotor();
        protected int setLinearLimits(ConstraintInfo2 info, int row, btTransform transA, btTransform transB, btVector3 linVelA, btVector3 linVelB, btVector3 angVelA, btVector3 angVelB)
        {
            //	int row = 0;
            //solve linear limits
            limot.Reset();
            for (int i = 0; i < 3; i++)
            {
                if (m_linearLimits.needApplyForce(i))
                { // re-use rotational motor code
                    limot.m_bounce = 0f;
                    limot.m_currentLimit = m_linearLimits.m_currentLimit[i];
                    limot.m_currentPosition = m_linearLimits.m_currentLinearDiff[i];
                    limot.m_currentLimitError = m_linearLimits.m_currentLimitError[i];
                    limot.m_damping = m_linearLimits.m_damping;
                    limot.m_enableMotor = m_linearLimits.m_enableMotor[i];
                    limot.m_hiLimit = m_linearLimits.m_upperLimit[i];
                    limot.m_limitSoftness = m_linearLimits.m_limitSoftness;
                    limot.m_loLimit = m_linearLimits.m_lowerLimit[i];
                    limot.m_maxLimitForce = 0f;
                    limot.m_maxMotorForce = m_linearLimits.m_maxMotorForce[i];
                    limot.m_targetVelocity = m_linearLimits.m_targetVelocity[i];
                    btVector3 axis;// = m_calculatedTransformA.Basis.getColumn(i);
                    m_calculatedTransformA.Basis.getColumn(i, out axis);
                    bt6DofFlags flags = (bt6DofFlags)((int)m_flags >> (i * (int)bt6DofFlags.BT_6DOF_FLAGS_AXIS_SHIFT));
                    //limot.m_normalCFM = (flags & bt6DofFlags.BT_6DOF_FLAGS_CFM_NORM) != 0 ? m_linearLimits.m_normalCFM[i] : info.cfm[0];
                    //limot.m_stopCFM = (flags & bt6DofFlags.BT_6DOF_FLAGS_CFM_STOP) != 0 ? m_linearLimits.m_stopCFM[i] : info.cfm[0];
                    limot.m_normalCFM = (flags & bt6DofFlags.BT_6DOF_FLAGS_CFM_NORM) != 0 ? m_linearLimits.m_normalCFM[i] : info.Constraints[info.CurrentRow].m_cfm;
                    limot.m_stopCFM = (flags & bt6DofFlags.BT_6DOF_FLAGS_CFM_STOP) != 0 ? m_linearLimits.m_stopCFM[i] : info.Constraints[info.CurrentRow].m_cfm;
                    limot.m_stopERP = (flags & bt6DofFlags.BT_6DOF_FLAGS_ERP_STOP) != 0 ? m_linearLimits.m_stopERP[i] : info.erp;
                    if (m_useOffsetForConstraintFrame)
                    {
                        int indx1 = (i + 1) % 3;
                        int indx2 = (i + 2) % 3;
                        bool rotAllowed = true; // rotations around orthos to current axis
                        if (m_angularLimits[indx1].m_currentLimit != 0 && m_angularLimits[indx2].m_currentLimit != 0)
                        {
                            rotAllowed = false;
                        }
                        row += get_limit_motor_info2(limot, transA, transB, linVelA, linVelB, angVelA, angVelB, info, row, ref axis, false, rotAllowed);
                    }
                    else
                    {
                        row += get_limit_motor_info2(limot, transA, transB, linVelA, linVelB, angVelA, angVelB, info, row, ref axis, false);
                    }
                }
            }
            return row;
        }
        protected int setAngularLimits(ConstraintInfo2 info, int row_offset, btTransform transA, btTransform transB, btVector3 linVelA, btVector3 linVelB, btVector3 angVelA, btVector3 angVelB)
        {
            Generic6DofConstraint d6constraint = this;
            int row = row_offset;
            //solve angular limits
            for (int i = 0; i < 3; i++)
            {
                if (d6constraint.getRotationalLimitMotor(i).needApplyTorques())
                {
                    btVector3 axis = d6constraint.getAxis(i);
                    bt6DofFlags flags = (bt6DofFlags)((int)m_flags >> ((i + 3) * (int)bt6DofFlags.BT_6DOF_FLAGS_AXIS_SHIFT));
                    if ((flags & bt6DofFlags.BT_6DOF_FLAGS_CFM_NORM) == 0)
                    {
                        m_angularLimits[i].m_normalCFM = info.Constraints[info.CurrentRow].m_cfm;
                    }
                    if ((flags & bt6DofFlags.BT_6DOF_FLAGS_CFM_STOP) == 0)
                    {
                        m_angularLimits[i].m_stopCFM = info.Constraints[info.CurrentRow].m_cfm;
                    }
                    if ((flags & bt6DofFlags.BT_6DOF_FLAGS_ERP_STOP) == 0)
                    {
                        m_angularLimits[i].m_stopERP = info.erp;
                    }
                    row += get_limit_motor_info2(d6constraint.getRotationalLimitMotor(i),
                                                        transA, transB, linVelA, linVelB, angVelA, angVelB, info, row,ref axis, true);
                }
            }

            return row;
        }

        public RotationalLimitMotor getRotationalLimitMotor(int index)
        {
            return m_angularLimits[index];
        }
        public int get_limit_motor_info2(
    RotationalLimitMotor limot,
    btTransform transA, btTransform transB, btVector3 linVelA, btVector3 linVelB, btVector3 angVelA, btVector3 angVelB,
    ConstraintInfo2 info, int row, ref btVector3 ax1, bool rotational)
        {
            return get_limit_motor_info2(limot, transA, transB, linVelA, linVelB, angVelA, angVelB, info, row, ref ax1, rotational, false);
        }
        public int get_limit_motor_info2(
	        RotationalLimitMotor  limot,
	        btTransform transA,btTransform transB,btVector3 linVelA,btVector3 linVelB,btVector3 angVelA,btVector3 angVelB,
	        ConstraintInfo2 info, int row,ref btVector3 ax1, bool rotational,bool rotAllowed)
        {
            //ポインタを使わず、Listを直接受け取っているので、srow=rowとして、Listに入れるように修正
            //int srow = row * info.rowskip;
            int srow = row;
            
                    bool powered = limot.m_enableMotor;
            int limit = limot.m_currentLimit;
            if (powered || limit!=0)
            {   // if the joint is powered, or has joint limits, add in the extra row

                //rotationalでポインタ分けをしていたのを修正。
#if false
                float *J1 = rotational ? info.m_J1angularAxis : info.m_J1linearAxis;
                J1[srow+0] = ax1[0];
                J1[srow+1] = ax1[1];
                J1[srow+2] = ax1[2];
#endif
                if (rotational)
                    info.Constraints[srow + info.CurrentRow].m_relpos1CrossNormal = ax1;
                else
                    info.Constraints[srow + info.CurrentRow].m_contactNormal = ax1;

                //btScalar* J2 = rotational ? info->m_J2angularAxis : 0;
                if (rotational)
                {
#if false
                    J2[srow+0] = -ax1[0];
                    J2[srow+1] = -ax1[1];
                    J2[srow+2] = -ax1[2];
#endif
                    
                    info.Constraints[srow + info.CurrentRow].m_relpos2CrossNormal = -ax1;
                }
                if((!rotational))
                {
			        if (m_useOffsetForConstraintFrame)
                    {
                        btVector3 tmpA, tmpB, relA, relB;
                        // get vector from bodyB to frameB in WCS
                        relB = m_calculatedTransformB.Origin - transB.Origin;
                        // get its projection to constraint axis
                        btVector3 projB = ax1 * relB.dot(ax1);
                        // get vector directed from bodyB to constraint axis (and orthogonal to it)
                        btVector3 orthoB = relB - projB;
                        // same for bodyA
                        relA = m_calculatedTransformA.Origin - transA.Origin;
                        btVector3 projA = ax1 * relA.dot(ax1);
                        btVector3 orthoA = relA - projA;
                        // get desired offset between frames A and B along constraint axis
                        float desiredOffs = limot.m_currentPosition - limot.m_currentLimitError;
                        // desired vector from projection of center of bodyA to projection of center of bodyB to constraint axis
                        btVector3 totalDist;// = projA + ax1 * desiredOffs - projB;
                        {
                            btVector3 temp1, temp2;
                            btVector3.Multiply(ref ax1, desiredOffs, out temp1);
                            btVector3.Add(ref projA, ref temp1, out temp2);
                            btVector3.Subtract(ref temp2, ref projB, out totalDist);
                        }
                        // get offset vectors relA and relB
                        #region relA = orthoA + totalDist * m_factA;
                        {
                            btVector3 temp;
                            btVector3.Multiply(ref totalDist, m_factA, out temp);
                            btVector3.Add(ref orthoA, ref temp, out relA);
                        }
                        #endregion
                        #region relB = orthoB - totalDist * m_factB;
                        {
                            btVector3 temp;
                            btVector3.Multiply(ref totalDist, m_factB, out temp);
                            btVector3.Subtract(ref orthoB, ref temp, out relB);
                        }
                        #endregion
                        //tmpA = relA.cross(ax1);
                        relA.cross(ref ax1, out tmpA);
                        //tmpB = relB.cross(ax1);
                        relB.cross(ref ax1, out tmpB);
                        if (m_hasStaticBody && (!rotAllowed))
                        {
                            tmpA *= m_factA;
                            tmpB *= m_factB;
                        }
                        //int i;
                        //for (i=0; i<3; i++) info->m_J1angularAxis[srow+i] = tmpA[i];
                        //for (i=0; i<3; i++) info->m_J2angularAxis[srow+i] = -tmpB[i];
                        info.Constraints[srow + info.CurrentRow].m_relpos1CrossNormal = tmpA;
                        info.Constraints[srow + info.CurrentRow].m_relpos2CrossNormal = -tmpB;
                    }
                    else
			        {
				        btVector3 ltd;	// Linear Torque Decoupling vector
				        btVector3 c = m_calculatedTransformB.Origin - transA.Origin;
				        ltd = c.cross(ax1);
#if false
				        info->m_J1angularAxis[srow+0] = ltd[0];
				        info->m_J1angularAxis[srow+1] = ltd[1];
				        info->m_J1angularAxis[srow+2] = ltd[2];
#endif
                        info.Constraints[srow + info.CurrentRow].m_relpos1CrossNormal = ltd;


				        c = m_calculatedTransformB.Origin - transB.Origin;
				        ltd = -c.cross(ax1);

#if false
				        info->m_J2angularAxis[srow+0] = ltd[0];
				        info->m_J2angularAxis[srow+1] = ltd[1];
				        info->m_J2angularAxis[srow+2] = ltd[2];
#endif
                        info.Constraints[srow + info.CurrentRow].m_relpos2CrossNormal = ltd;
			        }
                }
                // if we're limited low and high simultaneously, the joint motor is
                // ineffective
                if (limit!=0 && (limot.m_loLimit == limot.m_hiLimit)) powered = false;
                //info->m_constraintError[srow] = 0f;
                info.Constraints[srow + info.CurrentRow].m_rhs = 0f;
                if (powered)
                {
			        //info->cfm[srow] = limot.m_normalCFM;
                    info.Constraints[srow + info.CurrentRow].m_cfm = limot.m_normalCFM;
                    if(limit==0)
                    {
				        float tag_vel = rotational ? limot.m_targetVelocity : -limot.m_targetVelocity;

				        float mot_fact = getMotorFactor(	limot.m_currentPosition, 
													        limot.m_loLimit,
													        limot.m_hiLimit, 
													        tag_vel, 
													        info.fps * limot.m_stopERP);
#if false
				        info->m_constraintError[srow] += mot_fact * limot.m_targetVelocity;
                        info->m_lowerLimit[srow] = -limot.m_maxMotorForce;
                        info->m_upperLimit[srow] = limot.m_maxMotorForce;
#endif
                        info.Constraints[srow + info.CurrentRow].m_rhs+=mot_fact * limot.m_targetVelocity;
                        info.Constraints[srow + info.CurrentRow].m_lowerLimit = -limot.m_maxMotorForce;
                        info.Constraints[srow + info.CurrentRow].m_upperLimit = limot.m_maxMotorForce;
                    }
                }
                if(limit!=0)
                {
                    float k = info.fps * limot.m_stopERP;
			        if(!rotational)
			        {
				        //info->m_constraintError[srow] += k * limot.m_currentLimitError;
                        info.Constraints[srow + info.CurrentRow].m_rhs += k * limot.m_currentLimitError;

			        }
			        else
			        {
				        //info->m_constraintError[srow] += -k * limot.m_currentLimitError;
                        info.Constraints[srow + info.CurrentRow].m_rhs += -k * limot.m_currentLimitError;
			        }
                    //info->cfm[srow] = limot.m_stopCFM;
                    info.Constraints[srow + info.CurrentRow].m_cfm = limot.m_stopCFM;
                    if (limot.m_loLimit == limot.m_hiLimit)
                    {   // limited low and high simultaneously
                        //info->m_lowerLimit[srow] = float.NegativeInfinity;
                        //info->m_upperLimit[srow] = float.PositiveInfinity;
                        info.Constraints[srow + info.CurrentRow].m_lowerLimit = float.NegativeInfinity;
                        info.Constraints[srow + info.CurrentRow].m_upperLimit = float.PositiveInfinity;
                    }
                    else
                    {
                        if (limit == 1)
                        {
                            //info->m_lowerLimit[srow] = 0;
                            //info->m_upperLimit[srow] = float.PositiveInfinity;
                            info.Constraints[srow + info.CurrentRow].m_lowerLimit = 0;
                            info.Constraints[srow + info.CurrentRow].m_upperLimit = float.PositiveInfinity;
                        }
                        else
                        {
                            //info->m_lowerLimit[srow] = float.NegativeInfinity;
                            //info->m_upperLimit[srow] = 0;
                            info.Constraints[srow + info.CurrentRow].m_lowerLimit = float.NegativeInfinity;
                            info.Constraints[srow + info.CurrentRow].m_upperLimit = 0;
                        }
                        // deal with bounce
                        if (limot.m_bounce > 0)
                        {
                            // calculate joint velocity
                            float vel;
                            if (rotational)
                            {
                                vel = angVelA.dot(ax1);
        //make sure that if no body -> angVelB == zero vec
        //                        if (body1)
                                    vel -= angVelB.dot(ax1);
                            }
                            else
                            {
                                vel = linVelA.dot(ax1);
        //make sure that if no body -> angVelB == zero vec
        //                        if (body1)
                                    vel -= linVelB.dot(ax1);
                            }
                            // only apply bounce if the velocity is incoming, and if the
                            // resulting c[] exceeds what we already have.
                            if (limit == 1)
                            {
                                if (vel < 0)
                                {
                                    float newc = -limot.m_bounce* vel;
                                    //if (newc > info->m_constraintError[srow])
                                    //    info->m_constraintError[srow] = newc;
                                    if (newc > info.Constraints[srow + info.CurrentRow].m_rhs)
                                        info.Constraints[srow + info.CurrentRow].m_rhs = newc;
                                }
                            }
                            else
                            {
                                if (vel > 0)
                                {
                                    float newc = -limot.m_bounce * vel;
                                    //if (newc < info->m_constraintError[srow])
                                    //    info->m_constraintError[srow] = newc;
                                    if (newc < info.Constraints[srow + info.CurrentRow].m_rhs)
                                        info.Constraints[srow + info.CurrentRow].m_rhs = newc;
                                }
                            }
                        }
                    }
                }
                return 1;
            }
            else return 0;
        }



        public override void getInfo1(TypedConstraint.ConstraintInfo1 info)
        {
            if (m_useSolveConstraintObsolete)
            {
                info.m_numConstraintRows = 0;
                info.nub = 0;
            }
            else
            {
                //prepare constraint
                calculateTransforms(m_rbA.CenterOfMassTransform, m_rbB.CenterOfMassTransform);
                info.m_numConstraintRows = 0;
                info.nub = 6;
                int i;
                //test linear limits
                for (i = 0; i < 3; i++)
                {
                    if (m_linearLimits.needApplyForce(i))
                    {
                        info.m_numConstraintRows++;
                        info.nub--;
                    }
                }
                //test angular limits
                for (i = 0; i < 3; i++)
                {
                    if (testAngularLimitMotor(i))
                    {
                        info.m_numConstraintRows++;
                        info.nub--;
                    }
                }
            }
        }

        public btTransform CalculatedTransformA { get { return m_calculatedTransformA; } }
        public btTransform CalculatedTransformB { get { return m_calculatedTransformB; } }
        public float getAngle(int axisIndex)
        {
            return m_calculatedAxisAngleDiff[axisIndex];
        }
        public TranslationalLimitMotor TranslationalLimitMotor { get { return m_linearLimits; } }

    }
}
