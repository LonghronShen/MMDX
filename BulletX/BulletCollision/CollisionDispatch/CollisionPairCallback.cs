using BulletX.BulletCollision.BroadphaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    ///interface for iterating all overlapping collision pairs, no matter how those pairs are stored (array, set, map etc)
    ///this is useful for the collision dispatcher.
    class CollisionPairCallback : IOverlapCallback
    {
        DispatcherInfo m_dispatchInfo;
        CollisionDispatcher m_dispatcher;

        public void Constructor(DispatcherInfo dispatchInfo, CollisionDispatcher dispatcher)
        {
            m_dispatchInfo = dispatchInfo;
            m_dispatcher = dispatcher;
        }

        /*btCollisionPairCallback& operator=(btCollisionPairCallback& other)
        {
            m_dispatchInfo = other.m_dispatchInfo;
            m_dispatcher = other.m_dispatcher;
            return *this;
        }
        */

        public virtual bool processOverlap(BroadphasePair pair)
        {
            m_dispatcher.NearCallback(pair, m_dispatcher, m_dispatchInfo);

            return false;
        }
    }
}
