using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public class CollisionWorld
    {
        protected List<CollisionObject> m_collisionObjects = new List<CollisionObject>();

        protected IDispatcher m_dispatcher1;

        protected DispatcherInfo m_dispatchInfo = new DispatcherInfo();
        //メモリ確保系？
        //protected btStackAlloc* m_stackAlloc;

        protected IBroadphaseInterface m_broadphasePairCache;

        protected IDebugDraw m_debugDrawer;

        ///m_forceUpdateAllAabbs can be set to false as an optimization to only update active object AABBs
        ///it is true by default, because it is error-prone (setting the position of static objects wouldn't update their AABB)
        protected bool m_forceUpdateAllAabbs;

#if false
        protected void	serializeCollisionObjects(btSerializer* serializer);
#endif

        public static IProfiler Profiler { get { return BulletGlobal.profiler; } set { BulletGlobal.profiler = value; } }

        public CollisionWorld(IDispatcher dispatcher, IBroadphaseInterface pairCache, ICollisionConfiguration collisionConfiguration)
        {
            m_dispatcher1 = dispatcher;
            m_broadphasePairCache = pairCache;
            m_debugDrawer = null;
            m_forceUpdateAllAabbs = true;
            //メモリ確保系?
            //m_stackAlloc = collisionConfiguration->getStackAllocator();
            //m_dispatchInfo.m_stackAllocator = m_stackAlloc;
        }

        public IBroadphaseInterface Broadphase { get { return m_broadphasePairCache; } set { m_broadphasePairCache = value; } }
        public IOverlappingPairCache PairCache { get { return m_broadphasePairCache.OverlappingPairCache; } }
        public IDispatcher Dispatcher { get { return m_dispatcher1; } }


        static bool reportMe = true;
        public void updateSingleAabb(CollisionObject colObj)
        {
            btVector3 minAabb, maxAabb;
            colObj.CollisionShape.getAabb(colObj.WorldTransform, out minAabb, out maxAabb);
            //need to increase the aabb for contact thresholds
            btVector3 contactThreshold = new btVector3(PersistentManifold.gContactBreakingThreshold, PersistentManifold.gContactBreakingThreshold, PersistentManifold.gContactBreakingThreshold);
            //minAabb -= contactThreshold;
            //maxAabb += contactThreshold;
            minAabb.Subtract(ref contactThreshold);
            maxAabb.Add(ref contactThreshold);

            //IBroadphaseInterface bp = m_broadphasePairCache;

            //moving objects should be moderately sized, probably something wrong if not
            if (colObj.isStaticObject || ((maxAabb - minAabb).Length2 < 1e12f))
            {
                m_broadphasePairCache.setAabb(colObj.BroadphaseHandle,ref minAabb,ref maxAabb, m_dispatcher1);
            }
            else
            {
                //something went wrong, investigate
                //this assert is unwanted in 3D modelers (danger of loosing work)
                colObj.ActivationState = ActivationStateFlags.DISABLE_SIMULATION;

                if (reportMe && m_debugDrawer != null)
                {
                    reportMe = false;
                    m_debugDrawer.reportErrorWarning("Overflow in AABB, object removed from simulation");
                    m_debugDrawer.reportErrorWarning("If you can reproduce this, please email bugs@continuousphysics.com\n");
                    m_debugDrawer.reportErrorWarning("Please include above information, your Platform, version of OS.\n");
                    m_debugDrawer.reportErrorWarning("Thanks.\n");
                }
            }
        }
        public virtual void updateAabbs()
        {
            BulletGlobal.StartProfile("0-1-0 updateAabbs");

            //foreach (CollisionObject colObj in m_collisionObjects)
            for (int i = 0; i < m_collisionObjects.Count; i++)
            {
                CollisionObject colObj = m_collisionObjects[i];
                //only update aabb of active objects
                if (m_forceUpdateAllAabbs || colObj.isActive)
                {
                    updateSingleAabb(colObj);
                }
            }
            BulletGlobal.EndProfile("0-1-0 updateAabbs");
        }

        public virtual IDebugDraw DebugDrawer { get { return m_debugDrawer; } set { m_debugDrawer = value; } }

        public virtual void debugDrawWorld()
        {
            if (DebugDrawer != null && (DebugDrawer.DebugMode & DebugDrawModes.DBG_DrawContactPoints) != 0)
            {
                int numManifolds = Dispatcher.NumManifolds;
                btVector3 color = btVector3.Zero;
                for (int i = 0; i < numManifolds; i++)
                {
                    PersistentManifold contactManifold = Dispatcher.getManifoldByIndexInternal(i);
                    //btCollisionObject* obA = static_cast<btCollisionObject*>(contactManifold->getBody0());
                    //btCollisionObject* obB = static_cast<btCollisionObject*>(contactManifold->getBody1());

                    int numContacts = contactManifold.NumContacts;
                    for (int j = 0; j < numContacts; j++)
                    {
                        ManifoldPoint cp = contactManifold.getContactPoint(j);
                        DebugDrawer.drawContactPoint(ref cp.m_positionWorldOnB,ref cp.m_normalWorldOnB, cp.Distance, cp.LifeTime,ref color);
                    }
                }
            }

            if (DebugDrawer != null && ((DebugDrawer.DebugMode & (DebugDrawModes.DBG_DrawWireframe | DebugDrawModes.DBG_DrawAabb)) != 0))
            {

                for (int i = 0; i < m_collisionObjects.Count; i++ )
                {
                    CollisionObject colObj = m_collisionObjects[i];
                    if ((colObj.CollisionFlags & CollisionFlags.CF_DISABLE_VISUALIZE_OBJECT) == 0)
                    {
                        if (DebugDrawer != null && (DebugDrawer.DebugMode & DebugDrawModes.DBG_DrawWireframe) != 0)
                        {
                            btVector3 color = new btVector3(1, 1, 1);
                            switch (colObj.ActivationState)
                            {
                                case ActivationStateFlags.ACTIVE_TAG:
                                    color = new btVector3((1), (1), (1)); break;
                                case ActivationStateFlags.ISLAND_SLEEPING:
                                    color = new btVector3((0), (1), (0)); break;
                                case ActivationStateFlags.WANTS_DEACTIVATION:
                                    color = new btVector3((0), (1), (1)); break;
                                case ActivationStateFlags.DISABLE_DEACTIVATION:
                                    color = new btVector3((1), (0), (0)); break;
                                case ActivationStateFlags.DISABLE_SIMULATION:
                                    color = new btVector3((1), (1), (0)); break;
                                default:
                                    {
                                        color = new btVector3((0), (0), (1));
                                    }
                                    break;
                            };

                            debugDrawObject(ref colObj.WorldTransform, colObj.CollisionShape,ref color);
                        }
                        if (m_debugDrawer != null && (m_debugDrawer.DebugMode & DebugDrawModes.DBG_DrawAabb) != 0)
                        {
                            btVector3 minAabb, maxAabb;
                            btVector3 colorvec = new btVector3(1, 0, 0);
                            colObj.CollisionShape.getAabb(colObj.WorldTransform, out minAabb, out maxAabb);
                            m_debugDrawer.drawAabb(ref minAabb,ref maxAabb,ref colorvec);
                        }
                    }

                }
            }
        }
        public virtual void debugDrawObject(ref btTransform worldTransform, CollisionShape shape,ref btVector3 color)
        {
            // Draw a small simplex at the center of the object
            {
                btVector3 start = worldTransform.Origin;
                btVector3 temp1, temp2, temp3, temp4;
                //DebugDrawer.drawLine(start, start + btMatrix3x3.Multiply(worldTransform.Basis, new btVector3(1, 0, 0)), new btVector3(1, 0, 0));
                //DebugDrawer.drawLine(start, start + btMatrix3x3.Multiply(worldTransform.Basis, new btVector3(0, 1, 0)), new btVector3(0, 1, 0));
                //DebugDrawer.drawLine(start, start + btMatrix3x3.Multiply(worldTransform.Basis, new btVector3(0, 0, 1)), new btVector3(0, 0, 1));
                temp1 = new btVector3(1, 0, 0);
                btMatrix3x3.Multiply(ref worldTransform.Basis, ref temp1, out temp2);
                btVector3.Add(ref start, ref temp2, out temp3);
                temp4 = new btVector3(1, 0, 0);
                DebugDrawer.drawLine(ref start,ref temp3,ref temp4);
                temp1 = new btVector3(0, 1, 0);
                btMatrix3x3.Multiply(ref worldTransform.Basis, ref temp1, out temp2);
                btVector3.Add(ref start, ref temp2, out temp3);
                temp4 = new btVector3(0, 1, 0);
                DebugDrawer.drawLine(ref start,ref temp3,ref temp4);
                temp1 = new btVector3(0, 0, 1);
                btMatrix3x3.Multiply(ref worldTransform.Basis, ref temp1, out temp2);
                btVector3.Add(ref start, ref temp2, out temp3);
                temp4 = new btVector3(0, 0, 1);
                DebugDrawer.drawLine(ref start,ref temp3,ref temp4);

            }

            if (shape.ShapeType == BroadphaseNativeTypes.COMPOUND_SHAPE_PROXYTYPE)
            {
                throw new NotImplementedException();
#if false//未実装
		        const btCompoundShape* compoundShape = static_cast<const btCompoundShape*>(shape);
		        for (int i=compoundShape->getNumChildShapes()-1;i>=0;i--)
		        {
			        btTransform childTrans = compoundShape->getChildTransform(i);
			        const btCollisionShape* colShape = compoundShape->getChildShape(i);
			        debugDrawObject(worldTransform*childTrans,colShape,color);
		        }
#endif

            }
            else
            {
                switch (shape.ShapeType)
                {

                    case BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE:
                        {
                            BoxShape boxShape = (BoxShape)(shape);
                            btVector3 halfExtents = boxShape.HalfExtentsWithMargin;
                            btVector3 temp;
                            btVector3.Minus(ref halfExtents, out temp);
                            DebugDrawer.drawBox(ref temp, ref halfExtents,ref worldTransform,ref color);
                            break;
                        }

                    case BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE:
                        {
                            SphereShape sphereShape = (SphereShape)(shape);
                            float radius = sphereShape.Margin;//radius doesn't include the margin, so draw with margin

                            DebugDrawer.drawSphere(radius,ref worldTransform,ref color);
                            break;
                        }
                    case BroadphaseNativeTypes.MULTI_SPHERE_SHAPE_PROXYTYPE:
                        {
                            throw new NotImplementedException();
#if false
				        const btMultiSphereShape* multiSphereShape = static_cast<const btMultiSphereShape*>(shape);

				        btTransform childTransform;
				        childTransform.setIdentity();

				        for (int i = multiSphereShape->getSphereCount()-1; i>=0;i--)
				        {
					        childTransform.setOrigin(multiSphereShape->getSpherePosition(i));
					        DebugDrawer.drawSphere(multiSphereShape->getSphereRadius(i), worldTransform*childTransform, color);
				        }

				        break;
#endif
                        }
                    case BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE:
                        {
                            CapsuleShape capsuleShape = (CapsuleShape)(shape);

                            float radius = capsuleShape.Radius;
                            float halfHeight = capsuleShape.HalfHeight;

                            int upAxis = capsuleShape.UpAxis;


                            btVector3 capStart = btVector3.Zero;
                            capStart[upAxis] = -halfHeight;

                            btVector3 capEnd = btVector3.Zero;
                            capEnd[upAxis] = halfHeight;

                            // Draw the ends
                            {

                                btTransform childTransform = worldTransform;
                                childTransform.Origin = worldTransform * capStart;
                                DebugDrawer.drawSphere(radius,ref childTransform,ref color);
                            }

                            {
                                btTransform childTransform = worldTransform;
                                childTransform.Origin = worldTransform * capEnd;
                                DebugDrawer.drawSphere(radius, ref childTransform,ref color);
                            }

                            // Draw some additional lines
                            btVector3 start = worldTransform.Origin;


                            capStart[(upAxis + 1) % 3] = radius;
                            capEnd[(upAxis + 1) % 3] = radius;
                            #region DebugDrawer.drawLine(start + btMatrix3x3.Multiply( worldTransform.Basis , capStart), start + btMatrix3x3.Multiply(worldTransform.Basis , capEnd), color);
                            {
                                btVector3 capStart2, capEnd2, start2,end2;
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capStart, out capStart2);
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capEnd, out capEnd2);
                                btVector3.Add(ref start, ref capStart2, out start2);
                                btVector3.Add(ref start, ref capEnd2, out end2);
                                DebugDrawer.drawLine(ref start2,ref end2,ref color);
                            }
                            #endregion
                            capStart[(upAxis + 1) % 3] = -radius;
                            capEnd[(upAxis + 1) % 3] = -radius;
                            #region DebugDrawer.drawLine(start + btMatrix3x3.Multiply(worldTransform.Basis, capStart), start + btMatrix3x3.Multiply(worldTransform.Basis, capEnd), color);
                            {
                                btVector3 capStart2, capEnd2,start2,end2;
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capStart, out capStart2);
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capEnd, out capEnd2);
                                btVector3.Add(ref start, ref capStart2, out start2);
                                btVector3.Add(ref start, ref capEnd2, out end2);
                                DebugDrawer.drawLine(ref start2, ref end2, ref color);
                            }

                            #endregion
                            capStart[(upAxis + 1) % 3] = 0f;
                            capEnd[(upAxis + 1) % 3] = 0f;

                            capStart[(upAxis + 2) % 3] = radius;
                            capEnd[(upAxis + 2) % 3] = radius;
                            #region DebugDrawer.drawLine(start + btMatrix3x3.Multiply(worldTransform.Basis, capStart), start + btMatrix3x3.Multiply(worldTransform.Basis, capEnd), color);
                            {
                                btVector3 capStart2, capEnd2, start2, end2;
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capStart, out capStart2);
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capEnd, out capEnd2);
                                btVector3.Add(ref start, ref capStart2, out start2);
                                btVector3.Add(ref start, ref capEnd2, out end2);
                                DebugDrawer.drawLine(ref start2, ref end2, ref color);
                            }
                            #endregion
                            capStart[(upAxis + 2) % 3] = -radius;
                            capEnd[(upAxis + 2) % 3] = -radius;
                            #region DebugDrawer.drawLine(start + btMatrix3x3.Multiply(worldTransform.Basis, capStart), start + btMatrix3x3.Multiply(worldTransform.Basis, capEnd), color);
                            {
                                btVector3 capStart2, capEnd2, start2, end2;
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capStart, out capStart2);
                                btMatrix3x3.Multiply(ref worldTransform.Basis, ref capEnd, out capEnd2);
                                btVector3.Add(ref start, ref capStart2, out start2);
                                btVector3.Add(ref start, ref capEnd2, out end2);
                                DebugDrawer.drawLine(ref start2, ref end2, ref color);
                            }
                            #endregion
                            break;
                        }
                    case BroadphaseNativeTypes.CONE_SHAPE_PROXYTYPE:
                        {
                            throw new NotImplementedException();
#if false
				        const btConeShape* coneShape = static_cast<const btConeShape*>(shape);
				        btScalar radius = coneShape->getRadius();//+coneShape->getMargin();
				        btScalar height = coneShape->getHeight();//+coneShape->getMargin();
				        btVector3 start = worldTransform.Origin;

				        int upAxis= coneShape->getConeUpIndex();


				        btVector3	offsetHeight(0,0,0);
				        offsetHeight[upAxis] = height * btScalar(0.5);
				        btVector3	offsetRadius(0,0,0);
				        offsetRadius[(upAxis+1)%3] = radius;
				        btVector3	offset2Radius(0,0,0);
				        offset2Radius[(upAxis+2)%3] = radius;

				        DebugDrawer.drawLine(start+worldTransform.Basis * (offsetHeight),start+worldTransform.Basis * (-offsetHeight+offsetRadius),color);
				        DebugDrawer.drawLine(start+worldTransform.Basis * (offsetHeight),start+worldTransform.Basis * (-offsetHeight-offsetRadius),color);
				        DebugDrawer.drawLine(start+worldTransform.Basis * (offsetHeight),start+worldTransform.Basis * (-offsetHeight+offset2Radius),color);
				        DebugDrawer.drawLine(start+worldTransform.Basis * (offsetHeight),start+worldTransform.Basis * (-offsetHeight-offset2Radius),color);



				        break;
#endif
                        }
                    case BroadphaseNativeTypes.CYLINDER_SHAPE_PROXYTYPE:
                        {
                            throw new NotImplementedException();
#if false
				        const btCylinderShape* cylinder = static_cast<const btCylinderShape*>(shape);
				        int upAxis = cylinder->getUpAxis();
				        btScalar radius = cylinder->getRadius();
				        btScalar halfHeight = cylinder->getHalfExtentsWithMargin()[upAxis];
				        btVector3 start = worldTransform.Origin;
				        btVector3	offsetHeight(0,0,0);
				        offsetHeight[upAxis] = halfHeight;
				        btVector3	offsetRadius(0,0,0);
				        offsetRadius[(upAxis+1)%3] = radius;
				        DebugDrawer.drawLine(start+worldTransform.Basis * (offsetHeight+offsetRadius),start+worldTransform.Basis * (-offsetHeight+offsetRadius),color);
				        DebugDrawer.drawLine(start+worldTransform.Basis * (offsetHeight-offsetRadius),start+worldTransform.Basis * (-offsetHeight-offsetRadius),color);
				        break;
#endif
                        }

                    case BroadphaseNativeTypes.STATIC_PLANE_PROXYTYPE:
                        {
                            StaticPlaneShape staticPlaneShape = (StaticPlaneShape)(shape);
                            float planeConst = staticPlaneShape.PlaneConstant;
                            btVector3 planeNormal = staticPlaneShape.PlaneNormal;
                            btVector3 planeOrigin = planeNormal * planeConst;
                            btVector3 vec0, vec1;
                            btVector3.PlaneSpace1(ref planeNormal, out vec0, out vec1);
                            float vecLen = 100f;
                            btVector3 pt0;// = planeOrigin + vec0 * vecLen;
                            {
                                btVector3 temp;
                                btVector3.Multiply(ref vec0, vecLen, out temp);
                                btVector3.Add(ref planeOrigin, ref temp, out pt0);
                            }
                            btVector3 pt1;// = planeOrigin - vec0 * vecLen;
                            {
                                btVector3 temp;
                                btVector3.Multiply(ref vec0, vecLen, out temp);
                                btVector3.Subtract(ref planeOrigin, ref temp, out pt1);
                            }
                            btVector3 pt2;// = planeOrigin + vec1 * vecLen;
                            {
                                btVector3 temp;
                                btVector3.Multiply(ref vec1, vecLen, out temp);
                                btVector3.Add(ref planeOrigin, ref temp, out pt2);
                            }
                            btVector3 pt3 = planeOrigin - vec1 * vecLen;
                            #region DebugDrawer.drawLine(worldTransform * pt0, worldTransform * pt1, color);
                            {
                                btVector3 temp1, temp2;
                                btTransform.Multiply(ref worldTransform, ref pt0, out temp1);
                                btTransform.Multiply(ref worldTransform, ref pt1, out temp2);
                                DebugDrawer.drawLine(ref temp1, ref temp2, ref color);
                            }
                            #endregion
                            #region DebugDrawer.drawLine(worldTransform * pt2, worldTransform * pt3, color);
                            {
                                btVector3 temp1, temp2;
                                btTransform.Multiply(ref worldTransform, ref pt2, out temp1);
                                btTransform.Multiply(ref worldTransform, ref pt3, out temp2);
                                DebugDrawer.drawLine(ref temp1, ref temp2, ref color);
                            }
                            #endregion
                            break;

                        }
                    default:
                        {

                            if (shape.isConcave)
                            {
                                throw new NotImplementedException();
#if false
					            ConcaveShape concaveMesh = (ConcaveShape) shape;

					            ///@todo pass camera, for some culling? no -> we are not a graphics lib
					            btVector3 aabbMax=new btVector3(BulletGlobal.BT_LARGE_FLOAT,BulletGlobal.BT_LARGE_FLOAT,BulletGlobal.BT_LARGE_FLOAT);
					            btVector3 aabbMin=new btVector3(-BulletGlobal.BT_LARGE_FLOAT,-BulletGlobal.BT_LARGE_FLOAT,-BulletGlobal.BT_LARGE_FLOAT);

					            DebugDrawcallback drawCallback(getDebugDrawer(),worldTransform,color);
					            concaveMesh->processAllTriangles(&drawCallback,aabbMin,aabbMax);
#endif
                            }

                            if (shape.ShapeType == BroadphaseNativeTypes.CONVEX_TRIANGLEMESH_SHAPE_PROXYTYPE)
                            {
                                throw new NotImplementedException();
#if false
					            btConvexTriangleMeshShape* convexMesh = (btConvexTriangleMeshShape*) shape;
					            //todo: pass camera for some culling			
					            btVector3 aabbMax(btScalar(BT_LARGE_FLOAT),btScalar(BT_LARGE_FLOAT),btScalar(BT_LARGE_FLOAT));
					            btVector3 aabbMin(btScalar(-BT_LARGE_FLOAT),btScalar(-BT_LARGE_FLOAT),btScalar(-BT_LARGE_FLOAT));
					            //DebugDrawcallback drawCallback;
					            DebugDrawcallback drawCallback(getDebugDrawer(),worldTransform,color);
					            convexMesh->getMeshInterface()->InternalProcessAllTriangles(&drawCallback,aabbMin,aabbMax);
#endif
                            }


                            /// for polyhedral shapes
                            if (shape.isPolyhedral)
                            {
                                throw new NotImplementedException();
#if false
					            btPolyhedralConvexShape* polyshape = (btPolyhedralConvexShape*) shape;

					            int i;
					            for (i=0;i<polyshape->getNumEdges();i++)
					            {
						            btVector3 a,b;
						            polyshape->getEdge(i,a,b);
						            btVector3 wa = worldTransform * a;
						            btVector3 wb = worldTransform * b;
						            DebugDrawer.drawLine(wa,wb,color);

					            }
#endif

                            }
                            break;
                        }
                }
            }
        }
        //ここに入るクラスは別ファイルに移す

        public int NumCollisionObjects { get { return m_collisionObjects.Count; } }

#if false//未移植
        /// rayTest performs a raycast on all objects in the btCollisionWorld, and calls the resultCallback
	    /// This allows for several queries: first hit, all hits, any hit, dependent on the value returned by the callback.
	    virtual void rayTest(const btVector3& rayFromWorld, const btVector3& rayToWorld, RayResultCallback& resultCallback) const; 

	    /// convexTest performs a swept convex cast on all objects in the btCollisionWorld, and calls the resultCallback
	    /// This allows for several queries: first hit, all hits, any hit, dependent on the value return by the callback.
	    void    convexSweepTest (const btConvexShape* castShape, const btTransform& from, const btTransform& to, ConvexResultCallback& resultCallback,  btScalar allowedCcdPenetration = btScalar(0.)) const;

	    ///contactTest performs a discrete collision test between colObj against all objects in the btCollisionWorld, and calls the resultCallback.
	    ///it reports one or more contact points for every overlapping object (including the one with deepest penetration)
	    void	contactTest(btCollisionObject* colObj, ContactResultCallback& resultCallback);

	    ///contactTest performs a discrete collision test between two collision objects and calls the resultCallback if overlap if detected.
	    ///it reports one or more contact points (including the one with deepest penetration)
	    void	contactPairTest(btCollisionObject* colObjA, btCollisionObject* colObjB, ContactResultCallback& resultCallback);


	    /// rayTestSingle performs a raycast call and calls the resultCallback. It is used internally by rayTest.
	    /// In a future implementation, we consider moving the ray test as a virtual method in btCollisionShape.
	    /// This allows more customization.
	    static void	rayTestSingle(const btTransform& rayFromTrans,const btTransform& rayToTrans,
					      btCollisionObject* collisionObject,
					      const btCollisionShape* collisionShape,
					      const btTransform& colObjWorldTransform,
					      RayResultCallback& resultCallback);

	    /// objectQuerySingle performs a collision detection query and calls the resultCallback. It is used internally by rayTest.
	    static void	objectQuerySingle(const btConvexShape* castShape, const btTransform& rayFromTrans,const btTransform& rayToTrans,
					      btCollisionObject* collisionObject,
					      const btCollisionShape* collisionShape,
					      const btTransform& colObjWorldTransform,
					      ConvexResultCallback& resultCallback, btScalar	allowedPenetration);
#endif

        public virtual void addCollisionObject(CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask)
        {
            Debug.Assert(collisionObject != null);

            //check that the object isn't already added
            Debug.Assert(m_collisionObjects.IndexOf(collisionObject) == -1);

            m_collisionObjects.Add(collisionObject);

            //calculate new AABB
            btTransform trans = collisionObject.WorldTransform;

            btVector3 minAabb;
            btVector3 maxAabb;
            collisionObject.CollisionShape.getAabb(trans, out minAabb, out maxAabb);

            BroadphaseNativeTypes type = collisionObject.CollisionShape.ShapeType;
            collisionObject.BroadphaseHandle = m_broadphasePairCache.createProxy(
                ref minAabb,
                ref maxAabb,
                type,
                collisionObject,
                collisionFilterGroup,
                collisionFilterMask,
                m_dispatcher1, null
                );
        }
        public List<CollisionObject> CollisionObjects { get { return m_collisionObjects; } }


        public virtual void removeCollisionObject(CollisionObject collisionObject)
        {
            {

                BroadphaseProxy bp = collisionObject.BroadphaseHandle;
                if (bp != null)
                {
                    //
                    // only clear the cached algorithms
                    //
                    Broadphase.OverlappingPairCache.cleanProxyFromPairs(bp, m_dispatcher1);
                    Broadphase.destroyProxy(bp, m_dispatcher1);
                    collisionObject.BroadphaseHandle = null;
                }
            }


            //swapremove
            m_collisionObjects.Remove(collisionObject);

        }


        public virtual void performDiscreteCollisionDetection()
        {
            BulletGlobal.StartProfile("0-1 performDiscreteCollisionDetection");
            updateAabbs();
            BulletGlobal.StartProfile("0-1-1 calculateOverlappingPairs");
            m_broadphasePairCache.calculateOverlappingPairs(m_dispatcher1);
            BulletGlobal.EndProfile("0-1-1 calculateOverlappingPairs");
            BulletGlobal.StartProfile("0-1-2 dispatchAllCollisionPairs");
            if (Dispatcher != null)
                Dispatcher.dispatchAllCollisionPairs(m_broadphasePairCache.OverlappingPairCache, DispatchInfo, m_dispatcher1);
            BulletGlobal.EndProfile("0-1-2 dispatchAllCollisionPairs");
            BulletGlobal.EndProfile("0-1 performDiscreteCollisionDetection");
        }
        public DispatcherInfo DispatchInfo { get { return m_dispatchInfo; } }

        public bool ForceUpdateAllAabbs { get { return m_forceUpdateAllAabbs; } set { m_forceUpdateAllAabbs = value; } }

#if false
        ///Preliminary serialization test for Bullet 2.76. Loading those files requires a separate parser (Bullet/Demos/SerializeDemo)
        virtual void serialize(btSerializer* serializer);
#endif



    }
}
