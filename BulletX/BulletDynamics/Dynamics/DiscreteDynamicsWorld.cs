using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletDynamics.ConstraintSolver;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.Dynamics
{
    public class DiscreteDynamicsWorld : DynamicsWorld
    {
        protected IConstraintSolver m_constraintSolver;

        protected SimulationIslandManager m_islandManager;

        protected List<TypedConstraint> m_constraints = new List<TypedConstraint>();

        protected List<RigidBody> m_nonStaticRigidBodies = new List<RigidBody>();

        protected btVector3 m_gravity;

        //for variable timesteps
        protected float m_localTime;
        //for variable timesteps

        //メモリ開放用
        //protected bool m_ownsIslandManager;
        //protected bool m_ownsConstraintSolver;
        protected bool m_synchronizeAllMotionStates;

        protected List<ActionInterface> m_actions = new List<ActionInterface>();

        protected int m_profileTimings;

        protected virtual void predictUnconstraintMotion(float timeStep)
        {
            BulletGlobal.StartProfile("0-0 predictUnconstraintMotion");
            for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
            {
                RigidBody body = m_nonStaticRigidBodies[i];
                if (!body.isStaticOrKinematicObject)
                {
                    body.integrateVelocities(timeStep);
                    //damping
                    body.applyDamping(timeStep);
                    btTransform temp;
                    body.predictIntegratedTransform(timeStep, out temp);
                    body.InterpolationWorldTransform = temp;
                }
            }
            BulletGlobal.EndProfile("0-0 predictUnconstraintMotion");
        }
        //static int gNumClampedCcdMotions = 0;
        protected virtual void integrateTransforms(float timeStep)
        {
            BulletGlobal.StartProfile("0-4 integrateTransforms");
            btTransform predictedTrans;
            for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
            {
                RigidBody body = m_nonStaticRigidBodies[i];
                body.HitFraction = 1f;

                if (body.isActive && (!body.isStaticOrKinematicObject))
                {
                    body.predictIntegratedTransform(timeStep, out predictedTrans);
                    float squareMotion = (predictedTrans.Origin - body.WorldTransform.Origin).Length2;

                    if (body.CcdSquareMotionThreshold > 0 && body.CcdSquareMotionThreshold < squareMotion)
                    {

                        BulletGlobal.StartProfile("0-4-1 CCD motion clamping");
                        if (body.CollisionShape.isConvex)
                        {
                            throw new NotImplementedException("Bullet2.76ではここに到達はしないはず……");
#if false//未移植
                            gNumClampedCcdMotions++;
        					
					        ClosestNotMeConvexResultCallback sweepResults(body,body.WorldTransform.Origin,predictedTrans.Origin, getBroadphase().getOverlappingPairCache(),Dispatcher);
					        //btConvexShape* convexShape = static_cast<btConvexShape*>(body->getCollisionShape());
					        SphereShape* tmpSphere(body.CcdSweptSphereRadius))//btConvexShape* convexShape = static_cast<btConvexShape*>(body->getCollisionShape());
                            

					        sweepResults.m_collisionFilterGroup = body.BroadphaseProxy.m_collisionFilterGroup;
					        sweepResults.m_collisionFilterMask  = body.BroadphaseProxy.m_collisionFilterMask;

					        convexSweepTest(&tmpSphere,body.WorldTransform,predictedTrans,sweepResults);
					        if (sweepResults.hasHit() && (sweepResults.m_closestHitFraction < 1f))
					        {
						        body.HitFraction=sweepResults.m_closestHitFraction;
						        body.predictIntegratedTransform(timeStep*body.HitFraction,out predictedTrans);
						        body.HitFraction=0f;
        //							printf("clamped integration to hit fraction = %f\n",fraction);
					        }
#endif
                        }
                        BulletGlobal.EndProfile("0-4-1 CCD motion clamping");
                    }

                    body.proceedToTransform(predictedTrans);
                }
            }
            BulletGlobal.EndProfile("0-4 integrateTransforms");
        }
        protected virtual void calculateSimulationIslands()
        {
            BulletGlobal.StartProfile("0-2 calculateSimulationIslands");
            
            SimulationIslandManager.updateActivationState(this, Dispatcher);

            for (int i = 0; i < m_constraints.Count;i++ )
            {
                TypedConstraint constraint = m_constraints[i];
                RigidBody colObj0 = constraint.RigidBodyA;
                RigidBody colObj1 = constraint.RigidBodyB;

                if (((colObj0 != null) && (!colObj0.isStaticOrKinematicObject)) &&
                    ((colObj1 != null) && (!colObj1.isStaticOrKinematicObject)))
                {
                    if (colObj0.isActive || colObj1.isActive)
                    {

                        SimulationIslandManager.UnionFind.unite(colObj0.IslandTag,
                            colObj1.IslandTag);
                    }
                }
            }

            //Store the island id in each body
            SimulationIslandManager.storeIslandActivationState(this);

            BulletGlobal.EndProfile("0-2 calculateSimulationIslands");
        }
        //オブジェクトプール……
        List<TypedConstraint> sortedConstraints = new List<TypedConstraint>();
        SortConstraintOnIslandPredicate pred = new SortConstraintOnIslandPredicate();
        InplaceSolverIslandCallback solverCallback = new InplaceSolverIslandCallback();
        protected virtual void solveConstraints(ContactSolverInfo solverInfo)
        {
            BulletGlobal.StartProfile("0-3 solveConstraints");
            //sorted version of all btTypedConstraint, based on islandId
            //btAlignedObjectArray<btTypedConstraint*>	sortedConstraints;
            //sortedConstraints.resize( m_constraints.size());
            BulletGlobal.StartProfile("0-3-0 prepareSolve");
            sortedConstraints.Clear();
            int i;
            for (i = 0; i < NumConstraints; i++)
            {
                sortedConstraints.Add(m_constraints[i]);
            }
            
            //	btAssert(0);



            sortedConstraints.Sort(pred);
            
            List<TypedConstraint> constraintsPtr = NumConstraints != 0 ? sortedConstraints : null;

            solverCallback.Constructor(solverInfo, m_constraintSolver, constraintsPtr, sortedConstraints.Count, m_debugDrawer, m_dispatcher1);

            m_constraintSolver.prepareSolve(NumCollisionObjects, Dispatcher.NumManifolds);
            BulletGlobal.EndProfile("0-3-0 prepareSolve");
            /// solve all the constraints for this island
            m_islandManager.buildAndProcessIslands(Dispatcher, this, solverCallback);
            
            BulletGlobal.StartProfile("processConstraints");
            solverCallback.processConstraints();
            BulletGlobal.EndProfile("processConstraints");
            
            BulletGlobal.StartProfile("allSolved");
            m_constraintSolver.allSolved(solverInfo, m_debugDrawer);
            BulletGlobal.EndProfile("allSolved");
            BulletGlobal.EndProfile("0-3 solveConstraints");
        }
        
        protected void updateActivationState(float timeStep)
        {
            BulletGlobal.StartProfile("0-6 updateActivationState");
            for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
            {
                RigidBody body = m_nonStaticRigidBodies[i];
                if (body != null)
                {
                    body.updateDeactivation(timeStep);

                    if (body.wantsSleeping())
                    {
                        if (body.isStaticOrKinematicObject)
                        {
                            body.ActivationState = ActivationStateFlags.ISLAND_SLEEPING;
                        }
                        else
                        {
                            if (body.ActivationState == ActivationStateFlags.ACTIVE_TAG)
                                body.ActivationState = ActivationStateFlags.WANTS_DEACTIVATION;
                            if (body.ActivationState == ActivationStateFlags.ISLAND_SLEEPING)
                            {
                                body.AngularVelocity = new btVector3(0, 0, 0);
                                body.LinearVelocity = new btVector3(0, 0, 0);
                            }

                        }
                    }
                    else
                    {
                        if (body.ActivationState != ActivationStateFlags.DISABLE_DEACTIVATION)
                            body.ActivationState = ActivationStateFlags.ACTIVE_TAG;
                    }
                }
            }
            BulletGlobal.EndProfile("0-6 updateActivationState");
        }
        protected void updateActions(float timeStep)
        {
            BulletGlobal.StartProfile("0-5 updateActions");
            for (int i = 0; i < m_actions.Count;i++ )
            {
                var action = m_actions[i];
                action.updateAction(this, timeStep);
            }
            BulletGlobal.EndProfile("0-5 updateActions");
        }
        
        protected void startProfiling(float timeStep)
        {
            BulletGlobal.BeginProfileFrame();
        }
        protected void endProfiling()
        {
            BulletGlobal.EndProfileFrame();
        }
        protected virtual void internalSingleStepSimulation(float timeStep)
        {
            BulletGlobal.StartProfile("0 internalSingleStepSimulation");
            OnInternalPreTickCallback(this, timeStep);
            
            ///apply gravity, predict motion
            predictUnconstraintMotion(timeStep);

            DispatcherInfo dispatchInfo = DispatchInfo;

            dispatchInfo.m_timeStep = timeStep;
            dispatchInfo.m_stepCount = 0;
            dispatchInfo.m_debugDraw = DebugDrawer;

            ///perform collision detection
            performDiscreteCollisionDetection();

            calculateSimulationIslands();


            SolverInfo.m_timeStep = timeStep;


            ///solve contact and other joint constraints
            solveConstraints(SolverInfo);

            ///CallbackTriggers();
            ///integrate transforms
            integrateTransforms(timeStep);

            ///update vehicle simulation
            updateActions(timeStep);

            updateActivationState(timeStep);

            OnInternalTickCallback(this, timeStep);
            BulletGlobal.EndProfile("0 internalSingleStepSimulation");
        }
        protected virtual void saveKinematicState(float timeStep)
        {
            ///would like to iterate over m_nonStaticRigidBodies, but unfortunately old API allows
            ///to switch status _after_ adding kinematic objects to the world
            ///fix it for Bullet 3.x release
            for (int i = 0; i < m_collisionObjects.Count;i++ )
            {
                CollisionObject colObj = m_collisionObjects[i];
                RigidBody body = colObj as RigidBody;
                if (body != null && body.ActivationState != ActivationStateFlags.ISLAND_SLEEPING)
                {
                    if (body.isKinematicObject)
                    {
                        //to calculate velocities next frame
                        body.saveKinematicState(timeStep);
                    }
                }
            }
        }
#if false
        protected void serializeRigidBodies(btSerializer* serializer);
#endif

        ///this btDiscreteDynamicsWorld constructor gets created objects from the user, and will not delete those
        public DiscreteDynamicsWorld(IDispatcher dispatcher, IBroadphaseInterface pairCache,
                    IConstraintSolver constraintSolver, ICollisionConfiguration collisionConfiguration)
            : base(dispatcher, pairCache, collisionConfiguration)
        {
            m_constraintSolver = constraintSolver;
            m_gravity = new btVector3(0, -10, 0);
            m_localTime = 1.0f / 60.0f;
            m_synchronizeAllMotionStates = false;
            m_profileTimings = 0;
            if (m_constraintSolver == null)
            {
                m_constraintSolver = new SequentialImpulseConstraintSolver();
                //m_ownsConstraintSolver = true;
            } /*else
	        {
		        m_ownsConstraintSolver = false;
	        }*/

            {
                m_islandManager = new SimulationIslandManager();
            }

            //m_ownsIslandManager = true;
        }
        public override int stepSimulation(float timeStep)
        {
            return stepSimulation(timeStep, 1, 1f / 60f);
        }
        public override int stepSimulation(float timeStep, int maxSubSteps)
        {
            return stepSimulation(timeStep, maxSubSteps, 1f / 60f);
        }
        public override int stepSimulation(float timeStep, int maxSubSteps, float fixedTimeStep)
        {
            startProfiling(timeStep);

            BulletGlobal.StartProfile("stepSimulation");

            int numSimulationSubSteps = 0;

            if (maxSubSteps != 0)
            {
                //fixed timestep with interpolation
                m_localTime += timeStep;
                if (m_localTime >= fixedTimeStep)
                {
                    numSimulationSubSteps = (int)(m_localTime / fixedTimeStep);
                    m_localTime -= numSimulationSubSteps * fixedTimeStep;
                }
            }
            else
            {
                //variable timestep
                fixedTimeStep = timeStep;
                m_localTime = timeStep;
                if (Math.Abs(timeStep) < BulletGlobal.SIMD_EPSILON)
                {
                    numSimulationSubSteps = 0;
                    maxSubSteps = 0;
                }
                else
                {
                    numSimulationSubSteps = 1;
                    maxSubSteps = 1;
                }
            }

            //process some debugging flags
            if (DebugDrawer != null)
            {
                IDebugDraw debugDrawer = DebugDrawer;
                RigidBody.gDisableDeactivation = (debugDrawer.DebugMode & DebugDrawModes.DBG_NoDeactivation) != 0;
            }
            if (numSimulationSubSteps != 0)
            {

                //clamp the number of substeps, to prevent simulation grinding spiralling down to a halt
                int clampedSimulationSteps = (numSimulationSubSteps > maxSubSteps) ? maxSubSteps : numSimulationSubSteps;

                saveKinematicState(fixedTimeStep * clampedSimulationSteps);

                applyGravity();



                for (int i = 0; i < clampedSimulationSteps; i++)
                {
                    internalSingleStepSimulation(fixedTimeStep);
                    synchronizeMotionStates();
                }

            }
            else
            {
                synchronizeMotionStates();
            }

            clearForces();

            BulletGlobal.EndProfile("stepSimulation");
            BulletGlobal.EndProfileFrame();
            return numSimulationSubSteps;
        }
        public override void synchronizeMotionStates()
        {
            BulletGlobal.StartProfile("1 synchronizeMotionStates");
            if (m_synchronizeAllMotionStates)
            {
                //iterate  over all collision objects
                for (int i=0;i<m_collisionObjects.Count;i++)
                {
                    CollisionObject colObj = m_collisionObjects[i];
                    RigidBody body = colObj as RigidBody;
                    if (body != null)
                        synchronizeSingleMotionState(body);
                }
            }
            else
            {
                //iterate over all active rigid bodies
                for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
                {
                    RigidBody body = m_nonStaticRigidBodies[i];
                    if (body.isActive)
                        synchronizeSingleMotionState(body);
                }
            }
            BulletGlobal.EndProfile("1 synchronizeMotionStates");
        }
        public void synchronizeSingleMotionState(RigidBody body)
        {
            Debug.Assert(body != null);

            if (body.MotionState != null && !body.isStaticOrKinematicObject)
            {
                //we need to call the update at least once, even for sleeping objects
                //otherwise the 'graphics' transform never updates properly
                ///@todo: add 'dirty' flag
                //if (body->getActivationState() != ISLAND_SLEEPING)
                {
                    btTransform interpolatedTransform;
                    TransformUtil.integrateTransform(body.InterpolationWorldTransform,
                        body.InterpolationLinearVelocity, body.InterpolationAngularVelocity, m_localTime * body.HitFraction, out interpolatedTransform);
                    body.MotionState.setWorldTransform(interpolatedTransform);
                }
            }
        }
        public override void addConstraint(TypedConstraint constraint)
        {
            addConstraint(constraint, false);
        }
        public override void addConstraint(TypedConstraint constraint, bool disableCollisionsBetweenLinkedBodies)
        {
            m_constraints.Add(constraint);
            if (disableCollisionsBetweenLinkedBodies)
            {
                constraint.RigidBodyA.addConstraintRef(constraint);
                constraint.RigidBodyB.addConstraintRef(constraint);
            }
        }
        public override void removeConstraint(TypedConstraint constraint)
        {
            m_constraints.Remove(constraint);
            constraint.RigidBodyA.removeConstraintRef(constraint);
            constraint.RigidBodyB.removeConstraintRef(constraint);
        }

        public override void addAction(ActionInterface action)
        {
            m_actions.Add(action);
        }
        public override void removeAction(ActionInterface action)
        {
            m_actions.Remove(action);
        }
        public SimulationIslandManager SimulationIslandManager { get { return m_islandManager; } }
        public override btVector3 Gravity
        {
            get
            {
                return m_gravity;
            }
            set
            {
                m_gravity = value;

                for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
                {
                    var body = m_nonStaticRigidBodies[i];
                    if (body.isActive && ((body.RigidBodyFlag & RigidBodyFlags.BT_DISABLE_WORLD_GRAVITY) == 0))
                    {
                        body.Gravity = value;
                    }
                }
            }
        }

        public override void addCollisionObject(CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask)
        {
            base.addCollisionObject(collisionObject, collisionFilterGroup, collisionFilterMask);
        }

        public override void addRigidBody(RigidBody body)
        {
            if (!body.isStaticOrKinematicObject && 0==(body.RigidBodyFlag & RigidBodyFlags.BT_DISABLE_WORLD_GRAVITY))
	        {
		        body.Gravity=m_gravity;
	        }

	        if (body.CollisionShape!=null)
	        {
		        if (!body.isStaticObject)
		        {
			        m_nonStaticRigidBodies.Add(body);
		        } else
		        {
			        body.ActivationState= ActivationStateFlags.ISLAND_SLEEPING;
		        }

		        bool isDynamic = !(body.isStaticObject || body.isKinematicObject);
		        short collisionFilterGroup = isDynamic? (short)(CollisionFilterGroups.DefaultFilter) : (short)(CollisionFilterGroups.StaticFilter);
                short collisionFilterMask = isDynamic ? (short)(CollisionFilterGroups.AllFilter) : (short)(CollisionFilterGroups.AllFilter ^ CollisionFilterGroups.StaticFilter);

		        addCollisionObject(body,collisionFilterGroup,collisionFilterMask);
	        }
        }
        public virtual void addRigidBody(RigidBody body, short group, short mask)
        {
            if (!body.isStaticOrKinematicObject && ((body.RigidBodyFlag & RigidBodyFlags.BT_DISABLE_WORLD_GRAVITY)) == 0)
            {
                body.Gravity=m_gravity;
            }

            if (body.CollisionShape != null)
            {
                if (!body.isStaticObject)
                {
                    m_nonStaticRigidBodies.Add(body);
                }
                else
                {
                    body.ActivationState = ActivationStateFlags.ISLAND_SLEEPING;
                }
                addCollisionObject(body, group, mask);
            }
        }
        public override void removeRigidBody(RigidBody body)
        {
            m_nonStaticRigidBodies.Remove(body);
	        base.removeCollisionObject(body);
        }

        ///removeCollisionObject will first check if it is a rigid body, if so call removeRigidBody otherwise call btCollisionWorld::removeCollisionObject
        public override void removeCollisionObject(CollisionObject collisionObject)
        {
            RigidBody body = collisionObject as RigidBody;
            if (body != null)
                removeRigidBody(body);
            else
                base.removeCollisionObject(collisionObject);
        }
        public void debugDrawConstraint(TypedConstraint constraint)
        {
            bool drawFrames = (DebugDrawer.DebugMode & DebugDrawModes.DBG_DrawConstraints) != 0;
            bool drawLimits = (DebugDrawer.DebugMode & DebugDrawModes.DBG_DrawConstraintLimits) != 0;
	        float dbgDrawSize = constraint.DbgDrawSize;
	        if(dbgDrawSize <= 0f)
	        {
		        return;
	        }

	        switch(constraint.ConstraintType)
	        {
                case TypedConstraintType.POINT2POINT_CONSTRAINT_TYPE:
                    throw new NotImplementedException();
#if false
			        {
				        btPoint2PointConstraint* p2pC = (btPoint2PointConstraint*)constraint;
				        btTransform tr;
				        tr.setIdentity();
				        btVector3 pivot = p2pC->getPivotInA();
				        pivot = p2pC->getRigidBodyA().getCenterOfMassTransform() * pivot; 
				        tr.setOrigin(pivot);
				        getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        // that ideally should draw the same frame	
				        pivot = p2pC->getPivotInB();
				        pivot = p2pC->getRigidBodyB().getCenterOfMassTransform() * pivot; 
				        tr.setOrigin(pivot);
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
			        }
			        break;
#endif
                case TypedConstraintType.HINGE_CONSTRAINT_TYPE:
                    throw new NotImplementedException();
#if false
			        {
				        btHingeConstraint* pHinge = (btHingeConstraint*)constraint;
				        btTransform tr = pHinge->getRigidBodyA().getCenterOfMassTransform() * pHinge->getAFrame();
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        tr = pHinge->getRigidBodyB().getCenterOfMassTransform() * pHinge->getBFrame();
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        btScalar minAng = pHinge->getLowerLimit();
				        btScalar maxAng = pHinge->getUpperLimit();
				        if(minAng == maxAng)
				        {
					        break;
				        }
				        bool drawSect = true;
				        if(minAng > maxAng)
				        {
					        minAng = btScalar(0.f);
					        maxAng = SIMD_2_PI;
					        drawSect = false;
				        }
				        if(drawLimits) 
				        {
					        btVector3& center = tr.getOrigin();
					        btVector3 normal = tr.getBasis().getColumn(2);
					        btVector3 axis = tr.getBasis().getColumn(0);
					        getDebugDrawer()->drawArc(center, normal, axis, dbgDrawSize, dbgDrawSize, minAng, maxAng, btVector3(0,0,0), drawSect);
				        }
			        }
			        break;
#endif
                case TypedConstraintType.CONETWIST_CONSTRAINT_TYPE:
                    throw new NotImplementedException();
#if false
			        {
				        btConeTwistConstraint* pCT = (btConeTwistConstraint*)constraint;
				        btTransform tr = pCT->getRigidBodyA().getCenterOfMassTransform() * pCT->getAFrame();
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        tr = pCT->getRigidBodyB().getCenterOfMassTransform() * pCT->getBFrame();
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        if(drawLimits)
				        {
					        //const btScalar length = btScalar(5);
					        const btScalar length = dbgDrawSize;
					        static int nSegments = 8*4;
					        btScalar fAngleInRadians = btScalar(2.*3.1415926) * (btScalar)(nSegments-1)/btScalar(nSegments);
					        btVector3 pPrev = pCT->GetPointForAngle(fAngleInRadians, length);
					        pPrev = tr * pPrev;
					        for (int i=0; i<nSegments; i++)
					        {
						        fAngleInRadians = btScalar(2.*3.1415926) * (btScalar)i/btScalar(nSegments);
						        btVector3 pCur = pCT->GetPointForAngle(fAngleInRadians, length);
						        pCur = tr * pCur;
						        getDebugDrawer()->drawLine(pPrev, pCur, btVector3(0,0,0));

						        if (i%(nSegments/8) == 0)
							        getDebugDrawer()->drawLine(tr.getOrigin(), pCur, btVector3(0,0,0));

						        pPrev = pCur;
					        }						
					        btScalar tws = pCT->getTwistSpan();
					        btScalar twa = pCT->getTwistAngle();
					        bool useFrameB = (pCT->getRigidBodyB().getInvMass() > btScalar(0.f));
					        if(useFrameB)
					        {
						        tr = pCT->getRigidBodyB().getCenterOfMassTransform() * pCT->getBFrame();
					        }
					        else
					        {
						        tr = pCT->getRigidBodyA().getCenterOfMassTransform() * pCT->getAFrame();
					        }
					        btVector3 pivot = tr.getOrigin();
					        btVector3 normal = tr.getBasis().getColumn(0);
					        btVector3 axis1 = tr.getBasis().getColumn(1);
					        getDebugDrawer()->drawArc(pivot, normal, axis1, dbgDrawSize, dbgDrawSize, -twa-tws, -twa+tws, btVector3(0,0,0), true);

				        }
			        }
			        break;
#endif
                case TypedConstraintType.D6_CONSTRAINT_TYPE:
			        {
				        Generic6DofConstraint p6DOF = (Generic6DofConstraint)constraint;
				        btTransform tr = p6DOF.CalculatedTransformA;
				        if(drawFrames) DebugDrawer.drawTransform(ref tr, dbgDrawSize);
				        tr = p6DOF.CalculatedTransformB;
				        if(drawFrames) DebugDrawer.drawTransform(ref tr, dbgDrawSize);
				        if(drawLimits)
                        {
                            tr = p6DOF.CalculatedTransformA;
                            btVector3 center = p6DOF.CalculatedTransformB.Origin;
                            btVector3 up;// = tr.Basis.getColumn(2);
                            tr.Basis.getColumn(2, out up);
                            btVector3 axis;// = tr.Basis.getColumn(0);
                            tr.Basis.getColumn(0, out axis);
                            float minTh = p6DOF.getRotationalLimitMotor(1).m_loLimit;
                            float maxTh = p6DOF.getRotationalLimitMotor(1).m_hiLimit;
                            float minPs = p6DOF.getRotationalLimitMotor(2).m_loLimit;
                            float maxPs = p6DOF.getRotationalLimitMotor(2).m_hiLimit;
                            DebugDrawer.drawSpherePatch(ref center, ref  up, ref axis, dbgDrawSize * 0.9f, minTh, maxTh, minPs, maxPs, new btVector3(0, 0, 0));
                            //axis = tr.Basis.getColumn(1);
                            tr.Basis.getColumn(1, out axis);
                            float ay = p6DOF.getAngle(1);
                            float az = p6DOF.getAngle(2);
                            float cy = (float)Math.Cos(ay);
                            float sy = (float)Math.Sin(ay);
                            float cz = (float)Math.Cos(az);
                            float sz = (float)Math.Sin(az);
                            btVector3 refer = btVector3.Zero;
                            refer.X = cy * cz * axis.X + cy * sz * axis.Y - sy * axis.Z;
                            refer.Y = -sz * axis.X + cz * axis.Y;
                            refer.Z = cz * sy * axis.X + sz * sy * axis.Y + cy * axis.Z;
                            tr = p6DOF.CalculatedTransformB;
                            btVector3 normal;
                            #region btVector3 normal = -tr.Basis.getColumn(0);
                            {
                                btVector3 temp;
                                tr.Basis.getColumn(0, out temp);
                                normal = -temp;
                            }
                            #endregion
                            float minFi = p6DOF.getRotationalLimitMotor(0).m_loLimit;
                            float maxFi = p6DOF.getRotationalLimitMotor(0).m_hiLimit;
                            if (minFi > maxFi)
                            {
                                DebugDrawer.drawArc(ref center,ref normal,ref refer, dbgDrawSize, dbgDrawSize, -BulletGlobal.SIMD_PI, BulletGlobal.SIMD_PI, new btVector3(0, 0, 0), false);
                            }
                            else if (minFi < maxFi)
                            {
                                DebugDrawer.drawArc(ref  center,ref normal,ref refer, dbgDrawSize, dbgDrawSize, minFi, maxFi, new btVector3(0, 0, 0), true);
                            }
                            tr = p6DOF.CalculatedTransformA;
                            btVector3 bbMin = p6DOF.TranslationalLimitMotor.m_lowerLimit;
                            btVector3 bbMax = p6DOF.TranslationalLimitMotor.m_upperLimit;
                            {
                                btVector3 temp = new btVector3(0, 0, 0);
                                DebugDrawer.drawBox(ref bbMin, ref  bbMax, ref tr, ref temp);
                            }
                        }
			        }
			        break;
                case TypedConstraintType.SLIDER_CONSTRAINT_TYPE:
                    throw new NotImplementedException();
#if false
			        {
				        btSliderConstraint* pSlider = (btSliderConstraint*)constraint;
				        btTransform tr = pSlider->getCalculatedTransformA();
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        tr = pSlider->getCalculatedTransformB();
				        if(drawFrames) getDebugDrawer()->drawTransform(tr, dbgDrawSize);
				        if(drawLimits)
				        {
					        btTransform tr = pSlider->getUseLinearReferenceFrameA() ? pSlider->getCalculatedTransformA() : pSlider->getCalculatedTransformB();
					        btVector3 li_min = tr * btVector3(pSlider->getLowerLinLimit(), 0.f, 0.f);
					        btVector3 li_max = tr * btVector3(pSlider->getUpperLinLimit(), 0.f, 0.f);
					        getDebugDrawer()->drawLine(li_min, li_max, btVector3(0, 0, 0));
					        btVector3 normal = tr.getBasis().getColumn(0);
					        btVector3 axis = tr.getBasis().getColumn(1);
					        btScalar a_min = pSlider->getLowerAngLimit();
					        btScalar a_max = pSlider->getUpperAngLimit();
					        const btVector3& center = pSlider->getCalculatedTransformB().getOrigin();
					        getDebugDrawer()->drawArc(center, normal, axis, dbgDrawSize, dbgDrawSize, a_min, a_max, btVector3(0,0,0), true);
				        }
			        }
			        break;
#endif
		        default : 
			        break;
	        }
	        return;
        }

        public override void debugDrawWorld()
        {
            //マルチスレッド時に上手く動作しないのでコメントアウト
            //BulletGlobal.StartProfile("debugDrawWorld");

	        base.debugDrawWorld();

	        bool drawConstraints = false;
	        if (DebugDrawer!=null)
	        {
		        DebugDrawModes mode = DebugDrawer.DebugMode;
		        if((mode  & ( DebugDrawModes.DBG_DrawConstraints | DebugDrawModes.DBG_DrawConstraintLimits))!=0)
		        {
			        drawConstraints = true;
		        }
	        }
	        if(drawConstraints)
	        {
		        for(int i = NumConstraints-1; i>=0 ;i--)
		        {
			        TypedConstraint constraint = getConstraint(i);
			        debugDrawConstraint(constraint);
		        }
	        }



	        if (DebugDrawer!=null && (DebugDrawer.DebugMode & ( DebugDrawModes.DBG_DrawWireframe | DebugDrawModes.DBG_DrawAabb))!=0)
	        {
		        int i;

		        if (DebugDrawer!=null && DebugDrawer.DebugMode!=0)
		        {
			        for (i=0;i<m_actions.Count;i++)
			        {
				        m_actions[i].debugDraw(m_debugDrawer);
			        }
		        }
	        }
            //BulletGlobal.EndProfile("debugDrawWorld");
        }

        public override IConstraintSolver ConstraintSolver { get { return m_constraintSolver; } set { m_constraintSolver = value; } }

        public override int NumConstraints { get { return m_constraints.Count; } }

        public override TypedConstraint getConstraint(int index)
        {
            return m_constraints[index];
        }

        public override DynamicsWorldType WorldType { get { return DynamicsWorldType.BT_DISCRETE_DYNAMICS_WORLD; } }
        ///the forces on each rigidbody is accumulating together with gravity. clear this after each timestep.
        public override void clearForces()
        {
            ///@todo: iterate over awake simulation islands!
            for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
            {
                RigidBody body = m_nonStaticRigidBodies[i];
                //need to check if next line is ok
                //it might break backward compatibility (people applying forces on sleeping objects get never cleared and accumulate on wake-up
                body.clearForces();
            }
        }
        
        public virtual void applyGravity()
        {
            ///@todo: iterate over awake simulation islands!
            for (int i = 0; i < m_nonStaticRigidBodies.Count;i++ )
            {
                RigidBody body = m_nonStaticRigidBodies[i];
                if (body.isActive)
                {
                    body.applyGravity();
                }
            }
        }

        public virtual void setNumTasks(int numTasks) { }

        public bool SynchronizeAllMotionStates { get { return m_synchronizeAllMotionStates; } set { m_synchronizeAllMotionStates = value; } }
    }
}
