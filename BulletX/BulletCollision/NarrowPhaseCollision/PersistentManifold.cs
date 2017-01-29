using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    public delegate void ContactDestroyedCallback(object userPersistentData);
    public delegate void ContactProcessedCallback(ManifoldPoint cp, CollisionObject body0, CollisionObject body1);
    public class PersistentManifold : TypedObject<ContactManifoldTypes>
    {
        public const float gContactBreakingThreshold = 0.02f;
        public const int MANIFOLD_CACHE_SIZE = 4;
        public static event ContactDestroyedCallback gContactDestroyedCallback;
        public static event ContactProcessedCallback gContactProcessedCallback;

        static Queue<PersistentManifold> ObjPool = new Queue<PersistentManifold>();
        public static PersistentManifold allocate(CollisionObject body0, CollisionObject body1, int n, float contactBreakingThreshold, float contactProcessingThreshold)
        {
            if (ObjPool.Count > 0)
            {
                ObjPool.Peek().Constructor(body0, body1, contactBreakingThreshold, contactProcessingThreshold);
                return ObjPool.Dequeue();
            }
            PersistentManifold result = new PersistentManifold();
            result.Constructor(body0, body1, contactBreakingThreshold, contactProcessingThreshold);
            return result;
        }

        ManifoldPoint[] m_pointCache = new ManifoldPoint[4];
        CollisionObject m_body0;
        public CollisionObject Body0 { get { return m_body0; } }
        CollisionObject m_body1;
        public CollisionObject Body1 { get { return m_body1; } }
        int m_cachedPoints;
        public int NumContacts { get { return m_cachedPoints; } }

        float m_contactBreakingThreshold;
        public float ContactBreakingThreshold { get { return m_contactBreakingThreshold; } }
        float m_contactProcessingThreshold;
        public float ContactProcessingThreshold { get { return m_contactProcessingThreshold; } }
        public int m_index1a;

        private void Constructor(CollisionObject body0, CollisionObject body1, float contactBreakingThreshold, float contactProcessingThreshold)
        {
            m_body0 = body0;
            m_body1 = body1;
            m_cachedPoints = 0;
            m_contactBreakingThreshold = contactBreakingThreshold;
            m_contactProcessingThreshold = contactProcessingThreshold;
            for (int i = 0; i < m_pointCache.Length; i++)
                m_pointCache[i].Initialize();
            m_index1a = -1;
        }

        private PersistentManifold() : base(ContactManifoldTypes.BT_PERSISTENT_MANIFOLD_TYPE) 
        {
            for (int i = 0; i < m_pointCache.Length; i++)
                m_pointCache[i] = ManifoldPoint.GetFromPool();
        }


        public void clearManifold()
        {
            int i;
            for (i = 0; i < m_cachedPoints; i++)
            {
                clearUserCache(m_pointCache[i]);
            }
            m_cachedPoints = 0;
            ObjPool.Enqueue(this);
        }
        public void clearUserCache(ManifoldPoint pt)
        {

            if (pt.m_userPersistentData != null)
            {
                if (gContactDestroyedCallback != null)
                {
                    gContactDestroyedCallback(pt.m_userPersistentData);
                    pt.m_userPersistentData = null;
                }
            }
        }

        public void refreshContactPoints(btTransform trA, btTransform trB)
        {
            /// first refresh worldspace positions and distance
            for (int i = NumContacts - 1; i >= 0; i--)
            {
                m_pointCache[i].m_positionWorldOnA = btVector3.Transform(m_pointCache[i].m_localPointA, trA);
                m_pointCache[i].m_positionWorldOnB = btVector3.Transform(m_pointCache[i].m_localPointB, trB);
                m_pointCache[i].m_distance1 = (m_pointCache[i].m_positionWorldOnA - m_pointCache[i].m_positionWorldOnB).dot(m_pointCache[i].m_normalWorldOnB);
                m_pointCache[i].m_lifeTime++;
            }

            /// then 
            float distance2d;
            btVector3 projectedDifference, projectedPoint;
            for (int i = NumContacts - 1; i >= 0; i--)
            {

                //contact becomes invalid when signed distance exceeds margin (projected on contactnormal direction)
                if (!validContactDistance(m_pointCache[i]))
                {
                    removeContactPoint(i);
                }
                else
                {
                    //contact also becomes invalid when relative movement orthogonal to normal exceeds margin
                    projectedPoint = m_pointCache[i].m_positionWorldOnA - m_pointCache[i].m_normalWorldOnB * m_pointCache[i].m_distance1;
                    projectedDifference = m_pointCache[i].m_positionWorldOnB - projectedPoint;
                    distance2d = projectedDifference.dot(projectedDifference);
                    if (distance2d > ContactBreakingThreshold * ContactBreakingThreshold)
                    {
                        removeContactPoint(i);
                    }
                    else
                    {
                        //contact point processed callback
                        if (gContactProcessedCallback != null)
                            gContactProcessedCallback(m_pointCache[i], m_body0, m_body1);
                    }
                }
            }
        }

        public void removeContactPoint(int index)
        {
            clearUserCache(m_pointCache[index]);

            int lastUsedIndex = NumContacts - 1;
            //		m_pointCache[index] = m_pointCache[lastUsedIndex];
            if (index != lastUsedIndex)
            {
                //m_pointCache[index] = m_pointCache[lastUsedIndex];
                m_pointCache[lastUsedIndex].CopyTo(m_pointCache[index]);
                //get rid of duplicated userPersistentData pointer
                m_pointCache[lastUsedIndex].m_userPersistentData = null;
                m_pointCache[lastUsedIndex].m_appliedImpulse = 0f;
                m_pointCache[lastUsedIndex].m_lateralFrictionInitialized = false;
                m_pointCache[lastUsedIndex].m_appliedImpulseLateral1 = 0f;
                m_pointCache[lastUsedIndex].m_appliedImpulseLateral2 = 0f;
                m_pointCache[lastUsedIndex].m_lifeTime = 0;
            }

            Debug.Assert(m_pointCache[lastUsedIndex].m_userPersistentData == null);
            m_cachedPoints--;
        }

        public bool validContactDistance(ManifoldPoint pt)
	    {
		    return pt.m_distance1 <= ContactBreakingThreshold;
	    }

        public int getCacheEntry(ManifoldPoint newPoint)
        {
            float shortestDist = ContactBreakingThreshold * ContactBreakingThreshold;
            int size = NumContacts;
            int nearestPoint = -1;
            for (int i = 0; i < size; i++)
            {
                btVector3 diffA = m_pointCache[i].m_localPointA - newPoint.m_localPointA;
                float distToManiPoint = diffA.dot(diffA);
                if (distToManiPoint < shortestDist)
                {
                    shortestDist = distToManiPoint;
                    nearestPoint = i;
                }
            }
            return nearestPoint;
        }

        public void replaceContactPoint(ManifoldPoint newPoint, int insertIndex)
        {
            Debug.Assert(validContactDistance(newPoint));

		    int	lifeTime = m_pointCache[insertIndex].LifeTime;
		    float	appliedImpulse = m_pointCache[insertIndex].m_appliedImpulse;
		    float	appliedLateralImpulse1 = m_pointCache[insertIndex].m_appliedImpulseLateral1;
		    float	appliedLateralImpulse2 = m_pointCache[insertIndex].m_appliedImpulseLateral2;

            Debug.Assert(lifeTime >= 0);
		    object cache = m_pointCache[insertIndex].m_userPersistentData;
            //m_pointCache[insertIndex].Free();
		    //m_pointCache[insertIndex] = newPoint;
            newPoint.CopyTo(m_pointCache[insertIndex]);
		    m_pointCache[insertIndex].m_userPersistentData = cache;
		    m_pointCache[insertIndex].m_appliedImpulse = appliedImpulse;
		    m_pointCache[insertIndex].m_appliedImpulseLateral1 = appliedLateralImpulse1;
		    m_pointCache[insertIndex].m_appliedImpulseLateral2 = appliedLateralImpulse2;
    		
		    m_pointCache[insertIndex].m_lifeTime = lifeTime;
        }

        public int addManifoldPoint(ManifoldPoint newPoint)
        {
            Debug.Assert(validContactDistance(newPoint));

            int insertIndex = NumContacts;
            if (insertIndex == MANIFOLD_CACHE_SIZE)
            {
                /*if (MANIFOLD_CACHE_SIZE >= 4)
                {*/
                    //sort cache so best points come first, based on area
                    insertIndex = sortCachedPoints(newPoint);
                /*}
                else
                {
                    insertIndex = 0;
                }*/
                clearUserCache(m_pointCache[insertIndex]);

            }
            else
            {
                m_cachedPoints++;


            }
            if (insertIndex < 0)
                insertIndex = 0;

            Debug.Assert(m_pointCache[insertIndex].m_userPersistentData == null);
            /*if (m_pointCache[insertIndex] != null)
                m_pointCache[insertIndex].Free();*/
            //m_pointCache[insertIndex] = newPoint;
            newPoint.CopyTo(m_pointCache[insertIndex]);
            return insertIndex;
        }
        int sortCachedPoints(ManifoldPoint pt)
        {

            //calculate 4 possible cases areas, and take biggest area
            //also need to keep 'deepest'

            int maxPenetrationIndex = -1;
            float maxPenetration = pt.Distance;
            for (int i = 0; i < 4; i++)
            {
                if (m_pointCache[i].Distance < maxPenetration)
                {
                    maxPenetrationIndex = i;
                    maxPenetration = m_pointCache[i].Distance;
                }
            }

            float res0 = 0f, res1 = 0f, res2 = 0f, res3 = 0f;
            if (maxPenetrationIndex != 0)
            {
                btVector3 a0 = pt.m_localPointA - m_pointCache[1].m_localPointA;
                btVector3 b0 = m_pointCache[3].m_localPointA - m_pointCache[2].m_localPointA;
                btVector3 cross = a0.cross(b0);
                res0 = cross.Length2;
            }
            if (maxPenetrationIndex != 1)
            {
                btVector3 a1 = pt.m_localPointA - m_pointCache[0].m_localPointA;
                btVector3 b1 = m_pointCache[3].m_localPointA - m_pointCache[2].m_localPointA;
                btVector3 cross = a1.cross(b1);
                res1 = cross.Length2;
            }

            if (maxPenetrationIndex != 2)
            {
                btVector3 a2 = pt.m_localPointA - m_pointCache[0].m_localPointA;
                btVector3 b2 = m_pointCache[3].m_localPointA - m_pointCache[1].m_localPointA;
                btVector3 cross = a2.cross(b2);
                res2 = cross.Length2;
            }

            if (maxPenetrationIndex != 3)
            {
                btVector3 a3 = pt.m_localPointA - m_pointCache[0].m_localPointA;
                btVector3 b3 = m_pointCache[2].m_localPointA - m_pointCache[1].m_localPointA;
                btVector3 cross = a3.cross(b3);
                res3 = cross.Length2;
            }

            btVector4 maxvec = new btVector4(res0, res1, res2, res3);
            int biggestarea = maxvec.closestAxis4;
            return biggestarea;
        }
        public ManifoldPoint getContactPoint(int index)
        {
            Debug.Assert(index < m_cachedPoints);
            return m_pointCache[index];
        }
    }
}
