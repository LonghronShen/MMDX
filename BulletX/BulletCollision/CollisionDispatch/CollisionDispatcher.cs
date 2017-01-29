using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public delegate void NearCallback(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo);
    public class CollisionDispatcher : IDispatcher
    {
        //メンバ変数
        DispatcherFlags m_dispatcherFlags;
        List<PersistentManifold> m_manifoldsPtr = new List<PersistentManifold>();
#if false//未使用
        btManifoldResult	m_defaultManifoldResult;
#endif
        NearCallback m_neerCallback;
#if false//未使用らしきメンバ変数。初期化コードもコメントアウト中
        PoolAllocator m_collisionAlgorithmPoolAllocator;
        PoolAllocator m_persistentManifoldPoolAllocator;
#endif
        CollisionAlgorithmCreateFunc[,] m_doubleDispatch;
        ICollisionConfiguration m_collisionConfiguration;

        public enum DispatcherFlags
        {
            CD_STATIC_STATIC_REPORTED = 1,
            CD_USE_RELATIVE_CONTACT_BREAKING_THRESHOLD = 2
        }
        public DispatcherFlags DispatcherFlag { get { return m_dispatcherFlags; } set { m_dispatcherFlags = 0; } }

        ///registerCollisionCreateFunc allows registration of custom/alternative collision create functions
        void registerCollisionCreateFunc(int proxyType0, int proxyType1, CollisionAlgorithmCreateFunc createFunc)
        {
            m_doubleDispatch[proxyType0, proxyType1] = createFunc;
        }

        public int NumManifolds { get { return m_manifoldsPtr.Count; } }
        public IList<PersistentManifold> InternalManifoldPointer { get { return m_manifoldsPtr; } }
        public PersistentManifold getManifoldByIndexInternal(int index)
        {
            return m_manifoldsPtr[index];
        }

        public CollisionDispatcher(ICollisionConfiguration collisionConfiguration)
        {
            m_dispatcherFlags = DispatcherFlags.CD_USE_RELATIVE_CONTACT_BREAKING_THRESHOLD;
            m_collisionConfiguration = collisionConfiguration;
            m_neerCallback = DefaultNearCallback;

#if false
            m_collisionAlgorithmPoolAllocator = collisionConfiguration.getCollisionAlgorithmPool();
            m_persistentManifoldPoolAllocator = collisionConfiguration.getPersistentManifoldPool();
#endif
            m_doubleDispatch = new CollisionAlgorithmCreateFunc[(int)BroadphaseNativeTypes.MAX_BROADPHASE_COLLISION_TYPES, (int)BroadphaseNativeTypes.MAX_BROADPHASE_COLLISION_TYPES];
            for (BroadphaseNativeTypes i = BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE; i < BroadphaseNativeTypes.MAX_BROADPHASE_COLLISION_TYPES; i++)
            {
                for (BroadphaseNativeTypes j = BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE; j < BroadphaseNativeTypes.MAX_BROADPHASE_COLLISION_TYPES; j++)
                {
                    m_doubleDispatch[(int)i, (int)j] = m_collisionConfiguration.getCollisionAlgorithmCreateFunc(i, j);
                    if (m_doubleDispatch[(int)i, (int)j] == null)
                        throw new BulletException();
                }
            }
        }
        public virtual PersistentManifold getNewManifold(CollisionObject body0, CollisionObject body1)
        {
            //オブジェクトプールを利用して実装する。
            float contactBreakingThreshold = (m_dispatcherFlags & DispatcherFlags.CD_USE_RELATIVE_CONTACT_BREAKING_THRESHOLD) != 0 ?
                (float)Math.Min(body0.CollisionShape.getContactBreakingThreshold(PersistentManifold.gContactBreakingThreshold), body1.CollisionShape.getContactBreakingThreshold(PersistentManifold.gContactBreakingThreshold))
                : PersistentManifold.gContactBreakingThreshold;
            
            float contactProcessingThreshold = (float)Math.Min(body0.ContactProcessingThreshold, body1.ContactProcessingThreshold);

            PersistentManifold manifold = PersistentManifold.allocate(body0, body1, 0, contactBreakingThreshold, contactProcessingThreshold);

            manifold.m_index1a = m_manifoldsPtr.Count;
            m_manifoldsPtr.Add(manifold);

            return manifold;
        }
        public virtual void releaseManifold(PersistentManifold manifold)
        {
            clearManifold(manifold);
            int findIndex = manifold.m_index1a;
            PersistentManifold temp = m_manifoldsPtr[m_manifoldsPtr.Count - 1];
            m_manifoldsPtr[m_manifoldsPtr.Count - 1] = m_manifoldsPtr[findIndex];
            m_manifoldsPtr[findIndex] = temp;
            m_manifoldsPtr[findIndex].m_index1a = findIndex;
            m_manifoldsPtr.RemoveAt(m_manifoldsPtr.Count - 1);
        }
        public virtual void clearManifold(PersistentManifold manifold)
        {
            manifold.clearManifold();
        }
        public CollisionAlgorithm findAlgorithm(CollisionObject body0, CollisionObject body1)//,PersistentManifold sharedManifold = 0)
        {
            CollisionAlgorithmConstructionInfo ci;

            ci.m_dispatcher1 = this;
            ci.m_manifold = null;
            CollisionAlgorithm algo = m_doubleDispatch[(int)body0.CollisionShape.ShapeType, (int)body1.CollisionShape.ShapeType].CreateCollisionAlgorithm(ci, body0, body1);

            return algo;
        }
        public virtual bool needsCollision(CollisionObject body0, CollisionObject body1)
        {
            Debug.Assert(body0 != null);
            Debug.Assert(body1 != null);

            bool needsCollision = true;

#if DEBUG
            if ((m_dispatcherFlags & DispatcherFlags.CD_STATIC_STATIC_REPORTED) != 0)
            {
                //broadphase filtering already deals with this
                if ((body0.isStaticObject || body0.isKinematicObject) &&
                    (body1.isStaticObject || body1.isKinematicObject))
                {
                    m_dispatcherFlags |= DispatcherFlags.CD_STATIC_STATIC_REPORTED;
                    Debug.WriteLine("warning btCollisionDispatcher::needsCollision: static-static collision!\n");
                }
            }
#endif //BT_DEBUG

            if ((!body0.isActive) && (!body1.isActive))
                needsCollision = false;
            else if (!body0.checkCollideWith(body1))
                needsCollision = false;

            return needsCollision;

        }
        public virtual bool needsResponse(CollisionObject body0, CollisionObject body1)
        {
            //here you can do filtering
            bool hasResponse =
                (body0.hasContactResponse && body1.hasContactResponse);
            //no response between two static/kinematic bodies:
            hasResponse = hasResponse &&
                ((!body0.isStaticOrKinematicObject) || (!body1.isStaticOrKinematicObject));
            return hasResponse;
        }
        CollisionPairCallback collisionCallback = new CollisionPairCallback();
        public virtual void dispatchAllCollisionPairs(IOverlappingPairCache pairCache, DispatcherInfo dispatchInfo, IDispatcher dispatcher)
        {
            collisionCallback.Constructor(dispatchInfo, this);

            pairCache.processAllOverlappingPairs(collisionCallback, dispatcher);


        }
        public NearCallback NearCallback { get { return m_neerCallback; } set { m_neerCallback = value; } }
        
        //by default, Bullet will use this near callback
        static void DefaultNearCallback(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo)
        {
            CollisionObject colObj0 = (CollisionObject)collisionPair.m_pProxy0.m_clientObject;
		    CollisionObject colObj1 = (CollisionObject)collisionPair.m_pProxy1.m_clientObject;

		    if (dispatcher.needsCollision(colObj0,colObj1))
		    {
			    //dispatcher will keep algorithms persistent in the collision pair
			    if (collisionPair.m_algorithm==null)
			    {
				    collisionPair.m_algorithm = dispatcher.findAlgorithm(colObj0,colObj1);
                }

			    if (collisionPair.m_algorithm!=null)
			    {
				    ManifoldResult contactPointResult=new ManifoldResult(colObj0,colObj1);

                    if (dispatchInfo.m_dispatchFunc == DispatchFunc.DISPATCH_DISCRETE)
				    {
					    //discrete collision detection query
					    collisionPair.m_algorithm.processCollision(colObj0,colObj1,dispatchInfo,ref contactPointResult);
				    } else
				    {
					    //continuous collision detection query, time of impact (toi)
					    float toi = collisionPair.m_algorithm.calculateTimeOfImpact(colObj0,colObj1,dispatchInfo,ref contactPointResult);
					    if (dispatchInfo.m_timeOfImpact > toi)
						    dispatchInfo.m_timeOfImpact = toi;

				    }
			    }
		    }
        }
        public virtual void freeCollisionAlgorithm(CollisionAlgorithm ptr)
        {
            ptr.free();
        }

        public ICollisionConfiguration CollisionConfiguration { get { return m_collisionConfiguration; } set { m_collisionConfiguration = value; } }

    }
}
