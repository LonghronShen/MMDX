using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class EmptyAlgorithm : CollisionAlgorithm
    {
        //オブジェクトプール
        static Queue<EmptyAlgorithm> ObjPool
            = new Queue<EmptyAlgorithm>();
        static EmptyAlgorithm AllocFromPool(CollisionAlgorithmConstructionInfo ci)
        {
            EmptyAlgorithm result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new EmptyAlgorithm();
            result.Constructor(ci);
            return result;
        }
        public override void free()
        {
            ObjPool.Enqueue(this);
        }


        EmptyAlgorithm(){ }
        //オブジェクトプールを使うので初期化処理の代わりを……
        protected new void Constructor(CollisionAlgorithmConstructionInfo ci)
        {
            base.Constructor(ci);
        }
        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        { }
        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            return 1f;
        }
        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
        }
        
        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            public CreateFunc() { }

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo info, CollisionObject body0, CollisionObject body1)
            {
                return AllocFromPool(info);
            }
        }
    }
}
