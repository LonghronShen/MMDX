using System.Diagnostics;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    class VoronoiSimplexSolver : ISimplexSolver
    {
        public const float VORONOI_DEFAULT_EQUAL_VERTEX_THRESHOLD = 0.0001f;
        public const int VORONOI_SIMPLEX_MAX_VERTS = 5;
        const int VERTA = 0;
        const int VERTB = 1;
        const int VERTC = 2;
        const int VERTD = 3;


        int m_numVertices;
        public int numVertices { get { return m_numVertices; } }

        btVector3[] m_simplexVectorW = new btVector3[VORONOI_SIMPLEX_MAX_VERTS];
        btVector3[] m_simplexPointsP = new btVector3[VORONOI_SIMPLEX_MAX_VERTS];
        btVector3[] m_simplexPointsQ = new btVector3[VORONOI_SIMPLEX_MAX_VERTS];



        btVector3 m_cachedP1;
        btVector3 m_cachedP2;
        btVector3 m_cachedV;
        btVector3 m_lastW;

        public float m_equalVertexThreshold;
        bool m_cachedValidClosest;

        SubSimplexClosestResult m_cachedBC = new SubSimplexClosestResult();

        bool m_needsUpdate;

        public VoronoiSimplexSolver()
        {
            m_equalVertexThreshold = VORONOI_DEFAULT_EQUAL_VERTEX_THRESHOLD;
        }

        #region ISimplexSolver メンバ

        public void reset()
        {
            m_cachedValidClosest = false;
            m_numVertices = 0;
            m_needsUpdate = true;
            m_lastW = new btVector3(BulletGlobal.BT_LARGE_FLOAT, BulletGlobal.BT_LARGE_FLOAT, BulletGlobal.BT_LARGE_FLOAT);
            m_cachedBC.reset();
        }

        public bool inSimplex(btVector3 w)
        {
            bool found = false;
            int i, numverts = numVertices;
            //btScalar maxV = btScalar(0.);

            //w is in the current (reduced) simplex
            for (i = 0; i < numverts; i++)
            {
                if (m_simplexVectorW[i].distance2(w) <= m_equalVertexThreshold)
                    found = true;
            }

            //check in case lastW is already removed
            if (btVector3.Equals(ref w, ref m_lastW))
                return true;

            return found;
        }

        public void addVertex(btVector3 w, btVector3 p, btVector3 q)
        {
            m_lastW = w;
            m_needsUpdate = true;

            m_simplexVectorW[m_numVertices] = w;
            m_simplexPointsP[m_numVertices] = p;
            m_simplexPointsQ[m_numVertices] = q;

            m_numVertices++;
        }

        public bool closest(out btVector3 v)
        {
            bool succes = updateClosestVectorAndPoints();
            v = m_cachedV;
            return succes;
        }

        public void backup_closest(ref btVector3 v)
        {
            v = m_cachedV;
        }

        public bool fullSimplex { get { return (m_numVertices == 4); } }

        public void compute_points(out btVector3 p1, out btVector3 p2)
        {
            updateClosestVectorAndPoints();
            p1 = m_cachedP1;
            p2 = m_cachedP2;
        }

        #endregion
        bool updateClosestVectorAndPoints()
        {

            if (m_needsUpdate)
            {
                m_cachedBC.reset();

                m_needsUpdate = false;

                switch (numVertices)
                {
                    case 0:
                        m_cachedValidClosest = false;
                        break;
                    case 1:
                        {
                            m_cachedP1 = m_simplexPointsP[0];
                            m_cachedP2 = m_simplexPointsQ[0];
                            m_cachedV = m_cachedP1 - m_cachedP2; //== m_simplexVectorW[0]
                            m_cachedBC.reset();
                            m_cachedBC.setBarycentricCoordinates(1f, 0f, 0f, 0f);
                            m_cachedValidClosest = m_cachedBC.isValid();
                            break;
                        };
                    case 2:
                        {
                            //closest point origin from line segment
                            btVector3 nearest;

                            btVector3 p = btVector3.Zero;//(btScalar(0.),btScalar(0.),btScalar(0.));
                            btVector3 diff = p - m_simplexVectorW[0];
                            btVector3 v = m_simplexVectorW[1] - m_simplexVectorW[0];
                            float t = v.dot(diff);

                            if (t > 0)
                            {
                                float dotVV = v.dot(v);
                                if (t < dotVV)
                                {
                                    t /= dotVV;
                                    diff -= t * v;
                                    m_cachedBC.m_usedVertices.usedVertexA = true;
                                    m_cachedBC.m_usedVertices.usedVertexB = true;
                                }
                                else
                                {
                                    t = 1;
                                    diff -= v;
                                    //reduce to 1 point
                                    m_cachedBC.m_usedVertices.usedVertexB = true;
                                }
                            }
                            else
                            {
                                t = 0;
                                //reduce to 1 point
                                m_cachedBC.m_usedVertices.usedVertexA = true;
                            }
                            m_cachedBC.setBarycentricCoordinates(1 - t, t);
                            #region nearest = m_simplexVectorW[0] + t * v;
                            {
                                btVector3 temp;
                                btVector3.Multiply(ref v, t, out temp);
                                btVector3.Add(ref m_simplexVectorW[0], ref temp, out nearest);
                            }
                            #endregion

                            #region m_cachedP1 = m_simplexPointsP[0] + t * (m_simplexPointsP[1] - m_simplexPointsP[0]);
                            {
                                btVector3 temp1, temp2;
                                btVector3.Subtract(ref m_simplexPointsP[1], ref m_simplexPointsP[0], out temp1);
                                btVector3.Multiply(ref temp1, t, out temp2);
                                btVector3.Add(ref m_simplexPointsP[0], ref temp2, out m_cachedP1);
                            }
                            #endregion
                            #region m_cachedP2 = m_simplexPointsQ[0] + t * (m_simplexPointsQ[1] - m_simplexPointsQ[0]);
                            {
                                btVector3 temp1, temp2;
                                btVector3.Subtract(ref m_simplexPointsQ[1], ref m_simplexPointsQ[0], out temp1);
                                btVector3.Multiply(ref temp1, t, out temp2);
                                btVector3.Add(ref m_simplexPointsQ[0], ref temp2, out m_cachedP2);
                            }
                            #endregion
                            m_cachedV = m_cachedP1 - m_cachedP2;

                            reduceVertices(ref m_cachedBC.m_usedVertices);

                            m_cachedValidClosest = m_cachedBC.isValid();
                            break;
                        }
                    case 3:
                        {
                            //closest point origin from triangle 
                            btVector3 p = btVector3.Zero;//(btScalar(0.),btScalar(0.),btScalar(0.)); 

                            closestPtPointTriangle(ref p, ref m_simplexVectorW[0], ref m_simplexVectorW[1], ref m_simplexVectorW[2], m_cachedBC);
                            m_cachedP1 = m_simplexPointsP[0] * m_cachedBC.m_barycentricCoords[0] +
                            m_simplexPointsP[1] * m_cachedBC.m_barycentricCoords[1] +
                            m_simplexPointsP[2] * m_cachedBC.m_barycentricCoords[2];

                            m_cachedP2 = m_simplexPointsQ[0] * m_cachedBC.m_barycentricCoords[0] +
                            m_simplexPointsQ[1] * m_cachedBC.m_barycentricCoords[1] +
                            m_simplexPointsQ[2] * m_cachedBC.m_barycentricCoords[2];

                            m_cachedV = m_cachedP1 - m_cachedP2;

                            reduceVertices(ref m_cachedBC.m_usedVertices);
                            m_cachedValidClosest = m_cachedBC.isValid();

                            break;
                        }
                    case 4:
                        {


                            btVector3 p = btVector3.Zero;// (btScalar(0.),btScalar(0.),btScalar(0.));


                            bool hasSeperation = closestPtPointTetrahedron(ref p, ref m_simplexVectorW[0], ref m_simplexVectorW[1], ref m_simplexVectorW[2], ref m_simplexVectorW[3], m_cachedBC);

                            if (hasSeperation)
                            {

                                m_cachedP1 = m_simplexPointsP[0] * m_cachedBC.m_barycentricCoords[0] +
                                    m_simplexPointsP[1] * m_cachedBC.m_barycentricCoords[1] +
                                    m_simplexPointsP[2] * m_cachedBC.m_barycentricCoords[2] +
                                    m_simplexPointsP[3] * m_cachedBC.m_barycentricCoords[3];

                                m_cachedP2 = m_simplexPointsQ[0] * m_cachedBC.m_barycentricCoords[0] +
                                    m_simplexPointsQ[1] * m_cachedBC.m_barycentricCoords[1] +
                                    m_simplexPointsQ[2] * m_cachedBC.m_barycentricCoords[2] +
                                    m_simplexPointsQ[3] * m_cachedBC.m_barycentricCoords[3];

                                m_cachedV = m_cachedP1 - m_cachedP2;
                                reduceVertices(ref m_cachedBC.m_usedVertices);
                            }
                            else
                            {
                                //					printf("sub distance got penetration\n");

                                if (m_cachedBC.m_degenerate)
                                {
                                    m_cachedValidClosest = false;
                                }
                                else
                                {
                                    m_cachedValidClosest = true;
                                    //degenerate case == false, penetration = true + zero
                                    m_cachedV.setValue(0f, 0f, 0f);
                                }
                                break;
                            }

                            m_cachedValidClosest = m_cachedBC.isValid();

                            //closest point origin from tetrahedron
                            break;
                        }
                    default:
                        {
                            m_cachedValidClosest = false;
                        }
                        break;
                };
            }

            return m_cachedValidClosest;

        }
        void reduceVertices(ref UsageBitfield usedVerts)
        {
            if ((numVertices >= 4) && (!usedVerts.usedVertexD))
                removeVertex(3);

            if ((numVertices >= 3) && (!usedVerts.usedVertexC))
                removeVertex(2);

            if ((numVertices >= 2) && (!usedVerts.usedVertexB))
                removeVertex(1);

            if ((numVertices >= 1) && (!usedVerts.usedVertexA))
                removeVertex(0);

        }
        void removeVertex(int index)
        {

            Debug.Assert(m_numVertices > 0);
            m_numVertices--;
            m_simplexVectorW[index] = m_simplexVectorW[m_numVertices];
            m_simplexPointsP[index] = m_simplexPointsP[m_numVertices];
            m_simplexPointsQ[index] = m_simplexPointsQ[m_numVertices];
        }
        bool closestPtPointTriangle(ref btVector3 p, ref btVector3 a, ref btVector3 b, ref btVector3 c, SubSimplexClosestResult result)
        {
            result.m_usedVertices.reset();

            // Check if P in vertex region outside A
            btVector3 ab = b - a;
            btVector3 ac = c - a;
            btVector3 ap = p - a;
            float d1 = ab.dot(ap);
            float d2 = ac.dot(ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                result.m_closestPointOnSimplex = a;
                result.m_usedVertices.usedVertexA = true;
                result.setBarycentricCoordinates(1, 0, 0);
                return true;// a; // barycentric coordinates (1,0,0)
            }

            // Check if P in vertex region outside B
            btVector3 bp = p - b;
            float d3 = ab.dot(bp);
            float d4 = ac.dot(bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                result.m_closestPointOnSimplex = b;
                result.m_usedVertices.usedVertexB = true;
                result.setBarycentricCoordinates(0, 1, 0);

                return true; // b; // barycentric coordinates (0,1,0)
            }
            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                result.m_closestPointOnSimplex = a + v * ab;
                result.m_usedVertices.usedVertexA = true;
                result.m_usedVertices.usedVertexB = true;
                result.setBarycentricCoordinates(1 - v, v, 0);
                return true;
                //return a + v * ab; // barycentric coordinates (1-v,v,0)
            }

            // Check if P in vertex region outside C
            btVector3 cp = p - c;
            float d5 = ab.dot(cp);
            float d6 = ac.dot(cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                result.m_closestPointOnSimplex = c;
                result.m_usedVertices.usedVertexC = true;
                result.setBarycentricCoordinates(0, 0, 1);
                return true;//c; // barycentric coordinates (0,0,1)
            }

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                result.m_closestPointOnSimplex = a + w * ac;
                result.m_usedVertices.usedVertexA = true;
                result.m_usedVertices.usedVertexC = true;
                result.setBarycentricCoordinates(1 - w, 0, w);
                return true;
                //return a + w * ac; // barycentric coordinates (1-w,0,w)
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));

                result.m_closestPointOnSimplex = b + w * (c - b);
                result.m_usedVertices.usedVertexB = true;
                result.m_usedVertices.usedVertexC = true;
                result.setBarycentricCoordinates(0, 1 - w, w);
                return true;
                // return b + w * (c - b); // barycentric coordinates (0,1-w,w)
            }
            {
                // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
                float denom = 1.0f / (va + vb + vc);
                float v = vb * denom;
                float w = vc * denom;

                result.m_closestPointOnSimplex = a + ab * v + ac * w;
                result.m_usedVertices.usedVertexA = true;
                result.m_usedVertices.usedVertexB = true;
                result.m_usedVertices.usedVertexC = true;
                result.setBarycentricCoordinates(1 - v - w, v, w);
            }
            return true;
            //	return a + ab * v + ac * w; // = u*a + v*b + w*c, u = va * denom = btScalar(1.0) - v - w

        }
        SubSimplexClosestResult tempResult = new SubSimplexClosestResult();
        bool closestPtPointTetrahedron(ref btVector3 p, ref btVector3 a, ref btVector3 b, ref btVector3 c, ref btVector3 d, SubSimplexClosestResult finalResult)
        {
            tempResult.reset();

            // Start out assuming point inside all halfspaces, so closest to itself
            finalResult.m_closestPointOnSimplex = p;
            finalResult.m_usedVertices.reset();
            finalResult.m_usedVertices.usedVertexA = true;
            finalResult.m_usedVertices.usedVertexB = true;
            finalResult.m_usedVertices.usedVertexC = true;
            finalResult.m_usedVertices.usedVertexD = true;

            int pointOutsideABC = pointOutsideOfPlane(ref p, ref a, ref b, ref c, ref d);
            int pointOutsideACD = pointOutsideOfPlane(ref p, ref a, ref c, ref d, ref b);
            int pointOutsideADB = pointOutsideOfPlane(ref p, ref a, ref d, ref b, ref c);
            int pointOutsideBDC = pointOutsideOfPlane(ref p, ref b, ref d, ref c, ref a);

            if (pointOutsideABC < 0 || pointOutsideACD < 0 || pointOutsideADB < 0 || pointOutsideBDC < 0)
            {
                finalResult.m_degenerate = true;
                return false;
            }

            if (pointOutsideABC == 0 && pointOutsideACD == 0 && pointOutsideADB == 0 && pointOutsideBDC == 0)
            {
                return false;
            }


            float bestSqDist = float.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (pointOutsideABC != 0)
            {
                closestPtPointTriangle(ref p, ref a, ref b, ref  c, tempResult);
                btVector3 q = tempResult.m_closestPointOnSimplex;

                float sqDist = (q - p).dot(q - p);
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    finalResult.m_closestPointOnSimplex = q;
                    //convert result bitmask!
                    finalResult.m_usedVertices.reset();
                    finalResult.m_usedVertices.usedVertexA = tempResult.m_usedVertices.usedVertexA;
                    finalResult.m_usedVertices.usedVertexB = tempResult.m_usedVertices.usedVertexB;
                    finalResult.m_usedVertices.usedVertexC = tempResult.m_usedVertices.usedVertexC;
                    finalResult.setBarycentricCoordinates(
                            tempResult.m_barycentricCoords[VERTA],
                            tempResult.m_barycentricCoords[VERTB],
                            tempResult.m_barycentricCoords[VERTC],
                            0
                    );

                }
            }


            // Repeat test for face acd
            if (pointOutsideACD != 0)
            {
                closestPtPointTriangle(ref p, ref a, ref c, ref d, tempResult);
                btVector3 q = tempResult.m_closestPointOnSimplex;
                //convert result bitmask!

                float sqDist = (q - p).dot(q - p);
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    finalResult.m_closestPointOnSimplex = q;
                    finalResult.m_usedVertices.reset();
                    finalResult.m_usedVertices.usedVertexA = tempResult.m_usedVertices.usedVertexA;

                    finalResult.m_usedVertices.usedVertexC = tempResult.m_usedVertices.usedVertexB;
                    finalResult.m_usedVertices.usedVertexD = tempResult.m_usedVertices.usedVertexC;
                    finalResult.setBarycentricCoordinates(
                            tempResult.m_barycentricCoords[VERTA],
                            0,
                            tempResult.m_barycentricCoords[VERTB],
                            tempResult.m_barycentricCoords[VERTC]
                    );

                }
            }
            // Repeat test for face adb


            if (pointOutsideADB != 0)
            {
                closestPtPointTriangle(ref p, ref a, ref d, ref b, tempResult);
                btVector3 q = tempResult.m_closestPointOnSimplex;
                //convert result bitmask!

                float sqDist = (q - p).dot(q - p);
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    finalResult.m_closestPointOnSimplex = q;
                    finalResult.m_usedVertices.reset();
                    finalResult.m_usedVertices.usedVertexA = tempResult.m_usedVertices.usedVertexA;
                    finalResult.m_usedVertices.usedVertexB = tempResult.m_usedVertices.usedVertexC;

                    finalResult.m_usedVertices.usedVertexD = tempResult.m_usedVertices.usedVertexB;
                    finalResult.setBarycentricCoordinates(
                            tempResult.m_barycentricCoords[VERTA],
                            tempResult.m_barycentricCoords[VERTC],
                            0,
                            tempResult.m_barycentricCoords[VERTB]
                    );

                }
            }
            // Repeat test for face bdc


            if (pointOutsideBDC != 0)
            {
                closestPtPointTriangle(ref p, ref b, ref d, ref c, tempResult);
                btVector3 q = tempResult.m_closestPointOnSimplex;
                //convert result bitmask!
                float sqDist = (q - p).dot(q - p);
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    finalResult.m_closestPointOnSimplex = q;
                    finalResult.m_usedVertices.reset();
                    //
                    finalResult.m_usedVertices.usedVertexB = tempResult.m_usedVertices.usedVertexA;
                    finalResult.m_usedVertices.usedVertexC = tempResult.m_usedVertices.usedVertexC;
                    finalResult.m_usedVertices.usedVertexD = tempResult.m_usedVertices.usedVertexB;

                    finalResult.setBarycentricCoordinates(
                            0,
                            tempResult.m_barycentricCoords[VERTA],
                            tempResult.m_barycentricCoords[VERTC],
                            tempResult.m_barycentricCoords[VERTB]
                    );

                }
            }

            //help! we ended up full !

            if (finalResult.m_usedVertices.usedVertexA &&
                finalResult.m_usedVertices.usedVertexB &&
                finalResult.m_usedVertices.usedVertexC &&
                finalResult.m_usedVertices.usedVertexD)
            {
                return true;
            }

            return true;
        }
        /// Test if point p and d lie on opposite sides of plane through abc
        int pointOutsideOfPlane(ref btVector3 p, ref btVector3 a, ref btVector3 b, ref btVector3 c, ref btVector3 d)
        {
            btVector3 normal = (b - a).cross(c - a);

            float signp = (p - a).dot(normal); // [AP AB AC]
            float signd = (d - a).dot(normal); // [AD AB AC]

            if (signd * signd < (1e-4f * 1e-4f))
            {
                //		printf("affine dependent/degenerate\n");//
                return -1;
            }
            // Points on opposite sides if expression signs are opposite
            return signp * signd < 0f ? 1 : 0;
        }
    }
}
