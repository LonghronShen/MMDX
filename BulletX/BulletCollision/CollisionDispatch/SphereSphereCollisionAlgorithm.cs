using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class SphereSphereCollisionAlgorithm : ActivatingCollisionAlgorithm
    {
        //オブジェクトプール
        static Queue<SphereSphereCollisionAlgorithm> ObjPool
            = new Queue<SphereSphereCollisionAlgorithm>(new SphereSphereCollisionAlgorithm[1] { new SphereSphereCollisionAlgorithm() });
        static SphereSphereCollisionAlgorithm AllocFromPool(PersistentManifold mf,CollisionAlgorithmConstructionInfo ci,CollisionObject body0,CollisionObject body1)
        {
            SphereSphereCollisionAlgorithm result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new SphereSphereCollisionAlgorithm();
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
	
        SphereSphereCollisionAlgorithm() { }
        public void Constructor(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject col0, CollisionObject col1)
        {
            base.Constructor(ci);
            m_ownManifold = false;
            m_manifoldPtr = mf;

            if (m_manifoldPtr == null)
            {
                m_manifoldPtr = m_dispatcher.getNewManifold(col0, col1);
                m_ownManifold = true;
            }
        }
        public override void processCollision(CollisionObject col0, CollisionObject col1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            if (m_manifoldPtr == null)
                return;

            resultOut.PersistentManifold = m_manifoldPtr;

            SphereShape sphere0 = (SphereShape)col0.CollisionShape;
            SphereShape sphere1 = (SphereShape)col1.CollisionShape;

            btVector3 diff = col0.WorldTransform.Origin - col1.WorldTransform.Origin;
            float len = diff.Length;
            float radius0 = sphere0.Radius;
            float radius1 = sphere1.Radius;

#if CLEAR_MANIFOLD
	        m_manifoldPtr->clearManifold(); //don't do this, it disables warmstarting
#endif

            ///iff distance positive, don't generate a new contact
            if (len > (radius0 + radius1))
            {
#if! CLEAR_MANIFOLD
                resultOut.refreshContactPoints();
#endif //CLEAR_MANIFOLD
                return;
            }
            ///distance (negative means penetration)
            float dist = len - (radius0 + radius1);

            btVector3 normalOnSurfaceB = new btVector3(1, 0, 0);
            if (len > BulletGlobal.SIMD_EPSILON)
            {
                //normalOnSurfaceB = diff / len;
                btVector3.Divide(ref diff, len, out normalOnSurfaceB);
            }

            ///point on A (worldspace)
            ///btVector3 pos0 = col0->getWorldTransform().getOrigin() - radius0 * normalOnSurfaceB;
            ///point on B (worldspace)
            btVector3 pos1;// = col1.WorldTransform.Origin + radius1 * normalOnSurfaceB;
            {
                btVector3 temp;
                btVector3.Multiply(ref normalOnSurfaceB, radius1, out temp);
                btVector3.Add(ref col1.WorldTransform.Origin, ref temp, out pos1);
            }
            /// report a contact. internally this will be kept persistent, and contact reduction is done


            resultOut.addContactPoint(ref normalOnSurfaceB, ref pos1, dist);

#if! CLEAR_MANIFOLD
            resultOut.refreshContactPoints();
#endif //CLEAR_MANIFOLD

        }
        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            //not yet
            return 1f;
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

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo info, CollisionObject body0, CollisionObject body1)
            {
                return AllocFromPool(null, info, body0, body1);
            }
        }
    }
}
