using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public class DefaultCollisionConfiguration : ICollisionConfiguration
    {
        //メンバ変数
#if false//未使用らしきメンバ変数。初期化コードもコメントアウト中
        //int	m_persistentManifoldPoolSize;
	
        //btStackAlloc*	m_stackAlloc;
        //bool	m_ownsStackAllocator;

        //btPoolAllocator*	m_persistentManifoldPool;
        //bool	m_ownsPersistentManifoldPool;


        //btPoolAllocator*	m_collisionAlgorithmPool;
        //bool	m_ownsCollisionAlgorithmPool;
#endif
	    //default simplex/penetration depth solvers
	    VoronoiSimplexSolver	        m_simplexSolver;
	    IConvexPenetrationDepthSolver	m_pdSolver;
    	
	    //default CreationFunctions, filling the m_doubleDispatch table
	    CollisionAlgorithmCreateFunc	m_convexConvexCreateFunc;
	    CollisionAlgorithmCreateFunc	m_convexConcaveCreateFunc;
	    CollisionAlgorithmCreateFunc	m_swappedConvexConcaveCreateFunc;
	    CollisionAlgorithmCreateFunc	m_compoundCreateFunc;
	    CollisionAlgorithmCreateFunc	m_swappedCompoundCreateFunc;
	    CollisionAlgorithmCreateFunc    m_emptyCreateFunc;
	    CollisionAlgorithmCreateFunc    m_sphereSphereCF;
#if USE_BUGGY_SPHERE_BOX_ALGORITHM
	    CollisionAlgorithmCreateFunc* m_sphereBoxCF;
	    CollisionAlgorithmCreateFunc* m_boxSphereCF;
#endif //USE_BUGGY_SPHERE_BOX_ALGORITHM

        CollisionAlgorithmCreateFunc    m_boxBoxCF;
        CollisionAlgorithmCreateFunc	m_sphereTriangleCF;
        CollisionAlgorithmCreateFunc	m_triangleSphereCF;
        CollisionAlgorithmCreateFunc	m_planeConvexCF;
        CollisionAlgorithmCreateFunc	m_convexPlaneCF;
	

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DefaultCollisionConfiguration()
        {
            Constructor(DefaultCollisionConstructionInfo.Default);
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="constructionInfo">初期化用構造体</param>
        public DefaultCollisionConfiguration(DefaultCollisionConstructionInfo constructionInfo)
        {
            Constructor(constructionInfo);
        }
        /// <summary>
        /// 初期化関数
        /// </summary>
        /// <param name="constructionInfo">初期化用構造体</param>
        private void Constructor(DefaultCollisionConstructionInfo constructionInfo)
        {

            m_simplexSolver = new VoronoiSimplexSolver();

            if (constructionInfo.m_useEpaPenetrationAlgorithm)
            {
                m_pdSolver = new GjkEpaPenetrationDepthSolver();
            }
            else
            {
                m_pdSolver = new MinkowskiPenetrationDepthSolver();
            }

            //default CreationFunctions, filling the m_doubleDispatch table
            m_convexConvexCreateFunc = new ConvexConvexAlgorithm.CreateFunc(m_simplexSolver, m_pdSolver);
            m_convexConcaveCreateFunc = new ConvexConcaveCollisionAlgorithm.CreateFunc();
            m_swappedConvexConcaveCreateFunc = new ConvexConcaveCollisionAlgorithm.SwappedCreateFunc();
            m_compoundCreateFunc = new CompoundCollisionAlgorithm.CreateFunc();
            m_swappedCompoundCreateFunc = new CompoundCollisionAlgorithm.SwappedCreateFunc();
            m_emptyCreateFunc = new EmptyAlgorithm.CreateFunc();

            m_sphereSphereCF = new SphereSphereCollisionAlgorithm.CreateFunc();
#if USE_BUGGY_SPHERE_BOX_ALGORITHM
            //この中は移植作業を行ってない。
	        m_sphereBoxCF = new SphereBoxCollisionAlgorithm.CreateFunc();
	        m_boxSphereCF = new SphereBoxCollisionAlgorithm.CreateFunc();
	        m_boxSphereCF.m_swapped = true;
#endif //USE_BUGGY_SPHERE_BOX_ALGORITHM

            m_sphereTriangleCF = new SphereTriangleCollisionAlgorithm.CreateFunc();
            m_triangleSphereCF = new SphereTriangleCollisionAlgorithm.CreateFunc();
            m_triangleSphereCF.m_swapped = true;

            m_boxBoxCF = new BoxBoxCollisionAlgorithm.CreateFunc();

            //convex versus plane
            m_convexPlaneCF = new ConvexPlaneCollisionAlgorithm.CreateFunc();
            m_planeConvexCF = new ConvexPlaneCollisionAlgorithm.CreateFunc();
            m_planeConvexCF.m_swapped = true;
#if false//未使用らしきメンバ変数の初期化作業
	        ///calculate maximum element size, big enough to fit any collision algorithm in the memory pool
	        int maxSize = sizeof(btConvexConvexAlgorithm);
	        int maxSize2 = sizeof(btConvexConcaveCollisionAlgorithm);
	        int maxSize3 = sizeof(btCompoundCollisionAlgorithm);
	        int sl = sizeof(btConvexSeparatingDistanceUtil);
	        sl = sizeof(btGjkPairDetector);
	        int	collisionAlgorithmMaxElementSize = btMax(maxSize,constructionInfo.m_customCollisionAlgorithmMaxElementSize);
	        collisionAlgorithmMaxElementSize = btMax(collisionAlgorithmMaxElementSize,maxSize2);
	        collisionAlgorithmMaxElementSize = btMax(collisionAlgorithmMaxElementSize,maxSize3);

	        if (constructionInfo.m_stackAlloc)
	        {
		        m_ownsStackAllocator = false;
		        this->m_stackAlloc = constructionInfo.m_stackAlloc;
	        } else
	        {
		        m_ownsStackAllocator = true;
		        void* mem = btAlignedAlloc(sizeof(btStackAlloc),16);
		        m_stackAlloc = new(mem)btStackAlloc(constructionInfo.m_defaultStackAllocatorSize);
	        }
        		
	        if (constructionInfo.m_persistentManifoldPool)
	        {
		        m_ownsPersistentManifoldPool = false;
		        m_persistentManifoldPool = constructionInfo.m_persistentManifoldPool;
	        } else
	        {
		        m_ownsPersistentManifoldPool = true;
		        void* mem = btAlignedAlloc(sizeof(btPoolAllocator),16);
		        m_persistentManifoldPool = new (mem) btPoolAllocator(sizeof(btPersistentManifold),constructionInfo.m_defaultMaxPersistentManifoldPoolSize);
	        }
        	
	        if (constructionInfo.m_collisionAlgorithmPool)
	        {
		        m_ownsCollisionAlgorithmPool = false;
		        m_collisionAlgorithmPool = constructionInfo.m_collisionAlgorithmPool;
	        } else
	        {
		        m_ownsCollisionAlgorithmPool = true;
		        void* mem = btAlignedAlloc(sizeof(btPoolAllocator),16);
		        m_collisionAlgorithmPool = new(mem) btPoolAllocator(collisionAlgorithmMaxElementSize,constructionInfo.m_defaultMaxCollisionAlgorithmPoolSize);
	        }
#endif
        }
        #region ICollisionConfiguration メンバ

#if false//未使用らしきメンバ変数へのアクセッサ。初期化コードもコメントアウト中
        public PoolAllocator getCollisionAlgorithmPool()
        {
            throw new NotImplementedException();
        }

        public PoolAllocator getPersistentManifoldPool()
        {
            throw new NotImplementedException();
        }
#endif
        public CollisionAlgorithmCreateFunc getCollisionAlgorithmCreateFunc(BroadphaseNativeTypes proxyType0, BroadphaseNativeTypes proxyType1)
        {
            if ((proxyType0 == BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE))
            {
                return m_sphereSphereCF;
            }
#if USE_BUGGY_SPHERE_BOX_ALGORITHM
	        if ((proxyType0 == BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE) && (proxyType1==BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE))
	        {
		        return	m_sphereBoxCF;
	        }

	        if ((proxyType0 == BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE ) && (proxyType1==BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE))
	        {
		        return	m_boxSphereCF;
	        }
#endif //USE_BUGGY_SPHERE_BOX_ALGORITHM


            if ((proxyType0 == BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeTypes.TRIANGLE_SHAPE_PROXYTYPE))
            {
                return m_sphereTriangleCF;
            }

            if ((proxyType0 == BroadphaseNativeTypes.TRIANGLE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE))
            {
                return m_triangleSphereCF;
            }

            if ((proxyType0 == BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE))
            {
                return m_boxBoxCF;
            }

            if (BroadphaseProxy.isConvex(proxyType0) && (proxyType1 == BroadphaseNativeTypes.STATIC_PLANE_PROXYTYPE))
            {
                return m_convexPlaneCF;
            }

            if (BroadphaseProxy.isConvex(proxyType1) && (proxyType0 == BroadphaseNativeTypes.STATIC_PLANE_PROXYTYPE))
            {
                return m_planeConvexCF;
            }



            if (BroadphaseProxy.isConvex(proxyType0) && BroadphaseProxy.isConvex(proxyType1))
            {
                return m_convexConvexCreateFunc;
            }

            if (BroadphaseProxy.isConvex(proxyType0) && BroadphaseProxy.isConcave(proxyType1))
            {
                return m_convexConcaveCreateFunc;
            }

            if (BroadphaseProxy.isConvex(proxyType1) && BroadphaseProxy.isConcave(proxyType0))
            {
                return m_swappedConvexConcaveCreateFunc;
            }

            if (BroadphaseProxy.isCompound(proxyType0))
            {
                return m_compoundCreateFunc;
            }
            else
            {
                if (BroadphaseProxy.isCompound(proxyType1))
                {
                    return m_swappedCompoundCreateFunc;
                }
            }

            //failed to find an algorithm
            return m_emptyCreateFunc;
        }

        #endregion
    }
}
