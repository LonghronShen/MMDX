using System;
using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class ConvexConcaveCollisionAlgorithm : ActivatingCollisionAlgorithm
    {
        public ConvexConcaveCollisionAlgorithm()
        {
            throw new NotImplementedException();
        }

        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            public CreateFunc() { }

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo info, CollisionObject body0, CollisionObject body1)
            {
                throw new NotImplementedException();
            }
        }
        public class SwappedCreateFunc : CollisionAlgorithmCreateFunc
        {
            public SwappedCreateFunc() { }

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo info, CollisionObject body0, CollisionObject body1)
            {
                throw new NotImplementedException();
            }
        }

        public override void free()
        {
            throw new NotImplementedException();
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            throw new NotImplementedException();
        }

        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            throw new NotImplementedException();
        }

        public override void getAllContactManifolds(List<BulletX.BulletCollision.NarrowPhaseCollision.PersistentManifold> manifoldArray)
        {
            throw new NotImplementedException();
        }
    }
}
