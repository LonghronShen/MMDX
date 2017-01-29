using System.Collections.Generic;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    class NullPairCache : IOverlappingPairCache
    {
        List<BroadphasePair> m_overlappingPairArray = new List<BroadphasePair>();

        public NullPairCache() { }

        public virtual IList<BroadphasePair> OverlappingPairArrayPtr { get { return m_overlappingPairArray; } }
        public List<BroadphasePair> OverlappingPairArray { get { return m_overlappingPairArray; } }
        public virtual void cleanOverlappingPair(BroadphasePair pair, IDispatcher dispatcher)
        {
        }
        public virtual int NumOverlappingPairs
        {
            get { return 0; }
        }
        public virtual void cleanProxyFromPairs(BroadphaseProxy proxy, IDispatcher dispatcher)
        {
        }
        public IOverlapFilterCallback OverlapFilterCallback { set { } }
        public virtual void processAllOverlappingPairs(IOverlapCallback callback, IDispatcher dispatcher)
        {
        }
        public virtual BroadphasePair findPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            return null;
        }
        public bool hasDeferredRemoval()
        {
            return true;
        }
        public IOverlappingPairCallback InternalGhostPairCallback { set { } }
        public BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            return null;
        }
        public object removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, IDispatcher dispatcher)
        {
            return null;
        }
        public void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy0, IDispatcher dispatcher)
        {
        }
        public virtual void	sortOverlappingPairs(IDispatcher dispatcher)
	    {
        }
    }
}
