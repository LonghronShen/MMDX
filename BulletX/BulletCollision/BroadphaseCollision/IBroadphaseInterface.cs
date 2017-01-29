using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public interface IBroadphaseInterface
    {
        BroadphaseProxy createProxy(ref btVector3 aabbMin,ref btVector3 aabbMax, BroadphaseNativeTypes shapeType, object userPtr, short collisionFilterGroup, short collisionFilterMask, IDispatcher dispatcher, object multiSapProxy);
        void destroyProxy(BroadphaseProxy proxy, IDispatcher dispatcher);
        void setAabb(BroadphaseProxy proxy, ref btVector3 aabbMin,ref btVector3 aabbMax, IDispatcher dispatcher);
        void getAabb(BroadphaseProxy proxy, out btVector3 aabbMin, out btVector3 aabbMax);

        void	rayTest(ref btVector3 rayFrom,ref btVector3 rayTo, BroadphaseRayCallback rayCallback,ref btVector3 aabbMin,ref btVector3 aabbMax);

	    void	aabbTest(ref btVector3 aabbMin,ref btVector3 aabbMax, IBroadphaseAabbCallback callback);
	    ///calculateOverlappingPairs is optional: incremental algorithms (sweep and prune) might do it during the set aabb
	    void calculateOverlappingPairs(IDispatcher m_dispatcher1);
        IOverlappingPairCache OverlappingPairCache { get; }

        ///getAabb returns the axis aligned bounding box in the 'global' coordinate frame
	    ///will add some transform later
	    void getBroadphaseAabb(out btVector3 aabbMin,out btVector3 aabbMax);

	    ///reset broadphase internal structures, to ensure determinism/reproducability
	    void resetPool(IDispatcher dispatcher);

	    void	printStats();
    }
}
