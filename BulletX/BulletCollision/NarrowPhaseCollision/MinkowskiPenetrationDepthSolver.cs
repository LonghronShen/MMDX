using System;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    class MinkowskiPenetrationDepthSolver : IConvexPenetrationDepthSolver
    {
        public MinkowskiPenetrationDepthSolver()
        {
            throw new NotImplementedException();
        }

        #region IConvexPenetrationDepthSolver メンバ

        public bool calcPenDepth(ISimplexSolver simplexSolver, ConvexShape convexA, ConvexShape convexB, btTransform transA, btTransform transB, ref btVector3 v, out btVector3 pa, out btVector3 pb, IDebugDraw debugDraw)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
