using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    interface IConvexPenetrationDepthSolver
    {
        bool calcPenDepth(ISimplexSolver simplexSolver,
            ConvexShape convexA, ConvexShape convexB,
            btTransform transA, btTransform transB, 
            ref btVector3 v,out btVector3 pa,out btVector3 pb,
            IDebugDraw debugDraw);
    }
}
