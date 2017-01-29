using System.Collections.Generic;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletDynamics.ConstraintSolver;

namespace BulletX.BulletDynamics.Dynamics
{
    class SortConstraintOnIslandPredicate : IComparer<TypedConstraint>
    {
        #region IComparer<TypedConstraint> メンバ

        public int Compare(TypedConstraint lhs, TypedConstraint rhs)
        {
            int rIslandId0, lIslandId0;
            rIslandId0 = GetConstraintIslandId(rhs);
            lIslandId0 = GetConstraintIslandId(lhs);
            return (lIslandId0 < rIslandId0) ? -1 : 1;
        }

        #endregion
        int GetConstraintIslandId(TypedConstraint lhs)
        {
            int islandId;

            CollisionObject rcolObj0 = lhs.RigidBodyA;
            CollisionObject rcolObj1 = lhs.RigidBodyB;
            islandId = rcolObj0.IslandTag >= 0 ? rcolObj0.IslandTag : rcolObj1.IslandTag;
            return islandId;

        }
    }
}
