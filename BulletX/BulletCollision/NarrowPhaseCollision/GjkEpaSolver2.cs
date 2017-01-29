using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;
using tShape = BulletX.BulletCollision.NarrowPhaseCollision.MinkowskiDiff;
using U = System.UInt32;
	
namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    static class GjkEpaSolver2
    {
        const float GJK_MIN_DISTANCE = 0.0001f;
        public struct sResults
        {
            public enum eStatus
            {
                Separated,		/* Shapes doesnt penetrate												*/
                Penetrating,	/* Shapes are penetrating												*/
                GJK_Failed,		/* GJK phase fail, no big issue, shapes are probably just 'touching'	*/
                EPA_Failed		/* EPA phase fail, bigger problem, need to save parameters, and debug	*/
            }
            public eStatus status;
            public btVector3 witnesses0;
            public btVector3 witnesses1;
            public btVector3 normal;
            public float distance;
        }
        public static bool Distance(ConvexShape shape0, btTransform wtrs0, ConvexShape shape1, btTransform wtrs1, btVector3 guess, ref sResults results)
        {
            tShape shape = new MinkowskiDiff();
            Initialize(shape0, wtrs0, shape1, wtrs1, ref results, ref shape, false);
            using(GJK gjk = GJK.CreateFromPool())
            {
                GJK.eStatus gjk_status = gjk.Evaluate(shape, guess);
                if (gjk_status == GJK.eStatus.Valid)
                {
                    btVector3 w0 = new btVector3(0, 0, 0);
                    btVector3 w1 = new btVector3(0, 0, 0);
                    for (U i = 0; i < gjk.m_simplex.rank; ++i)
                    {
                        float p = gjk.m_simplex.p[i];
                        btVector3 temp,temp2,temp3;
                        #region w0 += shape.Support(gjk.m_simplex.c[i].d, 0) * p;
                        shape.Support(ref gjk.m_simplex.c[i].d, 0, out temp);
                        btVector3.Multiply(ref temp, p, out temp2);
                        w0.Add(ref temp2);
                        #endregion
                        #region w1 += shape.Support(-gjk.m_simplex.c[i].d, 1) * p;
                        btVector3.Minus(ref gjk.m_simplex.c[i].d, out temp3);
                        shape.Support(ref temp3, 1, out temp);
                        btVector3.Multiply(ref temp, p, out temp2);
                        w1.Add(ref temp2);
                        #endregion
                    }
                    results.witnesses0 = wtrs0 * w0;
                    results.witnesses1 = wtrs0 * w1;
                    results.normal = w0 - w1;
                    results.distance = results.normal.Length;
                    results.normal /= results.distance > GJK_MIN_DISTANCE ? results.distance : 1;
                    return (true);
                }
                else
                {
                    results.status = gjk_status == GJK.eStatus.Inside ?
                        sResults.eStatus.Penetrating :
                    sResults.eStatus.GJK_Failed;
                    return (false);
                }
            }
        }

        public static bool Penetration(ConvexShape shape0, btTransform wtrs0, ConvexShape shape1, btTransform wtrs1, btVector3 guess, ref sResults results)
        {
            return Penetration(shape0, wtrs0, shape1, wtrs1, guess, ref results, true);
        }
        public static bool Penetration(ConvexShape shape0, btTransform wtrs0, ConvexShape shape1, btTransform wtrs1, btVector3 guess, ref sResults results, bool usemargins)
        {
            tShape			shape=new MinkowskiDiff();
	        Initialize(shape0,wtrs0,shape1,wtrs1,ref results,ref shape,usemargins);
            using (GJK gjk = GJK.CreateFromPool())
            {
                GJK.eStatus gjk_status = gjk.Evaluate(shape, -guess);
                switch (gjk_status)
                {
                    case GJK.eStatus.Inside:
                        {
                            using(EPA epa = EPA.CreateFromPool())
                            {
                                EPA.eStatus epa_status = epa.Evaluate(gjk, -guess);
                                if (epa_status != EPA.eStatus.Failed)
                                {
                                    btVector3 w0 = new btVector3(0, 0, 0);
                                    for (U i = 0; i < epa.m_result.rank; ++i)
                                    {
                                        #region w0 += shape.Support(epa.m_result.c[i].d, 0) * epa.m_result.p[i];
                                        {
                                            btVector3 temp1, temp2;
                                            shape.Support(ref epa.m_result.c[i].d, 0, out temp1);
                                            btVector3.Multiply(ref temp1, epa.m_result.p[i], out temp2);
                                            w0.Add(ref temp2);
                                        }
                                        #endregion
                                    }
                                    results.status = sResults.eStatus.Penetrating;
                                    results.witnesses0 = wtrs0 * w0;
                                    results.witnesses1 = wtrs0 * (w0 - epa.m_normal * epa.m_depth);
                                    results.normal = -epa.m_normal;
                                    results.distance = -epa.m_depth;
                                    return (true);
                                }
                                else results.status = sResults.eStatus.EPA_Failed;
                            }
                        }
                        break;
                    case GJK.eStatus.Failed:
                        results.status = sResults.eStatus.GJK_Failed;
                        break;
                    default:
                        {
                            break;
                        }
                }
                return (false);
            }
            
        }
        static void Initialize(ConvexShape shape0, btTransform wtrs0,
            ConvexShape shape1, btTransform wtrs1,
            ref GjkEpaSolver2.sResults results,
            ref tShape shape,
            bool withmargins)
        {
            /* Results		*/
            results.witnesses0 =
                results.witnesses1 = new btVector3(0, 0, 0);
            results.status = GjkEpaSolver2.sResults.eStatus.Separated;
            /* Shape		*/
            shape.m_shapes0 = shape0;
            shape.m_shapes1 = shape1;
            //shape.m_toshape1		=	wtrs1.Basis.transposeTimes(wtrs0.Basis);
            wtrs1.Basis.transposeTimes(ref wtrs0.Basis, out shape.m_toshape1);
            shape.m_toshape0 = wtrs0.inverseTimes(wtrs1);
            shape.EnableMargin(withmargins);
        }
    }
}
