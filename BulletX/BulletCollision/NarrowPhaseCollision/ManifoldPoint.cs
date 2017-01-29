using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    public class ManifoldPoint : ObjPoolBase<ManifoldPoint>
    {
        internal override void Free() { 
            ObjPool.Enqueue(this); 
        }

        public btVector3 m_localPointA;
        public btVector3 m_localPointB;
        public btVector3 m_positionWorldOnB;
        public btVector3 PositionWorldOnB { get { return m_positionWorldOnB; } }
        ///m_positionWorldOnA is redundant information, see getPositionWorldOnA(), but for clarity
        public btVector3 m_positionWorldOnA;
        public btVector3 PositionWorldOnA { get { return m_positionWorldOnA; } }
        public btVector3 m_normalWorldOnB;

        public float m_distance1;
        public float Distance { get { return m_distance1; } }
        public float m_combinedFriction;
        public float m_combinedRestitution;

        //BP mod, store contact triangles.
        public int m_partId0;
        public int m_partId1;
        public int m_index0;
        public int m_index1;

        public object m_userPersistentData;
        public float m_appliedImpulse;

        public bool m_lateralFrictionInitialized;
        public float m_appliedImpulseLateral1;
        public float m_appliedImpulseLateral2;
        public float m_contactMotion1;
        public float m_contactMotion2;
        public float m_contactCFM1;
        public float m_contactCFM2;

        public int m_lifeTime;//lifetime of the contactpoint in frames
        public int LifeTime { get { return m_lifeTime; } }

        public btVector3 m_lateralFrictionDir1;
        public btVector3 m_lateralFrictionDir2;
        public void Initialize()
        {
            m_userPersistentData = null;
            m_appliedImpulse = 0f;
            m_lateralFrictionInitialized = false;
            m_appliedImpulseLateral1 = 0f;
            m_appliedImpulseLateral2 = 0f;
            m_contactMotion1 = 0f;
            m_contactMotion2 = 0f;
            m_contactCFM1 = 0f;
            m_contactCFM2 = 0f;
            m_lifeTime = 0;
        }
        internal static ManifoldPoint GetFromPool(btVector3 pointA, btVector3 pointB, btVector3 normal, float distance)
        {
            ManifoldPoint result = GetFromPool();

            result.m_localPointA = pointA;
            result.m_localPointB = pointB;
            result.m_normalWorldOnB = normal;
            result.m_distance1 = distance;
            result.m_combinedFriction = 0f;
            result.m_combinedRestitution = 0;
            result.m_userPersistentData = null;
            result.m_appliedImpulse = 0f;
            result.m_lateralFrictionInitialized = false;
            result.m_appliedImpulseLateral1 = 0f;
            result.m_appliedImpulseLateral2 = 0f;
            result.m_contactMotion1 = 0f;
            result.m_contactMotion2 = 0f;
            result.m_contactCFM1 = 0f;
            result.m_contactCFM2 = 0f;
            result.m_lifeTime = 0;

            result.m_lateralFrictionDir1 = btVector3.Zero;
            result.m_lateralFrictionDir2 = btVector3.Zero;
            result.m_index0 = -1;
            result.m_index1 = -1;
            result.m_partId0 = -1;
            result.m_partId1 = -1;
            result.m_positionWorldOnA = btVector3.Zero;
            result.m_positionWorldOnB = btVector3.Zero;
            return result;
        }
        public void CopyTo(ManifoldPoint dst)
        {
            dst.m_localPointA = m_localPointA;
            dst.m_localPointB = m_localPointB;
            dst.m_positionWorldOnB = m_positionWorldOnB;
            dst.m_positionWorldOnA = m_positionWorldOnA;
            dst.m_normalWorldOnB = m_normalWorldOnB;

            dst.m_distance1 = m_distance1;
            dst.m_combinedFriction = m_combinedFriction;
            dst.m_combinedRestitution = m_combinedRestitution;

            //BP mod, store contact triangles.
            dst.m_partId0 = m_partId0;
            dst.m_partId1 = m_partId1;
            dst.m_index0 = m_index0;
            dst.m_index1 = m_index1;

            dst.m_userPersistentData = m_userPersistentData;
            dst.m_appliedImpulse = m_appliedImpulse;

            dst.m_lateralFrictionInitialized = m_lateralFrictionInitialized;
            dst.m_appliedImpulseLateral1 = m_appliedImpulseLateral1;
            dst.m_appliedImpulseLateral2 = m_appliedImpulseLateral2;
            dst.m_contactMotion1 = m_contactMotion1;
            dst.m_contactMotion2 = m_contactMotion2;
            dst.m_contactCFM1 = m_contactCFM1;
            dst.m_contactCFM2 = m_contactCFM2;

            dst.m_lifeTime = m_lifeTime;//lifetime of the contactpoint in frames

            dst.m_lateralFrictionDir1 = m_lateralFrictionDir1;
            dst.m_lateralFrictionDir2 = m_lateralFrictionDir2;
        }
    }
}
