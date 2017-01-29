using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class ConvexPlaneCollisionAlgorithm : CollisionAlgorithm
    {
        //オブジェクトプール
        static Queue<ConvexPlaneCollisionAlgorithm> ObjPool
            = new Queue<ConvexPlaneCollisionAlgorithm>(new ConvexPlaneCollisionAlgorithm[1] { new ConvexPlaneCollisionAlgorithm() });
        static ConvexPlaneCollisionAlgorithm AllocFromPool(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject col0, CollisionObject col1, bool isSwapped, int numPerturbationIterations, int minimumPointsPerturbationThreshold)
        {
            ConvexPlaneCollisionAlgorithm result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new ConvexPlaneCollisionAlgorithm();
            result.Constructor(mf, ci, col0, col1, isSwapped, numPerturbationIterations, minimumPointsPerturbationThreshold);
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
        bool m_isSwapped;
        int m_numPerturbationIterations;
        int m_minimumPointsPerturbationThreshold;


        public ConvexPlaneCollisionAlgorithm() { }
        //オブジェクトプールを使うので初期化処理の代わりを……
        public void Constructor(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject col0, CollisionObject col1, bool isSwapped, int numPerturbationIterations, int minimumPointsPerturbationThreshold)
        {
            base.Constructor(ci);
            m_ownManifold = false;
            m_manifoldPtr = mf;
            m_isSwapped = isSwapped;
            m_numPerturbationIterations = numPerturbationIterations;
            m_minimumPointsPerturbationThreshold = minimumPointsPerturbationThreshold;

            CollisionObject convexObj = m_isSwapped ? col1 : col0;
            CollisionObject planeObj = m_isSwapped ? col0 : col1;

            if (m_manifoldPtr == null && m_dispatcher.needsCollision(convexObj, planeObj))
            {
                m_manifoldPtr = m_dispatcher.getNewManifold(convexObj, planeObj);
                m_ownManifold = true;
            }
        }
        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            if (m_manifoldPtr == null)
                return;

            CollisionObject convexObj = m_isSwapped ? body1 : body0;
            CollisionObject planeObj = m_isSwapped ? body0 : body1;

            ConvexShape convexShape = (ConvexShape)convexObj.CollisionShape;
            StaticPlaneShape planeShape = (StaticPlaneShape)planeObj.CollisionShape;

            btVector3 planeNormal = planeShape.PlaneNormal;
            //const btScalar& planeConstant = planeShape->getPlaneConstant();

            //first perform a collision query with the non-perturbated collision objects
            {
                btQuaternion rotq = new btQuaternion(0, 0, 0, 1);
                collideSingleContact(rotq, body0, body1, dispatchInfo, ref resultOut);
            }

            if (resultOut.PersistentManifold.NumContacts < m_minimumPointsPerturbationThreshold)
            {
                btVector3 v0, v1;
                btVector3.PlaneSpace1(ref planeNormal, out v0, out v1);
                //now perform 'm_numPerturbationIterations' collision queries with the perturbated collision objects

                float angleLimit = 0.125f * BulletGlobal.SIMD_PI;
                float perturbeAngle;
                float radius = convexShape.getAngularMotionDisc();
                perturbeAngle = PersistentManifold.gContactBreakingThreshold / radius;
                if (perturbeAngle > angleLimit)
                    perturbeAngle = angleLimit;

                btQuaternion perturbeRot = new btQuaternion(v0, perturbeAngle);
                for (int i = 0; i < m_numPerturbationIterations; i++)
                {
                    float iterationAngle = (float)i * (BulletGlobal.SIMD_2_PI / (float)m_numPerturbationIterations);
                    btQuaternion rotq = new btQuaternion(planeNormal, iterationAngle);
                    collideSingleContact(rotq.inverse() * perturbeRot * rotq, body0, body1, dispatchInfo, ref resultOut);
                }
                
            }

            if (m_ownManifold)
            {
                if (m_manifoldPtr.NumContacts != 0)
                {
                    resultOut.refreshContactPoints();
                }
            }
        }
        public void collideSingleContact(btQuaternion perturbeRot, CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            CollisionObject convexObj = m_isSwapped ? body1 : body0;
            CollisionObject planeObj = m_isSwapped ? body0 : body1;

            ConvexShape convexShape = (ConvexShape)convexObj.CollisionShape;
            StaticPlaneShape planeShape = (StaticPlaneShape)planeObj.CollisionShape;

            bool hasCollision = false;
            btVector3 planeNormal = planeShape.PlaneNormal;
            float planeConstant = planeShape.PlaneConstant;

            btTransform convexWorldTransform = convexObj.WorldTransform;
            btTransform convexInPlaneTrans;
            convexInPlaneTrans = planeObj.WorldTransform.inverse() * convexWorldTransform;
            //now perturbe the convex-world transform
            #region convexWorldTransform.Basis *= new btMatrix3x3(perturbeRot);
            {
                btMatrix3x3 temp1 = convexWorldTransform.Basis, temp2 = new btMatrix3x3(perturbeRot);
                btMatrix3x3.Multiply(ref temp1, ref temp2, out convexWorldTransform.Basis);
            }
            #endregion
            btTransform planeInConvex;
            planeInConvex = convexWorldTransform.inverse() * planeObj.WorldTransform;

            #region btVector3 vtx = convexShape.localGetSupportingVertex(planeInConvex.Basis * -planeNormal);
            btVector3 vtx;
            {
                btVector3 temp, temp2;
                temp2 = -planeNormal;
                btMatrix3x3.Multiply(ref planeInConvex.Basis, ref temp2, out temp);
                //vtx = convexShape.localGetSupportingVertex(temp);
                convexShape.localGetSupportingVertex(ref temp, out vtx);
            }
            #endregion
            btVector3 vtxInPlane = convexInPlaneTrans * vtx;
            float distance = (planeNormal.dot(vtxInPlane) - planeConstant);

            btVector3 vtxInPlaneProjected = vtxInPlane - distance * planeNormal;
            btVector3 vtxInPlaneWorld = planeObj.WorldTransform * vtxInPlaneProjected;

            hasCollision = distance < m_manifoldPtr.ContactBreakingThreshold;
            resultOut.PersistentManifold = m_manifoldPtr;
            if (hasCollision)
            {
                /// report a contact. internally this will be kept persistent, and contact reduction is done
                btVector3 normalOnSurfaceB;// = planeObj.WorldTransform.Basis * planeNormal;
                btMatrix3x3.Multiply(ref planeObj.WorldTransform.Basis, ref planeNormal, out normalOnSurfaceB);
                btVector3 pOnB = vtxInPlaneWorld;
                resultOut.addContactPoint(ref normalOnSurfaceB, ref pOnB, distance);
            }
        }
        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            //not yet
            return 1f;
        }
        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            if (m_manifoldPtr!=null && m_ownManifold)
            {
                manifoldArray.Add(m_manifoldPtr);
            }
        }
        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            public int m_numPerturbationIterations;
            public int m_minimumPointsPerturbationThreshold;

            public CreateFunc()
            {
                m_numPerturbationIterations = 1;
                m_minimumPointsPerturbationThreshold = 1;
            }

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                if (!m_swapped)
                {
                    return AllocFromPool(null, ci, body0, body1, false, m_numPerturbationIterations, m_minimumPointsPerturbationThreshold);
                }
                else
                {
                    return AllocFromPool(null, ci, body0, body1, true, m_numPerturbationIterations, m_minimumPointsPerturbationThreshold);
                }
            }
        }
    }
}
