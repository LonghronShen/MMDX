
namespace BulletX.BulletCollision.CollisionDispatch
{
    public struct DefaultCollisionConstructionInfo
    {
        //StackAlloc*		m_stackAlloc;
        //public PoolAllocator m_persistentManifoldPool;
        //public PoolAllocator m_collisionAlgorithmPool;
        public int m_defaultMaxPersistentManifoldPoolSize;
        public int m_defaultMaxCollisionAlgorithmPoolSize;
        public int m_customCollisionAlgorithmMaxElementSize;
        public int m_defaultStackAllocatorSize;
        public bool m_useEpaPenetrationAlgorithm;

        public static readonly DefaultCollisionConstructionInfo Default
            = new DefaultCollisionConstructionInfo
            {
                //m_stackAlloc=0,
                //m_persistentManifoldPool = null,
                //m_collisionAlgorithmPool = null,
                m_defaultMaxPersistentManifoldPoolSize = 4096,
                m_defaultMaxCollisionAlgorithmPoolSize = 4096,
                m_customCollisionAlgorithmMaxElementSize = 0,
                m_defaultStackAllocatorSize = 0,
                m_useEpaPenetrationAlgorithm = true
            };

    }
}
