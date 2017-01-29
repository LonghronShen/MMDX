using System;
using System.Diagnostics;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;
namespace BulletX.BulletCollision.CollisionDispatch
{
    struct BoxBoxDetector
    {
        BoxShape m_box1;
        BoxShape m_box2;


        public BoxBoxDetector(BoxShape box1, BoxShape box2)
        {
            m_box1 = box1;
            m_box2 = box2;
        }
        public void getClosestPoints(ref ClosestPointInput input, ref ManifoldResult output, IDebugDraw debugDraw)
        {

            //float* R1 = stackalloc float[12];
            //float* R2 = stackalloc float[12];
            StackPtr<float> R1 = StackPtr<float>.Allocate(12);
            StackPtr<float> R2 = StackPtr<float>.Allocate(12);

            try
            {
                for (int j = 0; j < 3; j++)
                {
                    R1[0 + 4 * j] = input.m_transformA.Basis[j].X;
                    R2[0 + 4 * j] = input.m_transformB.Basis[j].X;

                    R1[1 + 4 * j] = input.m_transformA.Basis[j].Y;
                    R2[1 + 4 * j] = input.m_transformB.Basis[j].Y;


                    R1[2 + 4 * j] = input.m_transformA.Basis[j].Z;
                    R2[2 + 4 * j] = input.m_transformB.Basis[j].Z;

                }



                btVector3 normal;
                float depth;
                int return_code;
                int maxc = 4;

                dBoxBox2(input.m_transformA.Origin,
                R1,
                m_box1.HalfExtentsWithMargin * 2f,
                input.m_transformB.Origin,
                R2,
                m_box2.HalfExtentsWithMargin * 2f,
                out normal, out depth, out return_code,
                maxc,
                ref output
                );
            }
            finally
            {
                R1.Dispose();
                R2.Dispose();
            }
        }
        //#define dDOTpq(a,b,p,q) ((a)[0]*(b)[0] + (a)[p]*(b)[q] + (a)[2*(p)]*(b)[2*(q)])
        /*static unsafe float dDOT   (float *a, float *b) { return dDOTpq(a,b,1,1); }
        static unsafe float dDOT44 (float *a, float *b) { return dDOTpq(a,b,4,4); }
        static unsafe float dDOT41 (float *a, float *b) { return dDOTpq(a,b,4,1); }
        static unsafe float dDOT14 (float *a, float *b) { return dDOTpq(a,b,1,4); }
        */
        static unsafe float dDOT(float* a, float* b) { return ((a)[0] * (b)[0] + (a)[1] * (b)[1] + (a)[2 * (1)] * (b)[2 * (1)]); }
        static unsafe float dDOT(float* a, ref btVector3 b) { return ((a)[0] * (b).X + (a)[1] * (b).Y + (a)[2 * (1)] * (b).Z); }
        static unsafe float dDOT(ref btVector3 a, float* b) { return ((a).X * (b)[0] + (a).Y * (b)[1] + (a).Z * (b)[2 * (1)]); }
        static unsafe float dDOT(ref btVector3 a, ref btVector3 b) { return ((a).X * (b).X + (a).Y * (b).Y + (a).Z * (b).Z); }
        static unsafe float dDOT44(float* a, float* b) { return ((a)[0] * (b)[0] + (a)[4] * (b)[4] + (a)[2 * (4)] * (b)[2 * (4)]); }
        static unsafe float dDOT41(float* a, float* b) { return ((a)[0] * (b)[0] + (a)[4] * (b)[1] + (a)[2 * (4)] * (b)[2 * (1)]); }
        static unsafe float dDOT41(float* a, ref btVector3 b) { return ((a)[0] * (b).X + (a)[4] * (b).Y + (a)[2 * (4)] * (b).Z); }
        static unsafe float dDOT14(float* a, float* b) { return ((a)[0] * (b)[0] + (a)[1] * (b)[4] + (a)[2 * (1)] * (b)[2 * (4)]); }
        static unsafe float dDOT14(ref btVector3 a, float* b) { return ((a).X * (b)[0] + (a).Y * (b)[4] + (a).Z * (b)[2 * (4)]); }
        static unsafe void dLineClosestApproach(ref btVector3 pa, ref btVector3 ua,
               ref btVector3 pb, ref btVector3 ub,
               float* alpha, float* beta)
        {
            btVector3 p = new btVector3(pb.X - pa.X, pb.Y - pa.Y, pb.Z - pa.Z);
            float uaub = dDOT(ref ua, ref ub);
            float q1 = dDOT(ref ua, ref p);
            float q2 = -dDOT(ref ub, ref p);
            float d = 1 - uaub * uaub;
            if (d <= 0.0001f)
            {
                // @@@ this needs to be made more robust
                *alpha = 0;
                *beta = 0;
            }
            else
            {
                d = 1f / d;
                *alpha = (q1 + uaub * q2) * d;
                *beta = (uaub * q1 + q2) * d;
            }
        }

        static unsafe int dBoxBox2(btVector3 p1, float[] R1safe, btVector3 side1, btVector3 p2, float[] R2safe, btVector3 side2,
            out btVector3 normal, out float depth, out int return_code, int maxc, ref ManifoldResult output)
        {
            fixed (float* R1 = &R1safe[0], R2 = &R2safe[0])
            {
                float fudge_factor = 1.05f;
                btVector3 p, pp, normalC = new btVector3(0f, 0f, 0f);
                float* normalR = null;
                //float* A = stackalloc float[3], B = stackalloc float[3];
                StackPtr<float> A = StackPtr<float>.Allocate(3);
                StackPtr<float> B = StackPtr<float>.Allocate(3);
                try
                {
                    float R11, R12, R13, R21, R22, R23, R31, R32, R33,
                            Q11, Q12, Q13, Q21, Q22, Q23, Q31, Q32, Q33, s, s2, l;
                    int i, j, code;
                    bool invert_normal;

                    normal = btVector3.Zero;
                    depth = 0;
                    return_code = -1;

                    // get vector from centers of box 1 to box 2, relative to box 1
                    p = p2 - p1;

                    pp = new btVector3(dDOT41((R1), ref p), dDOT41((R1 + 1), ref p), dDOT41((R1 + 2), ref p));
                    //dMULTIPLYOP1_331 (pp,=,R1,p);		// get pp = p relative to body 1

                    // get side lengths / 2
                    A[0] = side1.X * 0.5f;
                    A[1] = side1.Y * 0.5f;
                    A[2] = side1.Z * 0.5f;
                    B[0] = side2.X * 0.5f;
                    B[1] = side2.Y * 0.5f;
                    B[2] = side2.Z * 0.5f;

                    // Rij is R1'*R2, i.e. the relative rotation between R1 and R2
                    R11 = dDOT44(R1 + 0, R2 + 0); R12 = dDOT44(R1 + 0, R2 + 1); R13 = dDOT44(R1 + 0, R2 + 2);
                    R21 = dDOT44(R1 + 1, R2 + 0); R22 = dDOT44(R1 + 1, R2 + 1); R23 = dDOT44(R1 + 1, R2 + 2);
                    R31 = dDOT44(R1 + 2, R2 + 0); R32 = dDOT44(R1 + 2, R2 + 1); R33 = dDOT44(R1 + 2, R2 + 2);

                    Q11 = (float)Math.Abs(R11); Q12 = (float)Math.Abs(R12); Q13 = (float)Math.Abs(R13);
                    Q21 = (float)Math.Abs(R21); Q22 = (float)Math.Abs(R22); Q23 = (float)Math.Abs(R23);
                    Q31 = (float)Math.Abs(R31); Q32 = (float)Math.Abs(R32); Q33 = (float)Math.Abs(R33);

                    // for all 15 possible separating axes:
                    //   * see if the axis separates the boxes. if so, return 0.
                    //   * find the depth of the penetration along the separating axis (s2)
                    //   * if this is the largest depth so far, record it.
                    // the normal vector will be set to the separating axis with the smallest
                    // depth. note: normalR is set to point to a column of R1 or R2 if that is
                    // the smallest depth normal so far. otherwise normalR is 0 and normalC is
                    // set to a vector relative to body 1. invert_normal is 1 if the sign of
                    // the normal should be flipped.
                    {
                        //TSTマクロ有効域1
                        /*TST(expr1,expr2,norm,cc)
                        {
                            s2 = (float)Math.Abs(expr1) - (expr2); 
                              if (s2 > 0) return 0; 
                              if (s2 > s) { 
                                s = s2; 
                                normalR = norm; 
                                invert_normal = ((expr1) < 0); 
                                code = (cc); 
                              }
                        }*/

                        s = float.MinValue;
                        invert_normal = false;
                        code = 0;

                        // separating axis = u1,u2,u3
                        //TST(pp[0], (A[0] + B[0] * Q11 + B[1] * Q12 + B[2] * Q13), R1 + 0, 1);
                        {
                            s2 = (float)Math.Abs(pp.X) - ((A[0] + B[0] * Q11 + B[1] * Q12 + B[2] * Q13));
                            if (s2 > 0) return 0;
                            if (s2 > s)
                            {
                                s = s2;
                                normalR = R1;
                                invert_normal = ((pp.X) < 0);
                                code = (1);
                            }
                        }
                        //TST(pp[1], (A[1] + B[0] * Q21 + B[1] * Q22 + B[2] * Q23), R1 + 1, 2);
                        {
                            s2 = (float)Math.Abs(pp.Y) - ((A[1] + B[0] * Q21 + B[1] * Q22 + B[2] * Q23));
                            if (s2 > 0) return 0;
                            if (s2 > s)
                            {
                                s = s2;
                                normalR = R1 + 1;
                                invert_normal = ((pp.Y) < 0);
                                code = (2);
                            }
                        }
                        //TST(pp[2], (A[2] + B[0] * Q31 + B[1] * Q32 + B[2] * Q33), R1 + 2, 3);
                        {
                            s2 = (float)Math.Abs(pp.Z) - ((A[2] + B[0] * Q31 + B[1] * Q32 + B[2] * Q33));
                            if (s2 > 0) return 0;
                            if (s2 > s)
                            {
                                s = s2;
                                normalR = R1 + 2;
                                invert_normal = ((pp.Z) < 0);
                                code = (3);
                            }
                        }

                        // separating axis = v1,v2,v3
                        //TST(dDOT41(R2 + 0, ref p), (A[0] * Q11 + A[1] * Q21 + A[2] * Q31 + B[0]), R2 + 0, 4);
                        {
                            s2 = (float)Math.Abs(dDOT41(R2 + 0, ref p)) - ((A[0] * Q11 + A[1] * Q21 + A[2] * Q31 + B[0]));
                            if (s2 > 0) return 0;
                            if (s2 > s)
                            {
                                s = s2;
                                normalR = R2;
                                invert_normal = ((dDOT41(R2 + 0, ref p)) < 0);
                                code = (4);
                            }
                        }
                        //TST(dDOT41(R2 + 1, ref p), (A[0] * Q12 + A[1] * Q22 + A[2] * Q32 + B[1]), R2 + 1, 5);
                        {
                            s2 = (float)Math.Abs(dDOT41(R2 + 1, ref p)) - ((A[0] * Q12 + A[1] * Q22 + A[2] * Q32 + B[1]));
                            if (s2 > 0) return 0;
                            if (s2 > s)
                            {
                                s = s2;
                                normalR = R2 + 1;
                                invert_normal = ((dDOT41(R2 + 1, ref p)) < 0);
                                code = (5);
                            }
                        }
                        //TST(dDOT41(R2 + 2, ref p), (A[0] * Q13 + A[1] * Q23 + A[2] * Q33 + B[2]), R2 + 2, 6);
                        {
                            s2 = (float)Math.Abs(dDOT41(R2 + 2, ref p)) - ((A[0] * Q13 + A[1] * Q23 + A[2] * Q33 + B[2]));
                            if (s2 > 0) return 0;
                            if (s2 > s)
                            {
                                s = s2;
                                normalR = R2 + 2;
                                invert_normal = ((dDOT41(R2 + 2, ref p)) < 0);
                                code = (6);
                            }
                        }

                        // note: cross product axes need to be scaled when s is computed.
                        // normal (n1,n2,n3) is relative to box 1.
                    }
                    {
                        /*TST(expr1,expr2,n1,n2,n3,cc) 
                        {
                            s2 = (float)Math.Abs(expr1) - (expr2); 
                            if (s2 > SIMD_EPSILON) return 0; 
                            l = (float)Math.Sqrt((n1)*(n1) + (n2)*(n2) + (n3)*(n3));
                            if (l >  SIMD_EPSILON) {
                            s2 /= l; 
                            if (s2*fudge_factor > s) { 
                              s = s2; 
                              normalR = null; 
                              normalC[0] = (n1)/l; normalC[1] = (n2)/l; normalC[2] = (n3)/l; 
                              invert_normal = ((expr1) < 0); 
                              code = (cc); 
                            }
                            }
                         }*/


                        // separating axis = u1 x (v1,v2,v3)
                        //TST(pp[2] * R21 - pp[1] * R31, (A[1] * Q31 + A[2] * Q21 + B[1] * Q13 + B[2] * Q12), 0, -R31, R21, 7);
                        {
                            s2 = (float)Math.Abs(pp.Z * R21 - pp.Y * R31) - ((A[1] * Q31 + A[2] * Q21 + B[1] * Q13 + B[2] * Q12));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((0) * (0) + (-R31) * (-R31) + (R21) * (R21));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (0) / l; normalC.Y = (-R31) / l; normalC.Z = (R21) / l;
                                    invert_normal = ((pp.Z * R21 - pp.Y * R31) < 0);
                                    code = (7);
                                }
                            }
                        }
                        //TST(pp[2] * R22 - pp[1] * R32, (A[1] * Q32 + A[2] * Q22 + B[0] * Q13 + B[2] * Q11), 0, -R32, R22, 8);
                        {
                            s2 = (float)Math.Abs(pp.Z * R22 - pp.Y * R32) - ((A[1] * Q32 + A[2] * Q22 + B[0] * Q13 + B[2] * Q11));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((0) * (0) + (-R32) * (-R32) + (R22) * (R22));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (0) / l; normalC.Y = (-R32) / l; normalC.Z = (R22) / l;
                                    invert_normal = ((pp.Z * R22 - pp.Y * R32) < 0);
                                    code = (8);
                                }
                            }
                        }
                        //TST(pp[2] * R23 - pp[1] * R33, (A[1] * Q33 + A[2] * Q23 + B[0] * Q12 + B[1] * Q11), 0, -R33, R23, 9);
                        {
                            s2 = (float)Math.Abs(pp.Z * R23 - pp.Y * R33) - ((A[1] * Q33 + A[2] * Q23 + B[0] * Q12 + B[1] * Q11));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((0) * (0) + (-R33) * (-R33) + (R23) * (R23));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (0) / l; normalC.Y = (-R33) / l; normalC.Z = (R23) / l;
                                    invert_normal = ((pp.Z * R23 - pp.Y * R33) < 0);
                                    code = (9);
                                }
                            }
                        }

                        // separating axis = u2 x (v1,v2,v3)
                        //TST(pp[0] * R31 - pp[2] * R11, (A[0] * Q31 + A[2] * Q11 + B[1] * Q23 + B[2] * Q22), R31, 0, -R11, 10);
                        {
                            s2 = (float)Math.Abs(pp.X * R31 - pp.Z * R11) - ((A[0] * Q31 + A[2] * Q11 + B[1] * Q23 + B[2] * Q22));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((R31) * (R31) + (0) * (0) + (-R11) * (-R11));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (R31) / l; normalC.Y = (0) / l; normalC.Z = (-R11) / l;
                                    invert_normal = ((pp.X * R31 - pp.Z * R11) < 0);
                                    code = (10);
                                }
                            }
                        }
                        //TST(pp[0] * R32 - pp[2] * R12, (A[0] * Q32 + A[2] * Q12 + B[0] * Q23 + B[2] * Q21), R32, 0, -R12, 11);
                        {
                            s2 = (float)Math.Abs(pp.X * R32 - pp.Z * R12) - ((A[0] * Q32 + A[2] * Q12 + B[0] * Q23 + B[2] * Q21));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((R32) * (R32) + (0) * (0) + (-R12) * (-R12));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (R32) / l; normalC.Y = (0) / l; normalC.Z = (-R12) / l;
                                    invert_normal = ((pp.X * R32 - pp.Z * R12) < 0);
                                    code = (11);
                                }
                            }
                        }
                        //TST(pp[0] * R33 - pp[2] * R13, (A[0] * Q33 + A[2] * Q13 + B[0] * Q22 + B[1] * Q21), R33, 0, -R13, 12);
                        {
                            s2 = (float)Math.Abs(pp.X * R33 - pp.Z * R13) - ((A[0] * Q33 + A[2] * Q13 + B[0] * Q22 + B[1] * Q21));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((R33) * (R33) + (0) * (0) + (-R13) * (-R13));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (R33) / l; normalC.Y = (0) / l; normalC.Z = (-R13) / l;
                                    invert_normal = ((pp.X * R33 - pp.Z * R13) < 0);
                                    code = (12);
                                }
                            }
                        }

                        // separating axis = u3 x (v1,v2,v3)
                        //TST(pp[1] * R11 - pp[0] * R21, (A[0] * Q21 + A[1] * Q11 + B[1] * Q33 + B[2] * Q32), -R21, R11, 0, 13);
                        {
                            s2 = (float)Math.Abs(pp.Y * R11 - pp.X * R21) - ((A[0] * Q21 + A[1] * Q11 + B[1] * Q33 + B[2] * Q32));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((-R21) * (-R21) + (R11) * (R11) + (0) * (0));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (-R21) / l; normalC.Y = (R11) / l; normalC.Z = (0) / l;
                                    invert_normal = ((pp.Y * R11 - pp.X * R21) < 0);
                                    code = (13);
                                }
                            }
                        }
                        //TST(pp[1] * R12 - pp[0] * R22, (A[0] * Q22 + A[1] * Q12 + B[0] * Q33 + B[2] * Q31), -R22, R12, 0, 14);
                        {
                            s2 = (float)Math.Abs(pp.Y * R12 - pp.X * R22) - ((A[0] * Q22 + A[1] * Q12 + B[0] * Q33 + B[2] * Q31));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((-R22) * (-R22) + (R12) * (R12) + (0) * (0));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (-R22) / l; normalC.Y = (R12) / l; normalC.Z = (0) / l;
                                    invert_normal = ((pp.Y * R12 - pp.X * R22) < 0);
                                    code = (14);
                                }
                            }
                        }
                        //TST(pp[1] * R13 - pp[0] * R23, (A[0] * Q23 + A[1] * Q13 + B[0] * Q32 + B[1] * Q31), -R23, R13, 0, 15);
                        {
                            s2 = (float)Math.Abs(pp.Y * R13 - pp.X * R23) - ((A[0] * Q23 + A[1] * Q13 + B[0] * Q32 + B[1] * Q31));
                            if (s2 > BulletGlobal.SIMD_EPSILON) return 0;
                            l = (float)Math.Sqrt((-R23) * (-R23) + (R13) * (R13) + (0) * (0));
                            if (l > BulletGlobal.SIMD_EPSILON)
                            {
                                s2 /= l;
                                if (s2 * fudge_factor > s)
                                {
                                    s = s2;
                                    normalR = null;
                                    normalC.X = (-R23) / l; normalC.Y = (R13) / l; normalC.Z = (0) / l;
                                    invert_normal = ((pp.Y * R13 - pp.X * R23) < 0);
                                    code = (15);
                                }
                            }
                        }

                    }
                    if (code == 0) return 0;

                    // if we get to this point, the boxes interpenetrate. compute the normal
                    // in global coordinates.
                    if (normalR != null)
                    {
                        normal.X = normalR[0];
                        normal.Y = normalR[4];
                        normal.Z = normalR[8];
                    }
                    else
                    {
                        /*
                        //#define dMULTIPLYOP0_331(A,op,B,C) \
                        { \
                          (A)[0] op dDOT((B),(C)); \
                          (A)[1] op dDOT((B+4),(C)); \
                          (A)[2] op dDOT((B+8),(C)); \
                        } */

                        //dMULTIPLY0_331 (normal,R1,normalC);
                        (normal).X = dDOT((R1), ref normalC);
                        (normal).Y = dDOT((R1 + 4), ref normalC);
                        (normal).Z = dDOT((R1 + 8), ref normalC);
                    }
                    if (invert_normal)
                    {
                        /*normal[0] = -normal[0];
                        normal[1] = -normal[1];
                        normal[2] = -normal[2];*/
                        normal = -normal;
                    }
                    depth = -s;

                    // compute contact point(s)

                    if (code > 6)
                    {
                        // an edge from box 1 touches an edge from box 2.
                        // find a point pa on the intersecting edge of box 1
                        btVector3 pa = btVector3.Zero;
                        float sign;
                        for (i = 0; i < 3; i++) pa[i] = p1[i];
                        for (j = 0; j < 3; j++)
                        {
                            sign = (dDOT14(ref normal, (R1 + j)) > 0) ? 1.0f : -1.0f;
                            for (i = 0; i < 3; i++) pa[i] += sign * A[j] * R1[i * 4 + j];
                        }

                        // find a point pb on the intersecting edge of box 2
                        btVector3 pb = btVector3.Zero;
                        for (i = 0; i < 3; i++) pb[i] = p2[i];
                        for (j = 0; j < 3; j++)
                        {
                            sign = (dDOT14(ref normal, R2 + j) > 0) ? -1.0f : 1.0f;
                            for (i = 0; i < 3; i++) pb[i] += sign * B[j] * R2[i * 4 + j];
                        }

                        float alpha, beta;
                        btVector3 ua = btVector3.Zero, ub = btVector3.Zero;
                        for (i = 0; i < 3; i++) ua[i] = R1[((code) - 7) / 3 + i * 4];
                        for (i = 0; i < 3; i++) ub[i] = R2[((code) - 7) % 3 + i * 4];

                        dLineClosestApproach(ref pa, ref ua, ref pb, ref ub, &alpha, &beta);
                        for (i = 0; i < 3; i++) pa[i] += ua[i] * alpha;
                        for (i = 0; i < 3; i++) pb[i] += ub[i] * beta;

                        {

                            //contact[0].pos[i] = btScalar(0.5)*(pa[i]+pb[i]);
                            //contact[0].depth = *depth;
#if USE_CENTER_POINT
                    btVector3 pointInWorld;

                for (i=0; i<3; i++) 
		            pointInWorld[i] = (pa[i]+pb[i])*0.5f;
	            output.addContactPoint(-normal,pointInWorld,-depth);
#else
                            btVector3 temp = -normal;
                            output.addContactPoint(ref temp, ref pb, -depth);
                            normal = -temp;
#endif //
                            return_code = code;
                        }
                        return 1;
                    }
                    // okay, we have a face-something intersection (because the separating
                    // axis is perpendicular to a face). define face 'a' to be the reference
                    // face (i.e. the normal vector is perpendicular to this) and face 'b' to be
                    // the incident face (the closest face of the other box).
                    {
                        float* Ra, Rb;
                        float[] Sa, Sb;
                        //float* pa=stackalloc float[3], pb=stackalloc float[3];
                        StackPtr<float> pa = StackPtr<float>.Allocate(3);
                        StackPtr<float> pb = StackPtr<float>.Allocate(3);
                        try
                        {
                            if (code <= 3)
                            {
                                Ra = R1;
                                Rb = R2;
                                //pa = p1;
                                pa[0] = p1.X;
                                pa[1] = p1.Y;
                                pa[2] = p1.Z;
                                //pb = p2;
                                pb[0] = p2.X;
                                pb[1] = p2.Y;
                                pb[2] = p2.Z;
                                Sa = A;
                                Sb = B;
                            }
                            else
                            {
                                Ra = R2;
                                Rb = R1;
                                //pa = p2;
                                pa[0] = p2.X;
                                pa[1] = p2.Y;
                                pa[2] = p2.Z;
                                //pb = p1;
                                pb[0] = p1.X;
                                pb[1] = p1.Y;
                                pb[2] = p1.Z;
                                Sa = B;
                                Sb = A;
                            }
                            // nr = normal vector of reference face dotted with axes of incident box.
                            // anr = absolute values of nr.
                            btVector3 normal2, nr, anr;
                            if (code <= 3)
                            {
                                normal2 = normal;// new btVector3(normal[0], normal[1], normal[2]);
                            }
                            else
                            {
                                normal2 = -normal;// new btVector3(-normal[0], -normal[1], -normal[2]);
                            }
                            //dMULTIPLY1_331(nr, Rb, normal2);
                            nr = new btVector3(dDOT41((Rb), ref (normal2)), dDOT41((Rb + 1), ref (normal2)), dDOT41((Rb + 2), ref (normal2)));

                            anr = new btVector3((float)Math.Abs(nr.X), (float)Math.Abs(nr.Y), (float)Math.Abs(nr.Z));

                            // find the largest compontent of anr: this corresponds to the normal
                            // for the indident face. the other axis numbers of the indicent face
                            // are stored in a1,a2.
                            int lanr, a1, a2;
                            if (anr.Y > anr.X)
                            {
                                if (anr.Y > anr.Z)
                                {
                                    a1 = 0;
                                    lanr = 1;
                                    a2 = 2;
                                }
                                else
                                {
                                    a1 = 0;
                                    a2 = 1;
                                    lanr = 2;
                                }
                            }
                            else
                            {
                                if (anr.X > anr.Z)
                                {
                                    lanr = 0;
                                    a1 = 1;
                                    a2 = 2;
                                }
                                else
                                {
                                    a1 = 0;
                                    a2 = 1;
                                    lanr = 2;
                                }
                            }
                            // compute center point of incident face, in reference-face coordinates
                            btVector3 center = btVector3.Zero;
                            if (nr[lanr] < 0)
                            {
                                for (i = 0; i < 3; i++) center[i] = pb[i] - pa[i] + Sb[lanr] * Rb[i * 4 + lanr];
                            }
                            else
                            {
                                for (i = 0; i < 3; i++) center[i] = pb[i] - pa[i] - Sb[lanr] * Rb[i * 4 + lanr];
                            }
                            // find the normal and non-normal axis numbers of the reference box
                            int codeN, code1, code2;
                            if (code <= 3) codeN = code - 1; else codeN = code - 4;
                            if (codeN == 0)
                            {
                                code1 = 1;
                                code2 = 2;
                            }
                            else if (codeN == 1)
                            {
                                code1 = 0;
                                code2 = 2;
                            }
                            else
                            {
                                code1 = 0;
                                code2 = 1;
                            }
                            // find the four corners of the incident face, in reference-face coordinates
                            //float* quad = stackalloc float[8];	// 2D coordinate of incident face (x,y pairs)
                            StackPtr<float> quad = StackPtr<float>.Allocate(8);
                            try
                            {
                                float c1, c2, m11, m12, m21, m22;
                                c1 = dDOT14(ref center, Ra + code1);
                                c2 = dDOT14(ref center, Ra + code2);
                                // optimize this? - we have already computed this data above, but it is not
                                // stored in an easy-to-index format. for now it's quicker just to recompute
                                // the four dot products.
                                m11 = dDOT44(Ra + code1, Rb + a1);
                                m12 = dDOT44(Ra + code1, Rb + a2);
                                m21 = dDOT44(Ra + code2, Rb + a1);
                                m22 = dDOT44(Ra + code2, Rb + a2);
                                {
                                    float k1 = m11 * Sb[a1];
                                    float k2 = m21 * Sb[a1];
                                    float k3 = m12 * Sb[a2];
                                    float k4 = m22 * Sb[a2];
                                    quad[0] = c1 - k1 - k3;
                                    quad[1] = c2 - k2 - k4;
                                    quad[2] = c1 - k1 + k3;
                                    quad[3] = c2 - k2 + k4;
                                    quad[4] = c1 + k1 + k3;
                                    quad[5] = c2 + k2 + k4;
                                    quad[6] = c1 + k1 - k3;
                                    quad[7] = c2 + k2 - k4;
                                }
                                // find the size of the reference face
                                //float* rect = stackalloc float[2];
                                StackPtr<float> rect = StackPtr<float>.Allocate(2);
                                try
                                {
                                    rect[0] = Sa[code1];
                                    rect[1] = Sa[code2];

                                    // intersect the incident and reference faces
                                    //float* ret = stackalloc float[16];
                                    StackPtr<float> ret = StackPtr<float>.Allocate(16);
                                    try
                                    {
                                        int n = intersectRectQuad2(rect, quad, ret);
                                        if (n < 1) return 0;		// this should never happen

                                        // convert the intersection points into reference-face coordinates,
                                        // and compute the contact position and depth for each point. only keep
                                        // those points that have a positive (penetrating) depth. delete points in
                                        // the 'ret' array as necessary so that 'point' and 'ret' correspond.
                                        //float* point = stackalloc float[3 * 8];		// penetrating contact points
                                        //float* dep = stackalloc float[8];			// depths for those points
                                        StackPtr<float> pointSafe = StackPtr<float>.Allocate(3 * 8);
                                        StackPtr<float> dep = StackPtr<float>.Allocate(8);
                                        try
                                        {
                                            fixed (float* point = &pointSafe.Array[0])
                                            {
                                                float det1 = 1f / (m11 * m22 - m12 * m21);
                                                m11 *= det1;
                                                m12 *= det1;
                                                m21 *= det1;
                                                m22 *= det1;
                                                int cnum = 0;			// number of penetrating contact points found
                                                for (j = 0; j < n; j++)
                                                {
                                                    float k1 = m22 * (ret[j * 2] - c1) - m12 * (ret[j * 2 + 1] - c2);
                                                    float k2 = -m21 * (ret[j * 2] - c1) + m11 * (ret[j * 2 + 1] - c2);
                                                    for (i = 0; i < 3; i++) point[cnum * 3 + i] =
                                                                  center[i] + k1 * Rb[i * 4 + a1] + k2 * Rb[i * 4 + a2];
                                                    dep[cnum] = Sa[codeN] - dDOT(ref normal2, point + cnum * 3);
                                                    if (dep[cnum] >= 0)
                                                    {
                                                        ret[cnum * 2] = ret[j * 2];
                                                        ret[cnum * 2 + 1] = ret[j * 2 + 1];
                                                        cnum++;
                                                    }
                                                }
                                                if (cnum < 1) return 0;	// this should never happen
                                                // we can't generate more contacts than we actually have
                                                if (maxc > cnum) maxc = cnum;
                                                if (maxc < 1) maxc = 1;

                                                if (cnum <= maxc)
                                                {
                                                    if (code < 4)
                                                    {
                                                        // we have less contacts than we need, so we use them all
                                                        for (j = 0; j < cnum; j++)
                                                        {
                                                            btVector3 pointInWorld = btVector3.Zero;
                                                            for (i = 0; i < 3; i++)
                                                                pointInWorld[i] = point[j * 3 + i] + pa[i];
                                                            btVector3 temp = -normal;
                                                            output.addContactPoint(ref temp, ref  pointInWorld, -dep[j]);
                                                            normal = -temp;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // we have less contacts than we need, so we use them all
                                                        for (j = 0; j < cnum; j++)
                                                        {
                                                            btVector3 pointInWorld = btVector3.Zero;
                                                            for (i = 0; i < 3; i++)
                                                                pointInWorld[i] = point[j * 3 + i] + pa[i] - normal[i] * dep[j];
                                                            //pointInWorld[i] = point[j*3+i] + pa[i];
                                                            btVector3 temp = -normal;
                                                            output.addContactPoint(ref temp, ref pointInWorld, -dep[j]);
                                                            normal = -temp;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // we have more contacts than are wanted, some of them must be culled.
                                                    // find the deepest point, it is always the first contact.
                                                    int i1 = 0;
                                                    float maxdepth = dep[0];
                                                    for (i = 1; i < cnum; i++)
                                                    {
                                                        if (dep[i] > maxdepth)
                                                        {
                                                            maxdepth = dep[i];
                                                            i1 = i;
                                                        }
                                                    }

                                                    //int* iret = stackalloc int[8];
                                                    StackPtr<int> iret = StackPtr<int>.Allocate(8);
                                                    try
                                                    {
                                                        cullPoints2(cnum, ret, maxc, i1, iret);

                                                        for (j = 0; j < maxc; j++)
                                                        {
                                                            //      dContactGeom *con = CONTACT(contact,skip*j);
                                                            //    for (i=0; i<3; i++) con->pos[i] = point[iret[j]*3+i] + pa[i];
                                                            //  con->depth = dep[iret[j]];

                                                            btVector3 posInWorld = btVector3.Zero;
                                                            for (i = 0; i < 3; i++)
                                                                posInWorld[i] = point[iret[j] * 3 + i] + pa[i];
                                                            btVector3 temp = -normal;
                                                            if (code < 4)
                                                            {
                                                                output.addContactPoint(ref temp, ref posInWorld, -dep[iret[j]]);
                                                            }
                                                            else
                                                            {
                                                                btVector3 temp2 = posInWorld - normal * dep[iret[j]];
                                                                output.addContactPoint(ref temp, ref temp2, -dep[iret[j]]);
                                                            }
                                                            normal = -temp;
                                                        }
                                                        cnum = maxc;
                                                    }
                                                    finally
                                                    {
                                                        iret.Dispose();
                                                    }
                                                }
                                                return_code = code;
                                                return cnum;
                                            }
                                        }
                                        finally
                                        {
                                            pointSafe.Dispose();
                                            dep.Dispose();
                                        }
                                    }
                                    finally
                                    {
                                        ret.Dispose();
                                    }
                                }
                                finally
                                {
                                    rect.Dispose();
                                }
                            }
                            finally
                            {
                                quad.Dispose();
                            }
                        }
                        finally
                        {
                            pa.Dispose();
                            pb.Dispose();
                        }
                    }
                }
                finally
                {
                    A.Dispose();
                    B.Dispose();
                }
            }
        }

        private static unsafe void cullPoints2(int n, float[] p, int m, int i0, int[] iret_safe)
        {
            // compute the centroid of the polygon in cx,cy
            int i, j;
            fixed (int* iret_ptr = &iret_safe[0])
            {
                int* iret = iret_ptr;
                float a, cx, cy, q;
                if (n == 1)
                {
                    cx = p[0];
                    cy = p[1];
                }
                else if (n == 2)
                {
                    cx = 0.5f * (p[0] + p[2]);
                    cy = 0.5f * (p[1] + p[3]);
                }
                else
                {
                    a = 0;
                    cx = 0;
                    cy = 0;
                    for (i = 0; i < (n - 1); i++)
                    {
                        q = p[i * 2] * p[i * 2 + 3] - p[i * 2 + 2] * p[i * 2 + 1];
                        a += q;
                        cx += q * (p[i * 2] + p[i * 2 + 2]);
                        cy += q * (p[i * 2 + 1] + p[i * 2 + 3]);
                    }
                    q = p[n * 2 - 2] * p[1] - p[0] * p[n * 2 - 1];
                    if (Math.Abs(a + q) > BulletGlobal.SIMD_EPSILON)
                    {
                        a = 1f / (3.0f * (a + q));
                    }
                    else
                    {
                        a = BulletGlobal.BT_LARGE_FLOAT;
                    }
                    cx = a * (cx + q * (p[n * 2 - 2] + p[0]));
                    cy = a * (cy + q * (p[n * 2 - 1] + p[1]));
                }

                // compute the angle of each point w.r.t. the centroid
                //float* A = stackalloc float[8];
                //int* avail = stackalloc int[8];
                StackPtr<float> A = StackPtr<float>.Allocate(8);
                StackPtr<int> avail = StackPtr<int>.Allocate(8);
                try
                {

                    for (i = 0; i < n; i++) A[i] = (float)Math.Atan2(p[i * 2 + 1] - cy, p[i * 2] - cx);

                    // search for points that have angles closest to A[i0] + i*(2*pi/m).
                    for (i = 0; i < n; i++) avail[i] = 1;
                    avail[i0] = 0;
                    iret[0] = i0;
                    iret++;
                    const float M__PI = 3.14159265f;
                    for (j = 1; j < m; j++)
                    {
                        a = j * (2 * M__PI / m) + A[i0];
                        if (a > M__PI) a -= 2 * M__PI;
                        float maxdiff = 1e9f, diff;

                        *iret = i0;			// iret is not allowed to keep this value, but it sometimes does, when diff=#QNAN0

                        for (i = 0; i < n; i++)
                        {
                            if (avail[i] != 0)
                            {
                                diff = (float)Math.Abs(A[i] - a);
                                if (diff > M__PI) diff = 2 * M__PI - diff;
                                if (diff < maxdiff)
                                {
                                    maxdiff = diff;
                                    *iret = i;
                                }
                            }
                        }
#if DEBUG
                        Debug.Assert(*iret != i0);	// ensure iret got set
#endif
                        avail[*iret] = 0;
                        iret++;
                    }
                }
                finally
                {
                    A.Dispose();
                    avail.Dispose();
                }
            }
        }

        private static unsafe int intersectRectQuad2(float[] h, float[] p, float[] ret)
        {
            // q (and r) contain nq (and nr) coordinate points for the current (and
            // chopped) polygons
            int nq = 4, nr = 0;
            //float* buffer=stackalloc float[16];
            StackPtr<float> buffer_safe = StackPtr<float>.Allocate(16);
            try
            {
                fixed (float* p_ptr = &p[0], ret_ptr = &ret[0], buffer = &buffer_safe.Array[0])
                {
                    float* q = p_ptr, r = ret_ptr;
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        // direction notation: xy[0] = x axis, xy[1] = y axis
                        for (int sign = -1; sign <= 1; sign += 2)
                        {
                            // chop q along the line xy[dir] = sign*h[dir]
                            float* pq = q;
                            float* pr = r;
                            nr = 0;
                            for (int i = nq; i > 0; i--)
                            {
                                // go through all points in q and all lines between adjacent points
                                if (sign * pq[dir] < h[dir])
                                {
                                    // this point is inside the chopping line
                                    pr[0] = pq[0];
                                    pr[1] = pq[1];
                                    pr += 2;
                                    nr++;
                                    if ((nr & 8) != 0)
                                    {
                                        q = r;
                                        goto done;
                                    }
                                }
                                float* nextq = (i > 1) ? pq + 2 : q;
                                if ((sign * pq[dir] < h[dir]) ^ (sign * nextq[dir] < h[dir]))
                                {
                                    // this line crosses the chopping line
                                    pr[1 - dir] = pq[1 - dir] + (nextq[1 - dir] - pq[1 - dir]) /
                                        (nextq[dir] - pq[dir]) * (sign * h[dir] - pq[dir]);
                                    pr[dir] = sign * h[dir];
                                    pr += 2;
                                    nr++;
                                    if ((nr & 8) != 0)
                                    {
                                        q = r;
                                        goto done;
                                    }
                                }
                                pq += 2;
                            }
                            q = r;
                            r = (q == ret_ptr) ? buffer : ret_ptr;
                            nq = nr;
                        }
                    }
                done:
                    if (q != ret_ptr) memcpyf(ret_ptr, q, nr * 2);

                    return nr;
                }
            }
            finally
            {
                buffer_safe.Dispose();
            }
        }
        static unsafe void memcpyf(float* dst, float* src, int count)
        {
            while (count-- > 0)
                *dst++ = *src++;
        }

    }
}
