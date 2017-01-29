using System.Collections.Generic;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class UnionFindElementSortPredicate : IComparer<Element>
    {
        #region IComparer<UnionFind> メンバ

        public int Compare(Element lhs, Element rhs )
		{
            return (lhs.m_id < rhs.m_id) ? -1 : 1;
		}

        #endregion
    }
}
