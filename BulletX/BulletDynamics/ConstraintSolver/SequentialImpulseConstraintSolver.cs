using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.BulletDynamics.Dynamics;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public class ConstraintArray : List<SolverConstraint>
    {
        public void resize(int newsize)
        {
            while (Count < newsize)
                Add(SolverConstraint.GetFromPool());
            if (newsize < Count)
            {
                for (int i = newsize; i < Count; i++)
                {
                    this[i].Free();
                }
                this.RemoveRange(newsize, Count - newsize);
            }
        }
    }
    public class ConstraintInfo1Array : List<TypedConstraint.ConstraintInfo1>
    {
        public void resize(int newsize)
        {
            while (Count < newsize)
                Add(TypedConstraint.ConstraintInfo1.GetFromPool());
            if (newsize < Count)
            {
                for (int i = newsize; i < Count; i++)
                {
                    this[i].Free();
                }
                this.RemoveRange(newsize, Count - newsize);
            }
        }
    }
    public class SequentialImpulseConstraintSolver : IConstraintSolver
    {
        protected ConstraintArray m_tmpSolverContactConstraintPool = new ConstraintArray();
        protected ConstraintArray m_tmpSolverNonContactConstraintPool = new ConstraintArray();
        protected ConstraintArray m_tmpSolverContactFrictionConstraintPool = new ConstraintArray();
        protected List<int> m_orderTmpConstraintPool = new List<int>();
        protected List<int> m_orderFrictionConstraintPool = new List<int>();
        protected ConstraintInfo1Array m_tmpConstraintSizesPool = new ConstraintInfo1Array();

        protected void setupFrictionConstraint(SolverConstraint solverConstraint, btVector3 normalAxis, RigidBody solverBodyA, RigidBody solverBodyB, ManifoldPoint cp, btVector3 rel_pos1, btVector3 rel_pos2, CollisionObject colObj0, CollisionObject colObj1, float relaxation, float desiredVelocity, float cfmSlip)
        {
            RigidBody body0 = colObj0 as RigidBody;
            RigidBody body1 = colObj1 as RigidBody;

            solverConstraint.m_contactNormal = normalAxis;

            solverConstraint.m_solverBodyA = body0 != null ? body0 : FixedBody;
            solverConstraint.m_solverBodyB = body1 != null ? body1 : FixedBody;

            solverConstraint.m_friction = cp.m_combinedFriction;
            solverConstraint.m_originalContactPoint = null;

            solverConstraint.m_appliedImpulse = 0f;
            solverConstraint.m_appliedPushImpulse = 0f;

            {
                btVector3 ftorqueAxis1 = rel_pos1.cross(solverConstraint.m_contactNormal);
                solverConstraint.m_relpos1CrossNormal = ftorqueAxis1;
                #region solverConstraint.m_angularComponentA = body0 != null ? body0.InvInertiaTensorWorld * ftorqueAxis1 * body0.AngularFactor : btVector3.Zero;//(0,0,0);
                {
                    if (body0 != null)
                    {
                        btVector3 temp;
                        btMatrix3x3.Multiply(ref body0.InvInertiaTensorWorld, ref ftorqueAxis1, out temp);
                        solverConstraint.m_angularComponentA = temp * body0.AngularFactor;
                    }
                    else
                    {
                        solverConstraint.m_angularComponentA = btVector3.Zero;
                    }
                }
                #endregion
            }
            {
                btVector3 ftorqueAxis1 = rel_pos2.cross(-solverConstraint.m_contactNormal);
                solverConstraint.m_relpos2CrossNormal = ftorqueAxis1;
                #region solverConstraint.m_angularComponentB = body1 != null ? body1.InvInertiaTensorWorld * ftorqueAxis1 * body1.AngularFactor : btVector3.Zero;//btVector3(0,0,0);
                if (body1 != null)
                {
                    btVector3 temp;
                    btMatrix3x3.Multiply(ref body1.InvInertiaTensorWorld, ref ftorqueAxis1, out temp);
                    solverConstraint.m_angularComponentB = temp * body1.AngularFactor;
                }
                else
                {
                    solverConstraint.m_angularComponentB = btVector3.Zero;
                }
                #endregion
            }
#if COMPUTE_IMPULSE_DENOM
	        float denom0 = rb0.computeImpulseDenominator(pos1,solverConstraint.m_contactNormal);
	        float denom1 = rb1.computeImpulseDenominator(pos2,solverConstraint.m_contactNormal);
#else
            btVector3 vec;
            float denom0 = 0f;
            float denom1 = 0f;
            if (body0 != null)
            {
                vec = (solverConstraint.m_angularComponentA).cross(rel_pos1);
                denom0 = body0.InvMass + normalAxis.dot(vec);
            }
            if (body1 != null)
            {
                vec = (-solverConstraint.m_angularComponentB).cross(rel_pos2);
                denom1 = body1.InvMass + normalAxis.dot(vec);
            }


#endif //COMPUTE_IMPULSE_DENOM
            float denom = relaxation / (denom0 + denom1);
            solverConstraint.m_jacDiagABInv = denom;

#if _USE_JACOBIAN
	        solverConstraint.m_jac =  btJacobianEntry (
		        rel_pos1,rel_pos2,solverConstraint.m_contactNormal,
		        body0->getInvInertiaDiagLocal(),
		        body0->getInvMass(),
		        body1->getInvInertiaDiagLocal(),
		        body1->getInvMass());
#endif //_USE_JACOBIAN


            {
                float rel_vel;
                float vel1Dotn = solverConstraint.m_contactNormal.dot(body0 != null ? body0.LinearVelocity : btVector3.Zero)
                    + solverConstraint.m_relpos1CrossNormal.dot(body0 != null ? body0.AngularVelocity : btVector3.Zero);
                float vel2Dotn = -solverConstraint.m_contactNormal.dot(body1 != null ? body1.LinearVelocity : btVector3.Zero)
                    + solverConstraint.m_relpos2CrossNormal.dot(body1 != null ? body1.AngularVelocity : btVector3.Zero);

                rel_vel = vel1Dotn + vel2Dotn;

                //		btScalar positionalError = 0.f;

                float velocityError = desiredVelocity - rel_vel;
                float velocityImpulse = velocityError * solverConstraint.m_jacDiagABInv;
                solverConstraint.m_rhs = velocityImpulse;
                solverConstraint.m_cfm = cfmSlip;
                solverConstraint.m_lowerLimit = 0;
                solverConstraint.m_upperLimit = 1e10f;
            }
        }
        protected SolverConstraint addFrictionConstraint(btVector3 normalAxis, RigidBody solverBodyA, RigidBody solverBodyB, int frictionIndex, ManifoldPoint cp, btVector3 rel_pos1, btVector3 rel_pos2, CollisionObject colObj0, CollisionObject colObj1, float relaxation, float desiredVelocity, float cfmSlip)
        {
            SolverConstraint solverConstraint = SolverConstraint.GetFromPool();// m_tmpSolverContactFrictionConstraintPool.expandNonInitializing();
            m_tmpSolverContactFrictionConstraintPool.Add(solverConstraint);
            solverConstraint.m_frictionIndex = frictionIndex;
            setupFrictionConstraint(solverConstraint, normalAxis, solverBodyA, solverBodyB, cp, rel_pos1, rel_pos2,
                                    colObj0, colObj1, relaxation, desiredVelocity, cfmSlip);
            return solverConstraint;
        }
        protected void setupContactConstraint(SolverConstraint solverConstraint, CollisionObject colObj0, CollisionObject colObj1, ManifoldPoint cp,
                                ContactSolverInfo infoGlobal, out btVector3 vel, out float rel_vel, out float relaxation,
                                out btVector3 rel_pos1, out btVector3 rel_pos2)
        {
            RigidBody rb0 = colObj0 as RigidBody;
            RigidBody rb1 = colObj1 as RigidBody;

            btVector3 pos1 = cp.PositionWorldOnA;
            btVector3 pos2 = cp.PositionWorldOnB;

            //			btVector3 rel_pos1 = pos1 - colObj0->getWorldTransform().getOrigin(); 
            //			btVector3 rel_pos2 = pos2 - colObj1->getWorldTransform().getOrigin();
            rel_pos1 = pos1 - colObj0.WorldTransform.Origin;
            rel_pos2 = pos2 - colObj1.WorldTransform.Origin;

            relaxation = 1f;

            btVector3 torqueAxis0 = rel_pos1.cross(cp.m_normalWorldOnB);
            #region solverConstraint.m_angularComponentA = rb0 != null ? rb0.InvInertiaTensorWorld * torqueAxis0 * rb0.AngularFactor : btVector3.Zero;
            if (rb0 != null)
            {
                btVector3 temp;
                btMatrix3x3.Multiply(ref rb0.InvInertiaTensorWorld, ref torqueAxis0, out temp);
                solverConstraint.m_angularComponentA = temp * rb0.AngularFactor;
            }
            else
                solverConstraint.m_angularComponentA = btVector3.Zero;
            #endregion
            btVector3 torqueAxis1 = rel_pos2.cross(cp.m_normalWorldOnB);
            #region solverConstraint.m_angularComponentB = rb1 != null ? rb1.InvInertiaTensorWorld * -torqueAxis1 * rb1.AngularFactor : btVector3.Zero;
            if (rb1 != null)
            {
                btVector3 temp,temp2;
                temp2 = -torqueAxis1;
                btMatrix3x3.Multiply(ref rb1.InvInertiaTensorWorld, ref temp2, out temp);
                solverConstraint.m_angularComponentB = temp * rb1.AngularFactor;
            }
            else
                solverConstraint.m_angularComponentB = btVector3.Zero;
            #endregion
            {
#if COMPUTE_IMPULSE_DENOM
				btScalar denom0 = rb0->computeImpulseDenominator(pos1,cp.m_normalWorldOnB);
				btScalar denom1 = rb1->computeImpulseDenominator(pos2,cp.m_normalWorldOnB);
#else
                btVector3 vec;
                float denom0 = 0f;
                float denom1 = 0f;
                if (rb0 != null)
                {
                    vec = (solverConstraint.m_angularComponentA).cross(rel_pos1);
                    denom0 = rb0.InvMass + cp.m_normalWorldOnB.dot(vec);
                }
                if (rb1 != null)
                {
                    vec = (-solverConstraint.m_angularComponentB).cross(rel_pos2);
                    denom1 = rb1.InvMass + cp.m_normalWorldOnB.dot(vec);
                }
#endif //COMPUTE_IMPULSE_DENOM

                float denom = relaxation / (denom0 + denom1);
                solverConstraint.m_jacDiagABInv = denom;
            }

            solverConstraint.m_contactNormal = cp.m_normalWorldOnB;
            solverConstraint.m_relpos1CrossNormal = rel_pos1.cross(cp.m_normalWorldOnB);
            solverConstraint.m_relpos2CrossNormal = rel_pos2.cross(-cp.m_normalWorldOnB);




            btVector3 vel1 = rb0 != null ? rb0.getVelocityInLocalPoint(rel_pos1) : btVector3.Zero;
            btVector3 vel2 = rb1 != null ? rb1.getVelocityInLocalPoint(rel_pos2) : btVector3.Zero;
            vel = vel1 - vel2;
            rel_vel = cp.m_normalWorldOnB.dot(vel);

            float penetration = cp.Distance + infoGlobal.m_linearSlop;


            solverConstraint.m_friction = cp.m_combinedFriction;

            float restitution = 0f;

            if (cp.m_lifeTime > infoGlobal.m_restingContactRestitutionThreshold)
            {
                restitution = 0f;
            }
            else
            {
                restitution = restitutionCurve(rel_vel, cp.m_combinedRestitution);
                if (restitution <= 0f)
                {
                    restitution = 0f;
                };
            }


            ///warm starting (or zero if disabled)
            if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_WARMSTARTING) != 0)
            {
                solverConstraint.m_appliedImpulse = cp.m_appliedImpulse * infoGlobal.m_warmstartingFactor;
                if (rb0 != null)
                    rb0.internalApplyImpulse(solverConstraint.m_contactNormal * rb0.InvMass * rb0.LinearFactor, solverConstraint.m_angularComponentA, solverConstraint.m_appliedImpulse);
                if (rb1 != null)
                    rb1.internalApplyImpulse(solverConstraint.m_contactNormal * rb1.InvMass * rb1.LinearFactor, -solverConstraint.m_angularComponentB, -solverConstraint.m_appliedImpulse);
            }
            else
            {
                solverConstraint.m_appliedImpulse = 0f;
            }

            solverConstraint.m_appliedPushImpulse = 0f;

            {
                float rel_vel2;
                float vel1Dotn = solverConstraint.m_contactNormal.dot(rb0 != null ? rb0.LinearVelocity : btVector3.Zero)
                    + solverConstraint.m_relpos1CrossNormal.dot(rb0 != null ? rb0.AngularVelocity : btVector3.Zero);
                float vel2Dotn = -solverConstraint.m_contactNormal.dot(rb1 != null ? rb1.LinearVelocity : btVector3.Zero)
                    + solverConstraint.m_relpos2CrossNormal.dot(rb1 != null ? rb1.AngularVelocity : btVector3.Zero);

                rel_vel2 = vel1Dotn + vel2Dotn;

                float positionalError = 0f;
                positionalError = -penetration * infoGlobal.m_erp / infoGlobal.m_timeStep;
                float velocityError = restitution - rel_vel2;// * damping;
                float penetrationImpulse = positionalError * solverConstraint.m_jacDiagABInv;
                float velocityImpulse = velocityError * solverConstraint.m_jacDiagABInv;
                if (!infoGlobal.m_splitImpulse || (penetration > infoGlobal.m_splitImpulsePenetrationThreshold))
                {
                    //combine position and velocity into rhs
                    solverConstraint.m_rhs = penetrationImpulse + velocityImpulse;
                    solverConstraint.m_rhsPenetration = 0f;
                }
                else
                {
                    //split position and velocity into rhs and m_rhsPenetration
                    solverConstraint.m_rhs = velocityImpulse;
                    solverConstraint.m_rhsPenetration = penetrationImpulse;
                }
                solverConstraint.m_cfm = 0f;
                solverConstraint.m_lowerLimit = 0;
                solverConstraint.m_upperLimit = 1e10f;
            }

        }
        protected void setFrictionConstraintImpulse(SolverConstraint solverConstraint, RigidBody rb0, RigidBody rb1, ManifoldPoint cp, ContactSolverInfo infoGlobal)
        {
            if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_FRICTION_WARMSTARTING) != 0)
            {
                {
                    SolverConstraint frictionConstraint1 = m_tmpSolverContactFrictionConstraintPool[solverConstraint.m_frictionIndex];
                    if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_WARMSTARTING) != 0)
                    {
                        frictionConstraint1.m_appliedImpulse = cp.m_appliedImpulseLateral1 * infoGlobal.m_warmstartingFactor;
                        if (rb0 != null)
                            rb0.internalApplyImpulse(frictionConstraint1.m_contactNormal * rb0.InvMass * rb0.LinearFactor, frictionConstraint1.m_angularComponentA, frictionConstraint1.m_appliedImpulse);
                        if (rb1 != null)
                            rb1.internalApplyImpulse(frictionConstraint1.m_contactNormal * rb1.InvMass * rb1.LinearFactor, -frictionConstraint1.m_angularComponentB, -frictionConstraint1.m_appliedImpulse);
                    }
                    else
                    {
                        frictionConstraint1.m_appliedImpulse = 0f;
                    }
                }

                if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_2_FRICTION_DIRECTIONS) != 0)
                {
                    SolverConstraint frictionConstraint2 = m_tmpSolverContactFrictionConstraintPool[solverConstraint.m_frictionIndex + 1];
                    if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_WARMSTARTING) != 0)
                    {
                        frictionConstraint2.m_appliedImpulse = cp.m_appliedImpulseLateral2 * infoGlobal.m_warmstartingFactor;
                        if (rb0 != null)
                            rb0.internalApplyImpulse(frictionConstraint2.m_contactNormal * rb0.InvMass, frictionConstraint2.m_angularComponentA, frictionConstraint2.m_appliedImpulse);
                        if (rb1 != null)
                            rb1.internalApplyImpulse(frictionConstraint2.m_contactNormal * rb1.InvMass, -frictionConstraint2.m_angularComponentB, -frictionConstraint2.m_appliedImpulse);
                    }
                    else
                    {
                        frictionConstraint2.m_appliedImpulse = 0f;
                    }
                }
            }
            else
            {
                SolverConstraint frictionConstraint1 = m_tmpSolverContactFrictionConstraintPool[solverConstraint.m_frictionIndex];
                frictionConstraint1.m_appliedImpulse = 0f;
                if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_2_FRICTION_DIRECTIONS) != 0)
                {
                    SolverConstraint frictionConstraint2 = m_tmpSolverContactFrictionConstraintPool[solverConstraint.m_frictionIndex + 1];
                    frictionConstraint2.m_appliedImpulse = 0f;
                }
            }
        }
        protected float restitutionCurve(float rel_vel, float restitution)
        {
            return restitution * -rel_vel;
        }
        protected void convertContact(PersistentManifold manifold, ContactSolverInfo infoGlobal)
        {
            CollisionObject colObj0, colObj1;

            colObj0 = manifold.Body0;
            colObj1 = manifold.Body1;


            RigidBody solverBodyA = colObj0 as RigidBody;
            RigidBody solverBodyB = colObj1 as RigidBody;

            ///avoid collision response between two static objects
            if ((solverBodyA == null || solverBodyA.InvMass == 0) && (solverBodyB == null || solverBodyB.InvMass == 0))
                return;

            for (int j = 0; j < manifold.NumContacts; j++)
            {

                ManifoldPoint cp = manifold.getContactPoint(j);

                if (cp.Distance <= manifold.ContactProcessingThreshold)
                {
                    btVector3 rel_pos1;
                    btVector3 rel_pos2;
                    float relaxation;
                    float rel_vel;
                    btVector3 vel;

                    int frictionIndex = m_tmpSolverContactConstraintPool.Count;
                    SolverConstraint solverConstraint = SolverConstraint.GetFromPool();// m_tmpSolverContactConstraintPool.expandNonInitializing();
                    m_tmpSolverContactConstraintPool.Add(solverConstraint);
                    RigidBody rb0 = solverBodyA;
                    RigidBody rb1 = solverBodyB;
                    //Debug.Assert(rb0.CollisionShape.ShapeType != BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE && rb0.CollisionShape.ShapeType != BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE);
                    solverConstraint.m_solverBodyA = rb0 != null ? rb0 : FixedBody;
                    solverConstraint.m_solverBodyB = rb1 != null ? rb1 : FixedBody;
                    solverConstraint.m_originalContactPoint = cp;
                    
                    setupContactConstraint(solverConstraint, colObj0, colObj1, cp, infoGlobal, out vel, out rel_vel, out relaxation, out  rel_pos1, out rel_pos2);

                    //			const btVector3& pos1 = cp.getPositionWorldOnA();
                    //			const btVector3& pos2 = cp.getPositionWorldOnB();

                    /////setup the friction constraints

                    solverConstraint.m_frictionIndex = m_tmpSolverContactFrictionConstraintPool.Count;

                    if ((infoGlobal.m_solverMode & SolverMode.SOLVER_ENABLE_FRICTION_DIRECTION_CACHING) == 0 || !cp.m_lateralFrictionInitialized)
                    {
                        cp.m_lateralFrictionDir1 = vel - cp.m_normalWorldOnB * rel_vel;
                        float lat_rel_vel = cp.m_lateralFrictionDir1.Length2;
                        if ((infoGlobal.m_solverMode & SolverMode.SOLVER_DISABLE_VELOCITY_DEPENDENT_FRICTION_DIRECTION) == 0 && lat_rel_vel > BulletGlobal.SIMD_EPSILON)
                        {
                            cp.m_lateralFrictionDir1 /= (float)Math.Sqrt(lat_rel_vel);
                            if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_2_FRICTION_DIRECTIONS) != 0)
                            {
                                cp.m_lateralFrictionDir2 = cp.m_lateralFrictionDir1.cross(cp.m_normalWorldOnB);
                                cp.m_lateralFrictionDir2.normalize();//??
                                applyAnisotropicFriction(colObj0, ref cp.m_lateralFrictionDir2);
                                applyAnisotropicFriction(colObj1, ref cp.m_lateralFrictionDir2);
                                addFrictionConstraint(cp.m_lateralFrictionDir2, solverBodyA, solverBodyB, frictionIndex, cp, rel_pos1, rel_pos2, colObj0, colObj1, relaxation, 0f, 0f);
                            }

                            applyAnisotropicFriction(colObj0, ref cp.m_lateralFrictionDir1);
                            applyAnisotropicFriction(colObj1, ref cp.m_lateralFrictionDir1);
                            addFrictionConstraint(cp.m_lateralFrictionDir1, solverBodyA, solverBodyB, frictionIndex, cp, rel_pos1, rel_pos2, colObj0, colObj1, relaxation, 0f, 0f);
                            cp.m_lateralFrictionInitialized = true;
                        }
                        else
                        {
                            //re-calculate friction direction every frame, todo: check if this is really needed
                            btVector3.PlaneSpace1(ref cp.m_normalWorldOnB, out cp.m_lateralFrictionDir1, out cp.m_lateralFrictionDir2);
                            if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_2_FRICTION_DIRECTIONS) != 0)
                            {
                                applyAnisotropicFriction(colObj0, ref cp.m_lateralFrictionDir2);
                                applyAnisotropicFriction(colObj1, ref cp.m_lateralFrictionDir2);
                                addFrictionConstraint(cp.m_lateralFrictionDir2, solverBodyA, solverBodyB, frictionIndex, cp, rel_pos1, rel_pos2, colObj0, colObj1, relaxation, 0f, 0f);
                            }

                            applyAnisotropicFriction(colObj0, ref cp.m_lateralFrictionDir1);
                            applyAnisotropicFriction(colObj1, ref cp.m_lateralFrictionDir1);
                            addFrictionConstraint(cp.m_lateralFrictionDir1, solverBodyA, solverBodyB, frictionIndex, cp, rel_pos1, rel_pos2, colObj0, colObj1, relaxation, 0f, 0f);

                            cp.m_lateralFrictionInitialized = true;
                        }

                    }
                    else
                    {
                        addFrictionConstraint(cp.m_lateralFrictionDir1, solverBodyA, solverBodyB, frictionIndex, cp, rel_pos1, rel_pos2, colObj0, colObj1, relaxation, cp.m_contactMotion1, cp.m_contactCFM1);
                        if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_2_FRICTION_DIRECTIONS) != 0)
                            addFrictionConstraint(cp.m_lateralFrictionDir2, solverBodyA, solverBodyB, frictionIndex, cp, rel_pos1, rel_pos2, colObj0, colObj1, relaxation, cp.m_contactMotion2, cp.m_contactCFM2);
                    }

                    setFrictionConstraintImpulse(solverConstraint, rb0, rb1, cp, infoGlobal);

                }
            }
        }
        protected void resolveSplitPenetrationImpulseCacheFriendly(RigidBody body1, RigidBody body2, SolverConstraint c)
        {
            if (c.m_rhsPenetration != 0f)
            {
                float deltaImpulse = c.m_rhsPenetration - c.m_appliedPushImpulse * c.m_cfm;
                float deltaVel1Dotn = c.m_contactNormal.dot(body1.internalPushVelocity) + c.m_relpos1CrossNormal.dot(body1.internalTurnVelocity);
                float deltaVel2Dotn = -c.m_contactNormal.dot(body2.internalPushVelocity) + c.m_relpos2CrossNormal.dot(body2.internalTurnVelocity);

                deltaImpulse -= deltaVel1Dotn * c.m_jacDiagABInv;
                deltaImpulse -= deltaVel2Dotn * c.m_jacDiagABInv;
                float sum = c.m_appliedPushImpulse + deltaImpulse;
                if (sum < c.m_lowerLimit)
                {
                    deltaImpulse = c.m_lowerLimit - c.m_appliedPushImpulse;
                    c.m_appliedPushImpulse = c.m_lowerLimit;
                }
                else
                {
                    c.m_appliedPushImpulse = sum;
                }
                body1.internalApplyPushImpulse(c.m_contactNormal * body1.internalInvMass, c.m_angularComponentA, deltaImpulse);
                body2.internalApplyPushImpulse(-c.m_contactNormal * body2.internalInvMass, c.m_angularComponentB, deltaImpulse);
            }
        }
#if false//未移植
        //internal method
	    protected int	getOrInitSolverBody(btCollisionObject& body);
#endif
        protected void resolveSingleConstraintRowGeneric(RigidBody body1, RigidBody body2, SolverConstraint c)
        {
            float deltaImpulse = c.m_rhs - c.m_appliedImpulse * c.m_cfm;
            float deltaVel1Dotn = c.m_contactNormal.dot(body1.internalDeltaLinearVelocity) + c.m_relpos1CrossNormal.dot(body1.internalDeltaAngularVelocity);
            float deltaVel2Dotn = -c.m_contactNormal.dot(body2.internalDeltaLinearVelocity) + c.m_relpos2CrossNormal.dot(body2.internalDeltaAngularVelocity);

            //	const btScalar delta_rel_vel	=	deltaVel1Dotn-deltaVel2Dotn;
            deltaImpulse -= deltaVel1Dotn * c.m_jacDiagABInv;
            deltaImpulse -= deltaVel2Dotn * c.m_jacDiagABInv;

            float sum = c.m_appliedImpulse + deltaImpulse;
            if (sum < c.m_lowerLimit)
            {
                deltaImpulse = c.m_lowerLimit - c.m_appliedImpulse;
                c.m_appliedImpulse = c.m_lowerLimit;
            }
            else if (sum > c.m_upperLimit)
            {
                deltaImpulse = c.m_upperLimit - c.m_appliedImpulse;
                c.m_appliedImpulse = c.m_upperLimit;
            }
            else
            {
                c.m_appliedImpulse = sum;
            }
            body1.internalApplyImpulse(c.m_contactNormal * body1.internalInvMass, c.m_angularComponentA, deltaImpulse);
            body2.internalApplyImpulse(-c.m_contactNormal * body2.internalInvMass, c.m_angularComponentB, deltaImpulse);
        }
        // Project Gauss Seidel or the equivalent Sequential Impulse
        //この関数で最終的なインパルスを適用している
        protected void resolveSingleConstraintRowLowerLimit(RigidBody body1, RigidBody body2, SolverConstraint c)
        {
            float deltaImpulse = c.m_rhs - c.m_appliedImpulse * c.m_cfm;
            float deltaVel1Dotn = c.m_contactNormal.dot(body1.internalDeltaLinearVelocity) + c.m_relpos1CrossNormal.dot(body1.internalDeltaAngularVelocity);
            float deltaVel2Dotn = -c.m_contactNormal.dot(body2.internalDeltaLinearVelocity) + c.m_relpos2CrossNormal.dot(body2.internalDeltaAngularVelocity);
            
            deltaImpulse -= deltaVel1Dotn * c.m_jacDiagABInv;
            deltaImpulse -= deltaVel2Dotn * c.m_jacDiagABInv;
            float sum = c.m_appliedImpulse + deltaImpulse;
            if (sum < c.m_lowerLimit)
            {
                deltaImpulse = c.m_lowerLimit - c.m_appliedImpulse;
                c.m_appliedImpulse = c.m_lowerLimit;
            }
            else
            {
                c.m_appliedImpulse = sum;
            }
            body1.internalApplyImpulse(c.m_contactNormal * body1.internalInvMass, c.m_angularComponentA, deltaImpulse);
            body2.internalApplyImpulse(-c.m_contactNormal * body2.internalInvMass, c.m_angularComponentB, deltaImpulse);
        }
        static RigidBody s_fixed = new RigidBody(0, null, null, btVector3.Zero);
        static RigidBodyConstructionInfo constinfo = new RigidBodyConstructionInfo(0, null, null, btVector3.Zero);
        protected static RigidBody FixedBody
        {
            get
            {
                s_fixed.setupRigidBody(constinfo);
                s_fixed.setMassProps(0f, btVector3.Zero);
                return s_fixed;
            }
        }
        protected virtual void solveGroupCacheFriendlySplitImpulseIterations(IList<CollisionObject> bodies, IList<PersistentManifold> manifoldPtr, IList<TypedConstraint> constraints, ContactSolverInfo infoGlobal, IDebugDraw debugDrawer)
        {
            int iteration;
            if (infoGlobal.m_splitImpulse)
            {
                if ((infoGlobal.m_solverMode & SolverMode.SOLVER_SIMD) != 0)
                {
                    for (iteration = 0; iteration < infoGlobal.m_numIterations; iteration++)
                    {
                        {
                            int numPoolConstraints = m_tmpSolverContactConstraintPool.Count;
                            int j;
                            for (j = 0; j < numPoolConstraints; j++)
                            {
                                SolverConstraint solveManifold = m_tmpSolverContactConstraintPool[m_orderTmpConstraintPool[j]];

                                resolveSplitPenetrationImpulseCacheFriendly(solveManifold.m_solverBodyA, solveManifold.m_solverBodyB, solveManifold);
                            }
                        }
                    }
                }
                else
                {
                    for (iteration = 0; iteration < infoGlobal.m_numIterations; iteration++)
                    {
                        {
                            int numPoolConstraints = m_tmpSolverContactConstraintPool.Count;
                            int j;
                            for (j = 0; j < numPoolConstraints; j++)
                            {
                                SolverConstraint solveManifold = m_tmpSolverContactConstraintPool[m_orderTmpConstraintPool[j]];

                                resolveSplitPenetrationImpulseCacheFriendly(solveManifold.m_solverBodyA, solveManifold.m_solverBodyB, solveManifold);
                            }
                        }
                    }
                }
            }
        }
        protected virtual float solveGroupCacheFriendlyFinish(IList<CollisionObject> bodies, IList<PersistentManifold> manifoldPtr, IList<TypedConstraint> constraints, ContactSolverInfo infoGlobal, IDebugDraw debugDrawer)
        {
            
            for (int i = 0; i < m_tmpSolverContactConstraintPool.Count; i++)
            {
                SolverConstraint solveManifold = m_tmpSolverContactConstraintPool[i];
                ManifoldPoint pt = (ManifoldPoint)solveManifold.m_originalContactPoint;
                Debug.Assert(pt != null);
                pt.m_appliedImpulse = solveManifold.m_appliedImpulse;
                if ((infoGlobal.m_solverMode & SolverMode.SOLVER_USE_FRICTION_WARMSTARTING) != 0)
                {
                    pt.m_appliedImpulseLateral1 = m_tmpSolverContactFrictionConstraintPool[solveManifold.m_frictionIndex].m_appliedImpulse;
                    pt.m_appliedImpulseLateral2 = m_tmpSolverContactFrictionConstraintPool[solveManifold.m_frictionIndex + 1].m_appliedImpulse;
                }

                //do a callback here?
            }

            for (int i = 0; i < m_tmpSolverNonContactConstraintPool.Count; i++)
            {
                SolverConstraint solverConstr = m_tmpSolverNonContactConstraintPool[i];
                TypedConstraint constr = (TypedConstraint)solverConstr.m_originalContactPoint;
                float sum = constr.internalAppliedImpulse;
                sum += solverConstr.m_appliedImpulse;
                constr.internalAppliedImpulse = sum;
            }


            if (infoGlobal.m_splitImpulse)
            {
                for(int i=0;i<bodies.Count;i++)
                {
                    var obj = bodies[i];
                    RigidBody body = obj as RigidBody;
                    if (body != null)
                        body.internalWritebackVelocity(infoGlobal.m_timeStep);
                }
            }
            else
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    var obj = bodies[i];
                    RigidBody body = obj as RigidBody;
                    if (body != null)
                        body.internalWritebackVelocity();
                }
            }


            m_tmpSolverContactConstraintPool.resize(0);
            m_tmpSolverNonContactConstraintPool.resize(0);
            m_tmpSolverContactFrictionConstraintPool.resize(0);

            return 0f;
        }
        protected float solveSingleIteration(int iteration, IList<CollisionObject> bodies, IList<PersistentManifold> manifoldPtr, IList<TypedConstraint> constraints, ContactSolverInfo infoGlobal, IDebugDraw debugDrawer)
        {
            int numConstraintPool = m_tmpSolverContactConstraintPool.Count;
            int numFrictionPool = m_tmpSolverContactFrictionConstraintPool.Count;

            int j;

            if ((infoGlobal.m_solverMode & SolverMode.SOLVER_RANDMIZE_ORDER) != 0)
            {
                if ((iteration & 7) == 0)
                {
                    for (j = 0; j < numConstraintPool; ++j)
                    {
                        int tmp = m_orderTmpConstraintPool[j];
                        int swapi = BulletGlobal.Rand.Next(j + 1);
                        m_orderTmpConstraintPool[j] = m_orderTmpConstraintPool[swapi];
                        m_orderTmpConstraintPool[swapi] = tmp;
                    }

                    for (j = 0; j < numFrictionPool; ++j)
                    {
                        int tmp = m_orderFrictionConstraintPool[j];
                        int swapi = BulletGlobal.Rand.Next(j + 1);
                        m_orderFrictionConstraintPool[j] = m_orderFrictionConstraintPool[swapi];
                        m_orderFrictionConstraintPool[swapi] = tmp;
                    }
                }
            }

            if ((infoGlobal.m_solverMode & SolverMode.SOLVER_SIMD) != 0)
            {
                ///solve all joint constraints, using SIMD, if available
                for (int i = 0; i < m_tmpSolverNonContactConstraintPool.Count;i++ )
                {
                    SolverConstraint constraint = m_tmpSolverNonContactConstraintPool[i];
                    resolveSingleConstraintRowGeneric(constraint.m_solverBodyA, constraint.m_solverBodyB, constraint);
                }

                for (int i = 0; i < constraints.Count; i++)
                {
                    TypedConstraint constraint = constraints[i];
                    constraint.solveConstraintObsolete(constraint.RigidBodyA, constraint.RigidBodyB, infoGlobal.m_timeStep);
                }

                ///solve all contact constraints using SIMD, if available
                int numPoolConstraints = m_tmpSolverContactConstraintPool.Count;
                for (j = 0; j < numPoolConstraints; j++)
                {
                    SolverConstraint solveManifold = m_tmpSolverContactConstraintPool[m_orderTmpConstraintPool[j]];
                    resolveSingleConstraintRowLowerLimit(solveManifold.m_solverBodyA, solveManifold.m_solverBodyB, solveManifold);

                }
                ///solve all friction constraints, using SIMD, if available
                int numFrictionPoolConstraints = m_tmpSolverContactFrictionConstraintPool.Count;
                for (j = 0; j < numFrictionPoolConstraints; j++)
                {
                    SolverConstraint solveManifold = m_tmpSolverContactFrictionConstraintPool[m_orderFrictionConstraintPool[j]];
                    float totalImpulse = m_tmpSolverContactConstraintPool[solveManifold.m_frictionIndex].m_appliedImpulse;

                    if (totalImpulse > 0f)
                    {
                        solveManifold.m_lowerLimit = -(solveManifold.m_friction * totalImpulse);
                        solveManifold.m_upperLimit = solveManifold.m_friction * totalImpulse;

                        resolveSingleConstraintRowGeneric(solveManifold.m_solverBodyA, solveManifold.m_solverBodyB, solveManifold);
                    }
                }
            }
            else
            {

                ///solve all joint constraints
                for (int i = 0; i < m_tmpSolverNonContactConstraintPool.Count;i++ )
                {
                    SolverConstraint constraint = m_tmpSolverNonContactConstraintPool[i];
                    resolveSingleConstraintRowGeneric(constraint.m_solverBodyA, constraint.m_solverBodyB, constraint);
                }

                for (int i = 0; i < constraints.Count; i++)
                {
                    var constraint = constraints[i];
                    constraint.solveConstraintObsolete(constraint.RigidBodyA, constraint.RigidBodyB, infoGlobal.m_timeStep);
                }
                ///solve all contact constraints
                int numPoolConstraints = m_tmpSolverContactConstraintPool.Count;
                for (j = 0; j < numPoolConstraints; j++)
                {
                    SolverConstraint solveManifold = m_tmpSolverContactConstraintPool[m_orderTmpConstraintPool[j]];
                    resolveSingleConstraintRowLowerLimit(solveManifold.m_solverBodyA, solveManifold.m_solverBodyB, solveManifold);
                }
                ///solve all friction constraints
                int numFrictionPoolConstraints = m_tmpSolverContactFrictionConstraintPool.Count;
                for (j = 0; j < numFrictionPoolConstraints; j++)
                {
                    SolverConstraint solveManifold = m_tmpSolverContactFrictionConstraintPool[m_orderFrictionConstraintPool[j]];
                    float totalImpulse = m_tmpSolverContactConstraintPool[solveManifold.m_frictionIndex].m_appliedImpulse;

                    if (totalImpulse > 0f)
                    {
                        solveManifold.m_lowerLimit = -(solveManifold.m_friction * totalImpulse);
                        solveManifold.m_upperLimit = solveManifold.m_friction * totalImpulse;

                        resolveSingleConstraintRowGeneric(solveManifold.m_solverBodyA, solveManifold.m_solverBodyB, solveManifold);
                    }
                }
            }
            return 0f;
        }
        protected virtual float solveGroupCacheFriendlySetup(IList<CollisionObject> bodies, IList<PersistentManifold> manifoldPtr, IList<TypedConstraint> constraints, ContactSolverInfo infoGlobal, IDebugDraw debugDrawer)
        {
            BulletGlobal.StartProfile("0-3-?-1-1 solveGroupCacheFriendlySetup");
            try
            {
                if ((constraints.Count + manifoldPtr.Count) == 0)
                {
                    //		printf("empty\n");
                    return 0f;
                }

                //if (1)
                {
                    for(int i=0;i<constraints.Count;i++)
                    {
                        TypedConstraint constraint = constraints[i];
                        constraint.buildJacobian();
                    }
                }
                //btRigidBody* rb0=0,*rb1=0;

                //if (1)
                {
                    {

                        int totalNumRows = 0;
                        int i;

                        m_tmpConstraintSizesPool.resize(constraints.Count);
                        //calculate the total number of contraint rows
                        for (i = 0; i < constraints.Count; i++)
                        {
                            TypedConstraint.ConstraintInfo1 info1 = m_tmpConstraintSizesPool[i];
                            constraints[i].getInfo1(info1);
                            totalNumRows += info1.m_numConstraintRows;
                        }
                        m_tmpSolverNonContactConstraintPool.resize(totalNumRows);


                        ///setup the btSolverConstraints
                        int currentRow = 0;

                        for (i = 0; i < constraints.Count; i++)
                        {
                            TypedConstraint.ConstraintInfo1 info1 = m_tmpConstraintSizesPool[i];

                            if (info1.m_numConstraintRows != 0)
                            {
                                Debug.Assert(currentRow < totalNumRows);

                                SolverConstraint currentConstraintRow = m_tmpSolverNonContactConstraintPool[currentRow];
                                TypedConstraint constraint = constraints[i];



                                RigidBody rbA = constraint.RigidBodyA;
                                RigidBody rbB = constraint.RigidBodyB;


                                int j;
                                for (j = 0; j < info1.m_numConstraintRows; j++)
                                {
                                    //memset(&currentConstraintRow[j],0,sizeof(btSolverConstraint));
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].SetZero();
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].m_lowerLimit = float.MinValue;
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].m_upperLimit = float.MaxValue;
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].m_appliedImpulse = 0f;
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].m_appliedPushImpulse = 0f;
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].m_solverBodyA = rbA;
                                    m_tmpSolverNonContactConstraintPool[currentRow + j].m_solverBodyB = rbB;
                                }

                                rbA.internalDeltaLinearVelocity = new btVector3(0f, 0f, 0f);
                                rbA.internalDeltaAngularVelocity = new btVector3(0f, 0f, 0f);
                                rbB.internalDeltaLinearVelocity = new btVector3(0f, 0f, 0f);
                                rbB.internalDeltaAngularVelocity = new btVector3(0f, 0f, 0f);

                                //ポインタで受け渡しすると配列の次のオブジェクトに移るときに問題が発生するので、リストを受け渡しすることにする
#if false
                                fixed(btVector3* contactNormal=&currentConstraintRow.m_contactNormal,
                                relpos1=&currentConstraintRow.m_relpos1CrossNormal,
                                relpos2=&currentConstraintRow.m_relpos2CrossNormal)
                                    {
#endif
                                TypedConstraint.ConstraintInfo2 info2 = TypedConstraint.ConstraintInfo2.GetFromPool();
                                info2.fps = 1f / infoGlobal.m_timeStep;
                                info2.erp = infoGlobal.m_erp;
                                info2.Constraints = m_tmpSolverNonContactConstraintPool;
                                info2.CurrentRow = currentRow;
#if false
                                info2.m_J1linearAxis = (float*)contactNormal;//currentConstraintRow->m_contactNormal;
					            info2.m_J1angularAxis = (float*)relpos1;//currentConstraintRow.m_relpos1CrossNormal;
					            info2.m_J2linearAxis = null;
					            info2.m_J2angularAxis = (float*)relpos2;//currentConstraintRow.m_relpos2CrossNormal;
					            //info2.rowskip = sizeof(SolverConstraint)/sizeof(float);//check this
					            ///the size of btSolverConstraint needs be a multiple of float
					            //Debug.Assert(info2.rowskip*sizeof(float)== sizeof(btSolverConstraint));
					            info2.m_constraintError = &currentConstraintRow.m_rhs;
					            currentConstraintRow.m_cfm = infoGlobal.m_globalCfm;
					            info2.cfm = &currentConstraintRow.m_cfm;
					            info2.m_lowerLimit = &currentConstraintRow.m_lowerLimit;
					            info2.m_upperLimit = &currentConstraintRow.m_upperLimit;
#endif
                                info2.m_numIterations = infoGlobal.m_numIterations;
                                constraints[i].getInfo2(info2);
                                info2.Free();
#if false
                            }
#endif
                                ///finalize the constraint setup
                                for (j = 0; j < info1.m_numConstraintRows; j++)
                                {
                                    SolverConstraint solverConstraint = m_tmpSolverNonContactConstraintPool[currentRow + j];
                                    solverConstraint.m_originalContactPoint = constraint;

                                    {
                                        //const btVector3& ftorqueAxis1 = solverConstraint.m_relpos1CrossNormal;
                                        #region solverConstraint.m_angularComponentA = constraint.RigidBodyA.InvInertiaTensorWorld * solverConstraint.m_relpos1CrossNormal * constraint.RigidBodyA.AngularFactor;
                                        btVector3 temp;
                                        btMatrix3x3.Multiply(ref constraint.RigidBodyA.InvInertiaTensorWorld, ref solverConstraint.m_relpos1CrossNormal, out temp);
                                        solverConstraint.m_angularComponentA = temp * constraint.RigidBodyA.AngularFactor;
                                        #endregion
                                    }
                                    {
                                        //const btVector3& ftorqueAxis2 = solverConstraint.m_relpos2CrossNormal;
                                        #region solverConstraint.m_angularComponentB = constraint.RigidBodyB.InvInertiaTensorWorld * solverConstraint.m_relpos2CrossNormal * constraint.RigidBodyB.AngularFactor;
                                        btVector3 temp;
                                        btMatrix3x3.Multiply(ref constraint.RigidBodyB.InvInertiaTensorWorld, ref solverConstraint.m_relpos2CrossNormal, out temp);
                                        solverConstraint.m_angularComponentB = temp * constraint.RigidBodyB.AngularFactor;
                                        #endregion
                                    }

                                    {
                                        btVector3 iMJlA = solverConstraint.m_contactNormal * rbA.InvMass;
                                        btVector3 iMJaA;// = rbA.InvInertiaTensorWorld * solverConstraint.m_relpos1CrossNormal;
                                        btMatrix3x3.Multiply(ref rbA.InvInertiaTensorWorld, ref solverConstraint.m_relpos1CrossNormal, out iMJaA);
                                        btVector3 iMJlB = solverConstraint.m_contactNormal * rbB.InvMass;//sign of normal?
                                        btVector3 iMJaB;// = rbB.InvInertiaTensorWorld * solverConstraint.m_relpos2CrossNormal;
                                        btMatrix3x3.Multiply(ref rbB.InvInertiaTensorWorld, ref solverConstraint.m_relpos2CrossNormal, out iMJaB);

                                        float sum = iMJlA.dot(solverConstraint.m_contactNormal);
                                        sum += iMJaA.dot(solverConstraint.m_relpos1CrossNormal);
                                        sum += iMJlB.dot(solverConstraint.m_contactNormal);
                                        sum += iMJaB.dot(solverConstraint.m_relpos2CrossNormal);

                                        solverConstraint.m_jacDiagABInv = 1f / sum;
                                    }


                                    ///fix rhs
                                    ///todo: add force/torque accelerators
                                    {
                                        float rel_vel;
                                        float vel1Dotn = solverConstraint.m_contactNormal.dot(rbA.LinearVelocity) + solverConstraint.m_relpos1CrossNormal.dot(rbA.AngularVelocity);
                                        float vel2Dotn = -solverConstraint.m_contactNormal.dot(rbB.LinearVelocity) + solverConstraint.m_relpos2CrossNormal.dot(rbB.AngularVelocity);

                                        rel_vel = vel1Dotn + vel2Dotn;

                                        float restitution = 0f;
                                        float positionalError = solverConstraint.m_rhs;//already filled in by getConstraintInfo2
                                        float velocityError = restitution - rel_vel;// * damping;
                                        float penetrationImpulse = positionalError * solverConstraint.m_jacDiagABInv;
                                        float velocityImpulse = velocityError * solverConstraint.m_jacDiagABInv;
                                        solverConstraint.m_rhs = penetrationImpulse + velocityImpulse;
                                        solverConstraint.m_appliedImpulse = 0f;

                                    }
                                }
                            }
                            currentRow += m_tmpConstraintSizesPool[i].m_numConstraintRows;
                        }
                    }

                    {
                        //int i;
                        //PersistentManifold* manifold = 0;
                        //			btCollisionObject* colObj0=0,*colObj1=0;


                        for (int i = 0; i < manifoldPtr.Count; i++) 
                        {
                            PersistentManifold manifold = manifoldPtr[i];
                            convertContact(manifold, infoGlobal);
                        }
                    }
                }
                
                ContactSolverInfo info = infoGlobal;



                int numConstraintPool = m_tmpSolverContactConstraintPool.Count;
                int numFrictionPool = m_tmpSolverContactFrictionConstraintPool.Count;

                ///@todo: use stack allocator for such temporarily memory, same for solver bodies/constraints
                //m_orderTmpConstraintPool.resize(numConstraintPool);
                //m_orderFrictionConstraintPool.resize(numFrictionPool);
                m_orderTmpConstraintPool.Clear();
                m_orderFrictionConstraintPool.Clear();
                {
                    int i;
                    for (i = 0; i < numConstraintPool; i++)
                    {
                        m_orderTmpConstraintPool.Add(i);
                        //m_orderTmpConstraintPool[i] = i;
                    }
                    for (i = 0; i < numFrictionPool; i++)
                    {
                        m_orderFrictionConstraintPool.Add(i);
                        //m_orderFrictionConstraintPool[i] = i;
                    }
                }

                return 0f;
            }
            finally
            {
                BulletGlobal.EndProfile("0-3-?-1-1 solveGroupCacheFriendlySetup");
            }
        }

        protected virtual float solveGroupCacheFriendlyIterations(IList<CollisionObject> bodies, IList<PersistentManifold> manifoldPtr, IList<TypedConstraint> constraints, ContactSolverInfo infoGlobal, IDebugDraw debugDrawer)
        {
            BulletGlobal.StartProfile("0-3-?-1-2 solveGroupCacheFriendlyIterations");
            int iteration;
            {
                for (iteration = 0; iteration < infoGlobal.m_numIterations; iteration++)
                {
                    solveSingleIteration(iteration, bodies, manifoldPtr, constraints, infoGlobal, debugDrawer);
                }

                solveGroupCacheFriendlySplitImpulseIterations(bodies, manifoldPtr, constraints, infoGlobal, debugDrawer);
            }
            BulletGlobal.EndProfile("0-3-?-1-2 solveGroupCacheFriendlyIterations");
            return 0f;
        }
        public SequentialImpulseConstraintSolver()
        {
            //m_btSeed2 = 0;
        }
        public override float solveGroup(IList<CollisionObject> bodies, IList<PersistentManifold> manifoldPtr, IList<TypedConstraint> constraints, ContactSolverInfo infoGlobal, IDebugDraw debugDrawer, IDispatcher dispatcher)
        {
            BulletGlobal.StartProfile("0-3-?-1 solveGroup");
	
            Debug.Assert(bodies != null);
            Debug.Assert(bodies.Count != 0);

            solveGroupCacheFriendlySetup(bodies, manifoldPtr, constraints, infoGlobal, debugDrawer);

            solveGroupCacheFriendlyIterations(bodies, manifoldPtr, constraints, infoGlobal, debugDrawer);

            solveGroupCacheFriendlyFinish(bodies, manifoldPtr, constraints, infoGlobal, debugDrawer);

            BulletGlobal.EndProfile("0-3-?-1 solveGroup");
            return 0f;
        }

        ///clear internal cached data and reset random seed
        public override void reset() { }
	
#if false
        unsigned long btRand2();

	    int btRandInt2 (int n);

	    void	setRandSeed(unsigned long seed)
	    {
		    m_btSeed2 = seed;
	    }
	    unsigned long	getRandSeed() const
	    {
		    return m_btSeed2;
	    }

#endif
        static void applyAnisotropicFriction(CollisionObject colObj, ref btVector3 frictionDirection)
        {
            if (colObj != null && colObj.hasAnisotropicFriction)
            {
                // transform to local coordinates
                btVector3 loc_lateral;// = frictionDirection * colObj.WorldTransform.Basis;
                btMatrix3x3.Multiply(ref frictionDirection, ref colObj.WorldTransform.Basis, out loc_lateral);

                btVector3 friction_scaling = colObj.AnisotropicFriction;
                //apply anisotropic friction
                loc_lateral *= friction_scaling;
                // ... and transform it back to global coordinates
                //frictionDirection = colObj.WorldTransform.Basis * loc_lateral;
                btMatrix3x3.Multiply(ref colObj.WorldTransform.Basis, ref loc_lateral, out frictionDirection);
            }
        }
    }
}
