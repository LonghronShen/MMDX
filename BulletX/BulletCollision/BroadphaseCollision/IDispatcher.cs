using System.Collections.Generic;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public interface IDispatcher
    {
        CollisionAlgorithm findAlgorithm(CollisionObject body0, CollisionObject body1);

        PersistentManifold getNewManifold(CollisionObject body0, CollisionObject body1);

        void releaseManifold(PersistentManifold manifold);

        void clearManifold(PersistentManifold manifold);

        bool needsCollision(CollisionObject body0, CollisionObject body1);

        bool needsResponse(CollisionObject body0, CollisionObject body1);

        void dispatchAllCollisionPairs(IOverlappingPairCache pairCache, DispatcherInfo dispatchInfo, IDispatcher dispatcher);

        int NumManifolds { get; }

        PersistentManifold getManifoldByIndexInternal(int index);

        IList<PersistentManifold> InternalManifoldPointer { get; }

        void freeCollisionAlgorithm(CollisionAlgorithm ptr);



    }
}
