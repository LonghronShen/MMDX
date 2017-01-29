using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    class GjkEpaPenetrationDepthSolver : IConvexPenetrationDepthSolver
    {
        public GjkEpaPenetrationDepthSolver() { }

        #region IConvexPenetrationDepthSolver メンバ

        public bool calcPenDepth(ISimplexSolver simplexSolver, ConvexShape pConvexA, ConvexShape pConvexB, btTransform transformA, btTransform transformB, ref btVector3 v, out btVector3 wWitnessOnA, out btVector3 wWitnessOnB, IDebugDraw debugDraw)
        {
            btVector3 guessVector;// = transformA.Origin - transformB.Origin;
            btVector3.Subtract(ref transformA.Origin,ref transformB.Origin, out guessVector);
            GjkEpaSolver2.sResults results = new GjkEpaSolver2.sResults();

            
            if (GjkEpaSolver2.Penetration(pConvexA, transformA,
                                        pConvexB, transformB,
                                        guessVector, ref results))
            {
                //	debugDraw->drawLine(results.witnesses[1],results.witnesses[1]+results.normal,btVector3(255,0,0));
                //resultOut->addContactPoint(results.normal,results.witnesses[1],-results.depth);
                wWitnessOnA = results.witnesses0;
                wWitnessOnB = results.witnesses1;
                v = results.normal;
                return true;
            }
            else
            {
                if (GjkEpaSolver2.Distance(pConvexA, transformA, pConvexB, transformB, guessVector, ref results))
                {
                    wWitnessOnA = results.witnesses0;
                    wWitnessOnB = results.witnesses1;
                    v = results.normal;
                    return false;
                }
            }
            wWitnessOnA = new btVector3();
            wWitnessOnB = new btVector3();
            return false;
            
        }

        #endregion
    }
}
