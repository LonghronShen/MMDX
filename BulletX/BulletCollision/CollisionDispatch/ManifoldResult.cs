using System.Diagnostics;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public delegate void ContactAddedCallback(ManifoldPoint cp,CollisionObject colObj0,int partId0,int index0,CollisionObject colObj1,int partId1,int index1);
    public struct ManifoldResult
    {
        public static event ContactAddedCallback gContactAddedCallback = null;

        PersistentManifold m_manifoldPtr;
        public PersistentManifold PersistentManifold { get { return m_manifoldPtr; } set { m_manifoldPtr = value; } }

        //we need this for compounds
        btTransform m_rootTransA;
        btTransform m_rootTransB;

        CollisionObject m_body0;
        CollisionObject m_body1;
        int m_partId0;
        int m_partId1;
        int m_index0;
        int m_index1;

        public ManifoldResult(CollisionObject body0, CollisionObject body1)
        {
            m_manifoldPtr = null;
            m_body0 = body0;
            m_body1 = body1;
            m_rootTransA = body0.WorldTransform;
            m_rootTransB = body1.WorldTransform;
            m_partId0 = -1;
            m_partId1 = -1;
            m_index0 = -1;
            m_index1 = -1;
        }

        

        public void refreshContactPoints()
        {
            Debug.Assert(m_manifoldPtr != null);
            if (m_manifoldPtr.NumContacts == 0)
                return;

            bool isSwapped = m_manifoldPtr.Body0 != m_body0;

            if (isSwapped)
            {
                m_manifoldPtr.refreshContactPoints(m_rootTransB, m_rootTransA);
            }
            else
            {
                m_manifoldPtr.refreshContactPoints(m_rootTransA, m_rootTransB);
            }
        }
        public void addContactPoint(ref btVector3 normalOnBInWorld, ref btVector3 pointInWorld, float depth)
        {
            Debug.Assert(m_manifoldPtr != null);
            //order in manifold needs to match

            if (depth > m_manifoldPtr.ContactBreakingThreshold)
                return;

            bool isSwapped = m_manifoldPtr.Body0 != m_body0;

            btVector3 pointA;// = pointInWorld + normalOnBInWorld * depth;
            {
                btVector3 temp;
                btVector3.Multiply(ref normalOnBInWorld, depth, out temp);
                btVector3.Add(ref pointInWorld, ref temp, out pointA);
            }

            btVector3 localA;
            btVector3 localB;

            if (isSwapped)
            {
                localA = m_rootTransB.invXform(pointA);
                localB = m_rootTransA.invXform(pointInWorld);
            }
            else
            {
                localA = m_rootTransA.invXform(pointA);
                localB = m_rootTransB.invXform(pointInWorld);
            }
            //ローカル変数用途としてManifoldPointをプールから確保。このオブジェクトはコピーして使用
            ManifoldPoint newPt = ManifoldPoint.GetFromPool(localA, localB, normalOnBInWorld, depth);
            newPt.m_positionWorldOnA = pointA;
            newPt.m_positionWorldOnB = pointInWorld;

            int insertIndex = m_manifoldPtr.getCacheEntry(newPt);

            newPt.m_combinedFriction = calculateCombinedFriction(m_body0, m_body1);
            newPt.m_combinedRestitution = calculateCombinedRestitution(m_body0, m_body1);

            //BP mod, store contact triangles.
            if (isSwapped)
            {
                newPt.m_partId0 = m_partId1;
                newPt.m_partId1 = m_partId0;
                newPt.m_index0 = m_index1;
                newPt.m_index1 = m_index0;
            }
            else
            {
                newPt.m_partId0 = m_partId0;
                newPt.m_partId1 = m_partId1;
                newPt.m_index0 = m_index0;
                newPt.m_index1 = m_index1;
            }
            //printf("depth=%f\n",depth);
            ///@todo, check this for any side effects
            if (insertIndex >= 0)
            {
                //const btManifoldPoint& oldPoint = m_manifoldPtr->getContactPoint(insertIndex);
                m_manifoldPtr.replaceContactPoint(newPt, insertIndex);
            }
            else
            {
                insertIndex = m_manifoldPtr.addManifoldPoint(newPt);
            }

            //User can override friction and/or restitution
            if (gContactAddedCallback != null &&
                //and if either of the two bodies requires custom material
                 ((m_body0.CollisionFlags & CollisionFlags.CF_CUSTOM_MATERIAL_CALLBACK) != 0 ||
                   (m_body1.CollisionFlags & CollisionFlags.CF_CUSTOM_MATERIAL_CALLBACK) != 0))
            {
                //experimental feature info, for per-triangle material etc.
                CollisionObject obj0 = isSwapped ? m_body1 : m_body0;
                CollisionObject obj1 = isSwapped ? m_body0 : m_body1;
                ManifoldPoint cp = m_manifoldPtr.getContactPoint(insertIndex);
                gContactAddedCallback(cp, obj0, newPt.m_partId0, newPt.m_index0, obj1, newPt.m_partId1, newPt.m_index1);
            }
            //ローカル変数なので開放
            newPt.Free();
        }
        const float MAX_FRICTION = 10f;
        static float calculateCombinedFriction(CollisionObject body0, CollisionObject body1)
        {
	        float friction = body0.Friction * body1.Friction;

	        if (friction < -MAX_FRICTION)
		        friction = -MAX_FRICTION;
	        if (friction > MAX_FRICTION)
		        friction = MAX_FRICTION;
	        return friction;

        }
        static float calculateCombinedRestitution(CollisionObject body0, CollisionObject body1)
        {
            return body0.Restitution * body1.Restitution;
        }

    }
}
