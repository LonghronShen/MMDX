using System;
using BulletX.LinerMath;

namespace BulletX
{
    static class BulletGlobal
    {
        public const float SIMD_EPSILON =1.192092896e-07F;
        public const float BT_LARGE_FLOAT = 1e18f;
        public const float SIMDSQRT12 = 0.7071067811865475244008443621048490f;
        public const float SIMD_2_PI = 6.283185307179586232f;
        public const float SIMD_PI = (SIMD_2_PI * 0.5f);
        public const float SIMD_HALF_PI = (SIMD_2_PI * 0.25f);
        public const float SIMD_RADS_PER_DEG = ( SIMD_2_PI/ 360.0f);
        
        static Random rand = new Random();
        public static Random Rand { get { return rand; } }

        public static float GEN_clamped(float a, float lb, float ub)
        {
            return a < lb ? lb : (ub < a ? ub : a);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
        public static void btTransformAabb(btVector3 halfExtents, float margin, btTransform t, out btVector3 aabbMinOut, out btVector3 aabbMaxOut)
        {
            btVector3 halfExtentsWithMargin;// = halfExtents + new btVector3(margin, margin, margin);
            {
                btVector3 temp = new btVector3(margin, margin, margin);
                btVector3.Add(ref halfExtents, ref temp, out halfExtentsWithMargin);
            }
            btMatrix3x3 abs_b;// = t.Basis.absolute();
            t.Basis.absolute(out abs_b);
            btVector3 center = t.Origin;
            btVector3 extent = new btVector3(abs_b.el0.dot(halfExtentsWithMargin),
                   abs_b.el1.dot(halfExtentsWithMargin),
                  abs_b.el2.dot(halfExtentsWithMargin));
            //aabbMinOut = center - extent;
            btVector3.Subtract(ref center, ref extent, out aabbMinOut);
            //aabbMaxOut = center + extent;
            btVector3.Add(ref center, ref extent, out aabbMaxOut);
        }

        ///MatrixToEulerXYZ from http://www.geometrictools.com/LibFoundation/Mathematics/Wm4Matrix3.inl.html
        public static bool	matrixToEulerXYZ(btMatrix3x3 mat,out btVector3 xyz)
        {
            //	// rot =  cy*cz          -cy*sz           sy
            //	//        cz*sx*sy+cx*sz  cx*cz-sx*sy*sz -cy*sx
            //	//       -cx*cz*sy+sx*sz  cz*sx+cx*sy*sz  cx*cy
            //

            float fi = GetMatrixElem(mat, 2);
            xyz.W = 0;
            if (fi < (1.0f))
            {
                if (fi > (-1.0f))
                {
                    xyz.X = (float)Math.Atan2(-GetMatrixElem(mat, 5), GetMatrixElem(mat, 8));
                    xyz.Y = (float)Math.Asin(GetMatrixElem(mat, 2));
                    xyz.Z = (float)Math.Atan2(-GetMatrixElem(mat, 1), GetMatrixElem(mat, 0));
                    return true;
                }
                else
                {
                    // WARNING.  Not unique.  XA - ZA = -atan2(r10,r11)
                    xyz.X = -(float)Math.Atan2(GetMatrixElem(mat, 3), GetMatrixElem(mat, 4));
                    xyz.Y = -BulletGlobal.SIMD_HALF_PI;
                    xyz.Z = (0.0f);
                    return false;
                }
            }
            else
            {
                // WARNING.  Not unique.  XAngle + ZAngle = atan2(r10,r11)
                xyz.X = (float)Math.Atan2(GetMatrixElem(mat, 3), GetMatrixElem(mat, 4));
                xyz.Y = BulletGlobal.SIMD_HALF_PI;
                xyz.Z = 0.0f;
            }
            return false;
        }
        public static float GetMatrixElem(btMatrix3x3 mat, int index)
        {
	        int i = index%3;
	        int j = index/3;
	        return mat[i][j];
        }
        public static float NormalizeAngle(float angleInRadians)
        {
            angleInRadians = (float)(angleInRadians% BulletGlobal. SIMD_2_PI);
            if (angleInRadians < -BulletGlobal.SIMD_PI)
            {
                return angleInRadians + BulletGlobal.SIMD_2_PI;
            }
            else if (angleInRadians > BulletGlobal.SIMD_PI)
            {
                return angleInRadians - BulletGlobal.SIMD_2_PI;
            }
            else
            {
                return angleInRadians;
            }
        }
        public static bool FuzzyZero(float x) { return Math.Abs(x) < BulletGlobal.SIMD_EPSILON; }


        public static unsafe void SwapPtr<T>(ref T* a, ref T* b, out T* temp)
            where T: struct
        {
            temp = a;
            a = b;
            b = temp;
        }

        internal static IProfiler profiler;
        internal static void StartProfile(string p)
        {
            if (profiler != null)
                profiler.StartProfile(p);
        }

        internal static void EndProfile(string p)
        {
            if (profiler != null)
                profiler.EndProfile(p);
        }

        internal static void BeginProfileFrame()
        {
            if (profiler != null)
                profiler.BeginProfileFrame();
        }
        internal static void EndProfileFrame()
        {
            if (profiler != null)
                profiler.EndProfileFrame();
        }
    }
}
