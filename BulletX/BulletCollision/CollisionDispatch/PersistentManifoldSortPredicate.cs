using System.Collections.Generic;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class PersistentManifoldSortPredicate : IComparer<PersistentManifold>
    {
        #region IComparer<PersistentManifold> メンバ

        public int Compare(PersistentManifold lhs, PersistentManifold rhs)
        {
            //return -(getIslandId(lhs) - getIslandId(rhs));
            return getIslandId(lhs) < getIslandId(rhs) ? -1 : 1;
        }

        #endregion
        int getIslandId(PersistentManifold lhs)
        {
            int islandId;
            CollisionObject rcolObj0 = (CollisionObject)(lhs.Body0);
            CollisionObject rcolObj1 = (CollisionObject)(lhs.Body1);
            islandId = rcolObj0.IslandTag >= 0 ? rcolObj0.IslandTag : rcolObj1.IslandTag;
            return islandId;

        }
    }
}
