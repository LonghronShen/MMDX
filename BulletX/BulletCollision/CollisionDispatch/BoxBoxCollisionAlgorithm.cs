#define USE_PERSISTENT_CONTACTS
using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class BoxBoxCollisionAlgorithm : ActivatingCollisionAlgorithm
    {
        //オブジェクトプール
        static Queue<BoxBoxCollisionAlgorithm> ObjPool
            = new Queue<BoxBoxCollisionAlgorithm>(new BoxBoxCollisionAlgorithm[1] { new BoxBoxCollisionAlgorithm()});
        static BoxBoxCollisionAlgorithm AllocFromPool(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
        {
            BoxBoxCollisionAlgorithm result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new BoxBoxCollisionAlgorithm();
            result.Constructor(mf, ci, body0, body1);
            return result;
        }
        public override void free()
        {
            if (m_ownManifold)
            {
                if (m_manifoldPtr != null)
                    m_dispatcher.releaseManifold(m_manifoldPtr);
            }
            ObjPool.Enqueue(this);
        }

        bool m_ownManifold;
        PersistentManifold m_manifoldPtr;

        public override void processCollision(CollisionObject col0, CollisionObject col1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            if (m_manifoldPtr == null)
                return;

            BoxShape box0 = (BoxShape)col0.CollisionShape;
            BoxShape box1 = (BoxShape)col1.CollisionShape;



            /// report a contact. internally this will be kept persistent, and contact reduction is done
            resultOut.PersistentManifold = m_manifoldPtr;
#if! USE_PERSISTENT_CONTACTS	
            	m_manifoldPtr->clearManifold();
#endif //USE_PERSISTENT_CONTACTS

            ClosestPointInput input;
            input.m_maximumDistanceSquared = BulletGlobal.BT_LARGE_FLOAT;
            input.m_transformA = col0.WorldTransform;
            input.m_transformB = col1.WorldTransform;

            BoxBoxDetector detector = new BoxBoxDetector(box0, box1);
            detector.getClosestPoints(ref input, ref resultOut, dispatchInfo.m_debugDraw);

            //  refreshContactPoints is only necessary when using persistent contact points. otherwise all points are newly added
            if (m_ownManifold)
            {
                resultOut.refreshContactPoints();
            }

        }
        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            //not yet
            return 1f;
        }

        BoxBoxCollisionAlgorithm() { }
        //オブジェクトプールを使うので初期化処理の代わりを……
        protected void Constructor(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject obj0, CollisionObject obj1)
        {
            base.Constructor(ci);
            m_ownManifold = false;
            m_manifoldPtr = mf;
            if (m_manifoldPtr == null && m_dispatcher.needsCollision(obj0, obj1))
            {
                m_manifoldPtr = m_dispatcher.getNewManifold(obj0, obj1);
                m_ownManifold = true;
            }
        }
        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            if (m_manifoldPtr != null && m_ownManifold)
            {
                manifoldArray.Add(m_manifoldPtr);
            }
        }
        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            public CreateFunc() { }

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                return AllocFromPool(null, ci, body0, body1);
            }
        }

    }
}
