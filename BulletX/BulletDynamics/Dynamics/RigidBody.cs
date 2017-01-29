using System;
using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletDynamics.ConstraintSolver;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.Dynamics
{
    public class RigidBody : CollisionObject
    {
        //globalの代わり...
        public static bool gDisableDeactivation = false;
        static float gDeactivationTime = 2f;
        //statics
        static int uniqueId = 0;

        public btMatrix3x3 InvInertiaTensorWorld;
        btVector3 m_linearVelocity;
        btVector3 m_angularVelocity;
        float m_inverseMass;
        btVector3 m_linearFactor;

        btVector3 m_gravity;
        btVector3 m_gravity_acceleration;
        public btVector3 InvInertiaDiagLocal;
        btVector3 m_totalForce;
        btVector3 m_totalTorque;

        float m_linearDamping;
        float m_angularDamping;

        bool m_additionalDamping;
        float m_additionalDampingFactor;
        float m_additionalLinearDampingThresholdSqr;
        float m_additionalAngularDampingThresholdSqr;
        float m_additionalAngularDampingFactor;

        float m_linearSleepingThreshold;
        float m_angularSleepingThreshold;

        //m_optionalMotionState allows to automatic synchronize the world transform for active objects
        IMotionState m_optionalMotionState;

        //keep track of typed constraints referencing this rigid body
        List<TypedConstraint> m_constraintRefs = new List<TypedConstraint>();

        RigidBodyFlags m_rigidbodyFlags;

        int m_debugBodyId;

        protected btVector3 m_deltaLinearVelocity;
        protected btVector3 m_deltaAngularVelocity;
        protected btVector3 m_angularFactor;
        protected btVector3 m_invMass;
        protected btVector3 m_pushVelocity;
        protected btVector3 m_turnVelocity;

        protected override CollisionObjectTypes InternalType
        {
            get { return CollisionObjectTypes.CO_RIGID_BODY; }
        }
        //constructors
        public RigidBody(RigidBodyConstructionInfo constructionInfo)
        {
            setupRigidBody(constructionInfo);
        }
        public RigidBody(float mass, IMotionState motionState, CollisionShape collisionShape, btVector3 localInertia)
        {
            setupRigidBody(new RigidBodyConstructionInfo(mass, motionState, collisionShape, localInertia));
        }
        internal void setupRigidBody(RigidBodyConstructionInfo constructionInfo)
        {
            m_linearVelocity.setValue(0.0f, 0.0f, 0.0f);
            m_angularVelocity.setValue(0f, 0f, 0f);
            m_angularFactor.setValue(1, 1, 1);
            m_linearFactor.setValue(1, 1, 1);
            m_gravity.setValue(0.0f, 0.0f, 0.0f);
            m_gravity_acceleration.setValue(0.0f, 0.0f, 0.0f);
            m_totalForce.setValue(0.0f, 0.0f, 0.0f);
            m_totalTorque.setValue(0.0f, 0.0f, 0.0f);
            m_linearDamping = 0f;
            m_angularDamping = 0.5f;
            m_linearSleepingThreshold = constructionInfo.m_linearSleepingThreshold;
            m_angularSleepingThreshold = constructionInfo.m_angularSleepingThreshold;
            m_optionalMotionState = constructionInfo.m_motionState;
            m_contactSolverType = 0;
            m_frictionSolverType = 0;
            m_additionalDamping = constructionInfo.m_additionalDamping;
            m_additionalDampingFactor = constructionInfo.m_additionalDampingFactor;
            m_additionalLinearDampingThresholdSqr = constructionInfo.m_additionalLinearDampingThresholdSqr;
            m_additionalAngularDampingThresholdSqr = constructionInfo.m_additionalAngularDampingThresholdSqr;
            m_additionalAngularDampingFactor = constructionInfo.m_additionalAngularDampingFactor;

            if (m_optionalMotionState != null)
            {
                m_optionalMotionState.getWorldTransform(out WorldTransform);
            }
            else
            {
                WorldTransform = constructionInfo.m_startWorldTransform;
            }

            m_interpolationWorldTransform = WorldTransform;
            m_interpolationLinearVelocity.setValue(0, 0, 0);
            m_interpolationAngularVelocity.setValue(0, 0, 0);

            //moved to btCollisionObject
            m_friction = constructionInfo.m_friction;
            m_restitution = constructionInfo.m_restitution;

            this.CollisionShape = constructionInfo.m_collisionShape;
            m_debugBodyId = uniqueId++;

            setMassProps(constructionInfo.m_mass, constructionInfo.m_localInertia);
            setDamping(constructionInfo.m_linearDamping, constructionInfo.m_angularDamping);
            updateInertiaTensor();

            RigidBodyFlag = RigidBodyFlags.BT_NONE;


            m_deltaLinearVelocity.setZero();
            m_deltaAngularVelocity.setZero();
            m_invMass = m_linearFactor * m_inverseMass;
            m_pushVelocity.setZero();
            m_turnVelocity.setZero();

        }

        public void proceedToTransform(btTransform newTrans)
        {
            CenterOfMassTransform = newTrans;
        }

        //upcastは要らない……

        /// continuous collision detection needs prediction
        public void predictIntegratedTransform(float timeStep, out btTransform predictedTransform)
        {
            TransformUtil.integrateTransform(WorldTransform, m_linearVelocity, m_angularVelocity, timeStep, out predictedTransform);
        }
        public void saveKinematicState(float timeStep)
        {
            //todo: clamp to some (user definable) safe minimum timestep, to limit maximum angular/linear velocities
            if (timeStep != 0f)
            {
                //if we use motionstate to synchronize world transforms, get the new kinematic/animated world transform
                if (MotionState != null)
                    MotionState.getWorldTransform(out WorldTransform);
                //btVector3 linVel, angVel;

                TransformUtil.calculateVelocity(m_interpolationWorldTransform, WorldTransform, timeStep, out m_linearVelocity, out m_angularVelocity);
                m_interpolationLinearVelocity = m_linearVelocity;
                m_interpolationAngularVelocity = m_angularVelocity;
                m_interpolationWorldTransform = WorldTransform;
                //printf("angular = %f %f %f\n",m_angularVelocity.getX(),m_angularVelocity.getY(),m_angularVelocity.getZ());
            }
        }

        public void applyGravity()
        {
            if (isStaticOrKinematicObject)
                return;

            applyCentralForce(m_gravity);

        }

        public btVector3 Gravity
        {
            get { return m_gravity_acceleration; }
            set
            {
                if (m_inverseMass != 0.0f)
                {
                    m_gravity = value * (1.0f / m_inverseMass);
                }
                m_gravity_acceleration = value;
            }
        }
        public void setDamping(float lin_damping, float ang_damping)
        {
            m_linearDamping = BulletGlobal.GEN_clamped(lin_damping, 0.0f, 1.0f);
            m_angularDamping = BulletGlobal.GEN_clamped(ang_damping, 0.0f, 1.0f);
        }

        public float LinearDamping { get { return m_linearDamping; } }
        public float AngularDamping { get { return m_angularDamping; } }
        public float LinearSleepingThreshold { get { return m_linearSleepingThreshold; } }
        public float AngularSleepingThreshold { get { return m_angularSleepingThreshold; } }
        public void applyDamping(float timeStep)
        {
            //On new damping: see discussion/issue report here: http://code.google.com/p/bullet/issues/detail?id=74
            //todo: do some performance comparisons (but other parts of the engine are probably bottleneck anyway
            m_linearVelocity *= (float)Math.Pow(1f - m_linearDamping, timeStep);
            m_angularVelocity *= (float)Math.Pow(1f - m_angularDamping, timeStep);

            if (m_additionalDamping)
            {
                //Additional damping can help avoiding lowpass jitter motion, help stability for ragdolls etc.
                //Such damping is undesirable, so once the overall simulation quality of the rigid body dynamics system has improved, this should become obsolete
                if ((m_angularVelocity.Length2 < m_additionalAngularDampingThresholdSqr) &&
                    (m_linearVelocity.Length2 < m_additionalLinearDampingThresholdSqr))
                {
                    m_angularVelocity *= m_additionalDampingFactor;
                    m_linearVelocity *= m_additionalDampingFactor;
                }


                float speed = m_linearVelocity.Length;
                if (speed < m_linearDamping)
                {
                    float dampVel = 0.005f;
                    if (speed > dampVel)
                    {
                        btVector3 dir;// = m_linearVelocity.normalized();
                        m_linearVelocity.normalized(out dir);
                        m_linearVelocity -= dir * dampVel;
                    }
                    else
                    {
                        m_linearVelocity.setValue(0f, 0f, 0f);
                    }
                }

                float angSpeed = m_angularVelocity.Length;
                if (angSpeed < m_angularDamping)
                {
                    float angDampVel = 0.005f;
                    if (angSpeed > angDampVel)
                    {
                        btVector3 dir;// = m_angularVelocity.normalized();
                        m_angularVelocity.normalized(out dir);
                        m_angularVelocity -= dir * angDampVel;
                    }
                    else
                    {
                        m_angularVelocity.setValue(0f, 0f, 0f);
                    }
                }
            }
        }
        public override CollisionShape CollisionShape { get { return m_collisionShape; } }

        public void setMassProps(float mass, btVector3 inertia)
        {
            if (mass == 0f)
            {
                CollisionFlags |= CollisionFlags.CF_STATIC_OBJECT;
                m_inverseMass = 0f;
            }
            else
            {
                CollisionFlags &= (~CollisionFlags.CF_STATIC_OBJECT);
                m_inverseMass = 1.0f / mass;
            }

            InvInertiaDiagLocal.setValue(inertia.X != 0.0f ? 1.0f / inertia.X : 0.0f,
                           inertia.Y != 0.0f ? 1.0f / inertia.Y : 0.0f,
                           inertia.Z != 0.0f ? 1.0f / inertia.Z : 0.0f);

            m_invMass = m_linearFactor * m_inverseMass;
        }
        public btVector3 LinearFactor
        {
            get { return m_linearFactor; }
            set
            {
                m_linearFactor = value;
                m_invMass = m_linearFactor * m_inverseMass;
            }
        }
        public float InvMass { get { return m_inverseMass; } }
        //public btMatrix3x3 InvInertiaTensorWorld { get { return m_invInertiaTensorWorld; } }

        public void integrateVelocities(float step)
        {
            if (isStaticOrKinematicObject)
                return;

            #region m_linearVelocity += m_totalForce * (m_inverseMass * step);
            {
                btVector3 temp;
                btVector3.Multiply(ref m_totalForce, m_inverseMass * step, out temp);
                m_linearVelocity.Add(ref temp);
            }
            #endregion
            #region m_angularVelocity += m_invInertiaTensorWorld * m_totalTorque * step;
            {
                btVector3 temp,temp2;
                btMatrix3x3.Multiply(ref InvInertiaTensorWorld, ref m_totalTorque, out temp);
                btVector3.Multiply(ref temp, step, out temp2);
                //m_angularVelocity += temp * step;
                m_angularVelocity.Add(ref temp2);
            }
            #endregion
            /// clamp angular velocity. collision calculations will fail on higher angular velocities
            const float MAX_ANGVEL = BulletGlobal.SIMD_HALF_PI;
            float angvel = m_angularVelocity.Length;
            if (angvel * step > MAX_ANGVEL)
            {
                m_angularVelocity *= (MAX_ANGVEL / step) / angvel;
            }
        }
        public btTransform CenterOfMassTransform
        {
            get { return WorldTransform; }
            set
            {
                if (isStaticOrKinematicObject)
                {
                    m_interpolationWorldTransform = WorldTransform;
                }
                else
                {
                    m_interpolationWorldTransform = value;
                }
                m_interpolationLinearVelocity = LinearVelocity;
                m_interpolationAngularVelocity = AngularVelocity;
                WorldTransform = value;
                updateInertiaTensor();
            }
        }

        public void applyCentralForce(btVector3 force)
        {
            m_totalForce += force * m_linearFactor;
        }

        public btVector3 TotalForce { get { return m_totalForce; } }
        public btVector3 TotalTorque { get { return m_totalTorque; } }
        //public btVector3 InvInertiaDiagLocal { get { return m_invInertiaLocal; } set { m_invInertiaLocal = value; } }
        public void setSleepingThresholds(float linear, float angular)
        {
            m_linearSleepingThreshold = linear;
            m_angularSleepingThreshold = angular;
        }
        public void applyTorque(btVector3 torque)
        {
            m_totalTorque += torque * m_angularFactor;
        }

        public void applyForce(btVector3 force, btVector3 rel_pos)
        {
            applyCentralForce(force);
            applyTorque(rel_pos.cross(force * m_linearFactor));
        }

        public void applyCentralImpulse(btVector3 impulse)
        {
            m_linearVelocity += impulse * m_linearFactor * m_inverseMass;
        }

        public void applyTorqueImpulse(btVector3 torque)
        {
#region m_angularVelocity += m_invInertiaTensorWorld * torque * m_angularFactor;
            {
                btVector3 temp;
                btMatrix3x3.Multiply(ref InvInertiaTensorWorld, ref torque, out temp);
                m_angularVelocity += temp * m_angularFactor;
            }
#endregion
        }

        public void applyImpulse(btVector3 impulse, btVector3 rel_pos)
        {
            if (m_inverseMass != 0f)
            {
                applyCentralImpulse(impulse);
                if (m_angularFactor.X != 0 && m_angularFactor.Y != 0 && m_angularFactor.Z != 0)
                {
                    applyTorqueImpulse(rel_pos.cross(impulse * m_linearFactor));
                }
            }
        }
        public void clearForces()
        {
            m_totalForce.setValue(0.0f, 0.0f, 0.0f);
            m_totalTorque.setValue(0.0f, 0.0f, 0.0f);
        }
        public void updateInertiaTensor()
        {
            #region InvInertiaTensorWorld = WorldTransform.Basis.scaled(m_invInertiaLocal) * WorldTransform.Basis.transpose();
            {
                btMatrix3x3 temp1, temp2;
                WorldTransform.Basis.scaled(ref InvInertiaDiagLocal, out temp1);
                WorldTransform.Basis.transpose(out temp2);
                btMatrix3x3.Multiply(ref temp1, ref temp2, out InvInertiaTensorWorld);
            }
            #endregion
        }
        public btVector3 CenterOfMassPosition { get { return WorldTransform.Origin; } }
        public btQuaternion Orientation
        {
            get
            {
                btQuaternion orn;
                WorldTransform.Basis.getRotation(out orn);
                return orn;
            }
        }
        public btVector3 LinearVelocity { get { return m_linearVelocity; } set { m_linearVelocity = value; } }
        public btVector3 AngularVelocity { get { return m_angularVelocity; } set { m_angularVelocity = value; } }
        public btVector3 getVelocityInLocalPoint(btVector3 rel_pos)
        {
            //we also calculate lin/ang velocity for kinematic objects
            return m_linearVelocity + m_angularVelocity.cross(rel_pos);

            //for kinematic objects, we could also use use:
            //		return 	(m_worldTransform(rel_pos) - m_interpolationWorldTransform(rel_pos)) / m_kinematicTimeStep;
        }
        public void translate(btVector3 v)
        {
            WorldTransform.Origin += v;
        }

        public void getAabb(out btVector3 aabbMin, out btVector3 aabbMax)
        {
            CollisionShape.getAabb(WorldTransform, out aabbMin, out aabbMax);
        }

        public float computeImpulseDenominator(btVector3 pos, btVector3 normal)
        {
            btVector3 r0 = pos - CenterOfMassPosition;

            btVector3 c0 = (r0).cross(normal);

            btVector3 vec;
            #region btVector3 vec = (c0 * InvInertiaTensorWorld).cross(r0);
            {
                btVector3 temp;
                btMatrix3x3.Multiply(ref c0, ref InvInertiaTensorWorld, out temp);
                vec = temp.cross(r0);
            }
            #endregion
            return m_inverseMass + normal.dot(vec);

        }

        public float computeAngularImpulseDenominator(btVector3 axis)
        {
            btVector3 vec;// = axis * InvInertiaTensorWorld;
            btMatrix3x3.Multiply(ref axis, ref InvInertiaTensorWorld, out vec);
            return axis.dot(vec);
        }

        public void updateDeactivation(float timeStep)
        {
            if ((ActivationState == ActivationStateFlags.ISLAND_SLEEPING) || (ActivationState == ActivationStateFlags.DISABLE_DEACTIVATION))
                return;

            if ((LinearVelocity.Length2 < m_linearSleepingThreshold * m_linearSleepingThreshold) &&
                (AngularVelocity.Length2 < m_angularSleepingThreshold * m_angularSleepingThreshold))
            {
                m_deactivationTime += timeStep;
            }
            else
            {
                m_deactivationTime = 0f;
                ActivationState = 0;
            }

        }
        public bool wantsSleeping()
        {

            if (ActivationState == ActivationStateFlags.DISABLE_DEACTIVATION)
                return false;

            //disable deactivation
            if (gDisableDeactivation || (gDeactivationTime == 0f))
                return false;

            if ((ActivationState == ActivationStateFlags.ISLAND_SLEEPING) || (ActivationState == ActivationStateFlags.WANTS_DEACTIVATION))
                return true;

            if (m_deactivationTime > gDeactivationTime)
            {
                return true;
            }
            return false;
        }
        public BroadphaseProxy BroadphaseProxy { get { return m_broadphaseHandle; } set { m_broadphaseHandle = value; } }
        public IMotionState MotionState
        {
            get { return m_optionalMotionState; }
            set
            {
                m_optionalMotionState = value;
                if (m_optionalMotionState != null)
                    value.getWorldTransform(out WorldTransform);
            }
        }
        //for experimental overriding of friction/contact solver func
        public int m_contactSolverType;
        public int m_frictionSolverType;
        public void setAngularFactor(float angFac)
        {
            m_angularFactor.setValue(angFac, angFac, angFac);
        }
        public btVector3 AngularFactor
        {
            get { return m_angularFactor; }
            set { m_angularFactor = value; }
        }
        public bool isInWorld { get { return (BroadphaseProxy != null); } }
        public override bool checkCollideWithOverride(CollisionObject co)
        {
            RigidBody otherRb = co as RigidBody;
            if (otherRb == null)
                return true;

            for (int i = 0; i < m_constraintRefs.Count;i++ )
            {
                TypedConstraint c = m_constraintRefs[i];
                if (c.RigidBodyA == otherRb || c.RigidBodyB == otherRb)
                    return false;
            }

            return true;
        }
        public void addConstraintRef(TypedConstraint c)
        {
            int index = m_constraintRefs.IndexOf(c);
            if (index == -1)
                m_constraintRefs.Add(c);

            m_checkCollideWith = true;
        }
        public void removeConstraintRef(TypedConstraint c)
        {
            m_constraintRefs.Remove(c);
            m_checkCollideWith = m_constraintRefs.Count > 0;
        }

        public TypedConstraint getConstraintRef(int index)
        {
            return m_constraintRefs[index];
        }
        public int NumConstraintRefs { get { return m_constraintRefs.Count; } }
        public RigidBodyFlags RigidBodyFlag { get { return m_rigidbodyFlags; } set { m_rigidbodyFlags = value; } }

        ////////////////////////////////////////////////
        ///some internal methods, don't use them
        internal btVector3 internalDeltaLinearVelocity { get { return m_deltaLinearVelocity; } set { m_deltaLinearVelocity = value; } }
        internal btVector3 internalDeltaAngularVelocity { get { return m_deltaAngularVelocity; } set { m_deltaAngularVelocity = value; } }
        internal btVector3 internalAngularFactor { get { return m_angularFactor; } }
        internal btVector3 internalInvMass { get { return m_invMass; } }
        internal btVector3 internalPushVelocity { get { return m_pushVelocity; } }
        internal btVector3 internalTurnVelocity { get { return m_turnVelocity; } }
        internal void internalGetVelocityInLocalPointObsolete(btVector3 rel_pos, out btVector3 velocity)
        {
            velocity = LinearVelocity + m_deltaLinearVelocity + (AngularVelocity + m_deltaAngularVelocity).cross(rel_pos);
        }
        internal btVector3 internalGetAngularVelocity
        {
            get
            {
                return AngularVelocity + m_deltaAngularVelocity;
            }
        }
        internal void internalApplyImpulse(btVector3 linearComponent, btVector3 angularComponent, float impulseMagnitude)
        {
            if (m_inverseMass != 0)
            {
                m_deltaLinearVelocity += linearComponent * impulseMagnitude;
                m_deltaAngularVelocity += angularComponent * (impulseMagnitude * m_angularFactor);
            }
        }
        internal void internalApplyPushImpulse(btVector3 linearComponent, btVector3 angularComponent, float impulseMagnitude)
        {
            if (m_inverseMass != 0f)
            {
                m_pushVelocity += linearComponent * impulseMagnitude;
                m_turnVelocity += angularComponent * (impulseMagnitude * m_angularFactor);
            }
        }
        internal void internalWritebackVelocity()
        {
            if (m_inverseMass != 0)
            {
                LinearVelocity += m_deltaLinearVelocity;
                AngularVelocity += m_deltaAngularVelocity;
                m_deltaLinearVelocity.setZero();
                m_deltaAngularVelocity.setZero();
                //m_originalBody->setCompanionId(-1);
            }
        }
        internal void internalWritebackVelocity(float timeStep)
        {
            if (m_inverseMass != 0)
            {
                LinearVelocity += m_deltaLinearVelocity;
                AngularVelocity += m_deltaAngularVelocity;

                //correct the position/orientation based on push/turn recovery
                btTransform newTransform;
                TransformUtil.integrateTransform(WorldTransform, m_pushVelocity, m_turnVelocity, timeStep, out newTransform);
                WorldTransform = newTransform;
                //m_originalBody->setCompanionId(-1);
            }
            m_deltaLinearVelocity.setZero();
            m_deltaAngularVelocity.setZero();
            m_pushVelocity.setZero();
            m_turnVelocity.setZero();
        }

        ///////////////////////////////////////////////
#if false
        virtual	int	calculateSerializeBufferSize()	const;

	    ///fills the dataBuffer and returns the struct name (and 0 on failure)
	    virtual	const char*	serialize(void* dataBuffer,  class btSerializer* serializer) const;

	    virtual void serializeSingleObject(class btSerializer* serializer) const;


#endif
    }
}
