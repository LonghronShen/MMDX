using System.Collections.Generic;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    class BroadphasePairSortPredicate : IComparer<BroadphasePair>
    {
        
        #region IComparer<BroadphasePair> メンバ

        public int Compare(BroadphasePair a, BroadphasePair b)
        {
            int uidA0 = (a.m_pProxy0 != null ? a.m_pProxy0.m_uniqueId : -1);
            int uidB0 = (b.m_pProxy0 != null ? b.m_pProxy0.m_uniqueId : -1);
            int uidA1 = (a.m_pProxy1 != null ? a.m_pProxy1.m_uniqueId : -1);
            int uidB1 = (b.m_pProxy1 != null ? b.m_pProxy1.m_uniqueId : -1);
            //あってるのかなぁ……？
            if( uidA0 > uidB0 ||
               (a.m_pProxy0 == b.m_pProxy0 && uidA1 > uidB1) ||
               (a.m_pProxy0 == b.m_pProxy0 && a.m_pProxy1 == b.m_pProxy1 && a.m_algorithm.AlgorithmID > b.m_algorithm.AlgorithmID))
                return -1;
            return 1;
        }

        #endregion
    }
}
