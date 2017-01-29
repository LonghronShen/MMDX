
namespace BulletX.BulletCollision.BroadphaseCollision
{
    public interface IOverlappingPairCallback
    {
        BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);

        object removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, IDispatcher dispatcher);
        void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy0, IDispatcher dispatcher);
    }
}
