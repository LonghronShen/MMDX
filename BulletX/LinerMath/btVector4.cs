using System;

namespace BulletX.LinerMath
{
    public struct btVector4
    {
        public float X, Y, Z, W;
        public btVector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public int closestAxis4
        {
            get
            {
                return absolute4.maxAxis4;
            }
        }
        public btVector4 absolute4
        {
            get
            {
                return new btVector4(
                    (float)Math.Abs(X),
                    (float)Math.Abs(Y),
                    (float)Math.Abs(Z),
                    (float)Math.Abs(W));
            }
        }
        public int maxAxis4
        {
            get
            {
                int maxIndex = -1;
                float maxVal = -BulletGlobal.BT_LARGE_FLOAT;
                if (X > maxVal)
                {
                    maxIndex = 0;
                    maxVal = X;
                }
                if (Y > maxVal)
                {
                    maxIndex = 1;
                    maxVal = Y;
                }
                if (Z > maxVal)
                {
                    maxIndex = 2;
                    maxVal = Z;
                }
                if (W > maxVal)
                {
                    maxIndex = 3;
                    maxVal = W;
                }




                return maxIndex;

            }
        }
    }
}
