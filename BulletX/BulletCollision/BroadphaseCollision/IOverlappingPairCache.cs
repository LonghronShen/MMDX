using System.Collections.Generic;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public interface IOverlappingPairCache : IOverlappingPairCallback
    {
        IList<BroadphasePair> OverlappingPairArrayPtr { get; }
        List<BroadphasePair> OverlappingPairArray { get; }
        void cleanOverlappingPair(BroadphasePair pair, IDispatcher dispatcher);
        int NumOverlappingPairs { get; }
        void cleanProxyFromPairs(BroadphaseProxy proxy, IDispatcher dispatcher);
        IOverlapFilterCallback OverlapFilterCallback { set; }
        void processAllOverlappingPairs(IOverlapCallback callback, IDispatcher dispatcher);
#if false
        BroadphasePair findPair(BroadphaseProxy proxy0, btBroadphaseProxy proxy1);
#endif
        bool hasDeferredRemoval();
        IOverlappingPairCallback InternalGhostPairCallback { set; }
#if false
	    virtual void	sortOverlappingPairs(btDispatcher* dispatcher) = 0;
#endif
    }
}
