using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public struct CollisionAlgorithmConstructionInfo
    {
        public IDispatcher m_dispatcher1;
        public PersistentManifold m_manifold;
        public CollisionAlgorithmConstructionInfo(IDispatcher dispatcher)
        {
            m_dispatcher1 = dispatcher;
            m_manifold = null;
        }

    }
}
