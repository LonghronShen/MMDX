using System;
using System.Diagnostics;

namespace BulletX.LinerMath
{
    public struct btVector3
    {
        #region メンバ変数
        public float X;
        public float Y;
        public float Z;
        public float W;
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    case 3:
                        return W;
                }
                throw new ArgumentOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    case 2:
                        Z = value;
                        return;
                    case 3:
                        W = value;
                        return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }
        #endregion
        public btVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 0;
        }
        public btVector3(float[] param)
        {
            X = param[0];
            Y = param[1];
            Z = param[2];
            if (param.Length >= 4)
                W = param[3];
            else
                W = 0;
        }

        //statics
        public static btVector3 Zero { get { return new btVector3(0, 0, 0); } }

        #region 演算子オーバーロード
        public static btVector3 operator +(btVector3 v1, btVector3 v2)
        {
            return new btVector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }
        public static void Add(ref btVector3 v1, ref btVector3 v2, out btVector3 result)
        {
            result.X = v1.X + v2.X;
            result.Y = v1.Y + v2.Y;
            result.Z = v1.Z + v2.Z;
            result.W = 0;
        }
        public void Add(ref btVector3 v2)
        {
            X = X + v2.X;
            Y = Y + v2.Y;
            Z = Z + v2.Z;
        }
        public static btVector3 operator -(btVector3 value1, btVector3 value2)
        {
            return new btVector3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }
        public static void Subtract(ref btVector3 value1, ref btVector3 value2, out btVector3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            result.W = 0;
        }
        public void Subtract(ref btVector3 value2)
        {
            X = X - value2.X;
            Y = Y - value2.Y;
            Z = Z - value2.Z;
        }
        public static btVector3 operator *(btVector3 value1, float value2)
        {
            return new btVector3(value1.X * value2, value1.Y * value2, value1.Z * value2);
        }
        public static void Multiply(ref btVector3 value1, float value2, out btVector3 result)
        {
            result.X = value1.X * value2;
            result.Y = value1.Y * value2;
            result.Z = value1.Z * value2;
            result.W = 0;
        }
        public static btVector3 operator *(float value1, btVector3 value2)
        {
            return value2 * value1;
        }
        public static void Multiply(float value2, ref btVector3 value1, out btVector3 result)
        {
            result.X = value1.X * value2;
            result.Y = value1.Y * value2;
            result.Z = value1.Z * value2;
            result.W = 0;
        }
        public void Multiply(float value2)
        {
            X = X * value2;
            Y = Y * value2;
            Z = Z * value2;
        }
        public static btVector3 operator *(btVector3 v1, btVector3 v2)
        {
            return new btVector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }
        public static void Multiply(ref btVector3 v1, ref btVector3 v2, out btVector3 result)
        {
            result.X = v1.X * v2.X;
            result.Y = v1.Y * v2.Y;
            result.Z = v1.Z * v2.Z;
            result.W = 0;
        }
        public static btVector3 operator /(btVector3 value1, btVector3 value2)
        {
            Debug.Assert(value2.X != 0 && value2.Y != 0 && value2.Z != 0);
            return new btVector3(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z);
        }
        public static void Divide(ref btVector3 value1, ref btVector3 value2, out btVector3 result)
        {
            Debug.Assert(value2.X != 0 && value2.Y != 0 && value2.Z != 0);
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            result.W = 0;
        }
        public static btVector3 operator /(btVector3 value1, float value2)
        {
            Debug.Assert(value2 != 0);
            return new btVector3(value1.X / value2, value1.Y / value2, value1.Z / value2);
        }
        public static void Divide(ref btVector3 value1, float value2, out btVector3 result)
        {
            Debug.Assert(value2 != 0);
            result.X = value1.X / value2;
            result.Y = value1.Y / value2;
            result.Z = value1.Z / value2;
            result.W = 0;
        }
        public static btVector3 operator -(btVector3 value)
        {
            return new btVector3(-value.X, -value.Y, -value.Z);
        }
        public static void Minus(ref btVector3 value, out btVector3 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = 0;
        }
        
        public static bool Equals(ref btVector3 v1, ref btVector3 v2)
        {
            return (v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z);
        }
        #endregion

        //プロパティ
        public float Length
        {
            get { return (float)Math.Sqrt(Length2); }
        }
        public float Length2
        {
            get { return dot(ref this); }
        }


        //演算系
        public void setValue(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 0;
        }
        public float dot(btVector3 v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }
        public float dot(ref btVector3 v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }
        /*public btVector3 normalized()
        {
            return this / Length;
        }*/
        public void normalized(out btVector3 result)
        {
            btVector3.Divide(ref this, Length, out result);
            //return this / Length;
        }
        public void setZero()
        {
            setValue(0f, 0f, 0f);
        }
        public btVector3 cross(btVector3 v)
        {
            return new btVector3(
                Y * v.Z - Z * v.Y,
                Z * v.X - X * v.Z,
                X * v.Y - Y * v.X);
        }
        public void cross(ref btVector3 v, out btVector3 result)
        {
            result.X = Y * v.Z - Z * v.Y;
            result.Y = Z * v.X - X * v.Z;
            result.Z = X * v.Y - Y * v.X;
            result.W = 0;
        }
        /*public btVector3 normalize()
        {
            return (this /= Length);
        }*/
        public void normalize()
        {
            btVector3.Divide(ref this, Length, out this);
        }
        public void normalize(out btVector3 result)
        {
            btVector3.Divide(ref this, Length, out result);
            this = result;
        }

        public static btVector3 Transform(btVector3 vector, btTransform trans)
        {
            return new btVector3(trans.Basis.el0.dot(vector) + trans.Origin.X,
                trans.Basis.el1.dot(vector) + trans.Origin.Y,
                trans.Basis.el2.dot(vector) + trans.Origin.Z);
        }
        public static void Transform(ref btVector3 vector, ref btTransform trans, out btVector3 result)
        {
            result.X = trans.Basis.el0.dot(ref vector) + trans.Origin.X;
            result.Y = trans.Basis.el1.dot(ref vector) + trans.Origin.Y;
            result.Z = trans.Basis.el2.dot(ref vector) + trans.Origin.Z;
            result.W = 0;
        }
        public static void PlaneSpace1(ref btVector3 n, out btVector3 p, out btVector3 q)
        {
            if ((float)Math.Abs(n.Z) > BulletGlobal.SIMDSQRT12)
            {
                // choose p in y-z plane
                float a = n.Y * n.Y + n.Z * n.Z;
                float k = 1f / (float)Math.Sqrt(a);
                p = new btVector3(0, -n.Z * k, n.Y * k);
                // set q = n x p
                q = new btVector3(a * k, -n.X * p.Z, n.X * p.Y);
            }
            else
            {
                // choose p in x-y plane
                float a = n.X * n.X + n.Y * n.Y;
                float k = 1f / (float)Math.Sqrt(a);
                p = new btVector3(-n.Y * k, n.X * k, 0);
                // set q = n x p
                q = new btVector3(-n.Z * p.Y, n.Z * p.X, a * k);
            }
        }
        public float distance2(btVector3 v)
        {
            //return (v - this).Length2;
            btVector3 temp;
            btVector3.Subtract(ref v, ref this, out temp);
            return temp.Length2;
        }
        public float distance2(ref btVector3 v)
        {
            //return (v - this).Length2;
            btVector3 temp;
            btVector3.Subtract(ref v, ref this, out temp);
            return temp.Length2;
        }

        public static float dot(btVector3 v1, btVector3 v2)
        {
            return v1.dot(v2);
        }
        public static float dot(ref btVector3 v1, ref btVector3 v2)
        {
            return v1.dot(ref v2);
        }
        public static btVector3 cross(btVector3 v1, btVector3 v2)
        {
            return v1.cross(v2);
        }
        public static void cross(ref btVector3 v1, ref btVector3 v2, out btVector3 result)
        {
            v1.cross(ref v2, out result);
        }
        public btVector3 absolute()
        {
            return new btVector3(
            (float)Math.Abs(X),
            (float)Math.Abs(Y),
            (float)Math.Abs(Z));
        }
        /*public Microsoft.Xna.Framework.Vector3 CreateXNAVector()
        {
            return new Microsoft.Xna.Framework.Vector3(X, Y, Z);
        }*/
    }
}
