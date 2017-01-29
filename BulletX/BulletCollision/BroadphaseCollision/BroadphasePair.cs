using System.Collections.Generic;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class BroadphasePair
    {
        static Queue<BroadphasePair> ObjPool = new Queue<BroadphasePair>();
        public static BroadphasePair Allocate(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            BroadphasePair result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new BroadphasePair();
            result.Constructor(proxy0, proxy1);
            return result;
        }
        public void free()
        {
            ObjPool.Enqueue(this);
        }

        public BroadphaseProxy m_pProxy0;
        public BroadphaseProxy m_pProxy1;

        public CollisionAlgorithm m_algorithm;
        //影響なしを確認
        //union { void* m_internalInfo1; int m_internalTmpValue;};//don't use this data, it will be removed in future version.
        public int m_internalTmpValue;//現在これだけ使われている

        BroadphasePair()
        {
            m_pProxy0 = null;
            m_pProxy1 = null;
            m_algorithm = null;
            m_internalTmpValue = 0;
        }
        void Constructor(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            //keep them sorted, so the std::set operations work
            if (proxy0.m_uniqueId < proxy1.m_uniqueId)
            {
                m_pProxy0 = proxy0;
                m_pProxy1 = proxy1;
            }
            else
            {
                m_pProxy0 = proxy1;
                m_pProxy1 = proxy0;
            }

            m_algorithm = null;
            m_internalTmpValue = 0;

        }
    }
}
