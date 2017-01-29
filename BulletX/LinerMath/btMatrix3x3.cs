using System;
using System.Diagnostics;

namespace BulletX.LinerMath
{
    public struct btMatrix3x3
    {
        public btVector3 el0, el1, el2;
        
        public static btMatrix3x3 Identity
        {
            get
            {
                return new btMatrix3x3(1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);
            }
        }
        
        public btMatrix3x3(float xx, float xy, float xz,
		    float yx, float yy, float yz,
		    float zx, float zy, float zz)
	    {
            el0=new btVector3(xx, xy, xz);
            el1 = new btVector3(yx, yy, yz);
            el2 = new btVector3(zx, zy, zz);
        }
        public btMatrix3x3(btQuaternion q)
        {
            float d = q.Length2;
            Debug.Assert(d != 0.0f);
            float s = 2.0f / d;
            float xs = q.X * s, ys = q.Y * s, zs = q.Z * s;
            float wx = q.W * xs, wy = q.W * ys, wz = q.W * zs;
            float xx = q.X * xs, xy = q.X * ys, xz = q.X * zs;
            float yy = q.Y * ys, yz = q.Y * zs, zz = q.Z * zs;
            el0 = new btVector3(1.0f - (yy + zz), xy - wz, xz + wy);
            el1 = new btVector3(xy + wz, 1.0f - (xx + zz), yz - wx);
            el2 = new btVector3(xz - wy, yz + wx, 1.0f - (xx + yy));
        }
        
        #region 演算子オーバーロード
        public btVector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return el0;
                    case 1:
                        return el1;
                    case 2:
                        return el2;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static void Multiply(ref btMatrix3x3 m, ref btVector3 v, out btVector3 result)
        {
            //result = new btVector3();
            result.X = m.el0.dot(v);
            result.Y = m.el1.dot(v);
            result.Z = m.el2.dot(v);
            result.W = 0;
        }
        public static void Multiply(ref btVector3 v, ref btMatrix3x3 m, out btVector3 result)
        {
            result.X = m.tdotx(ref v);
            result.Y = m.tdoty(ref v);
            result.Z = m.tdotz(ref v);
            result.W = 0;
        }
        /*public static btMatrix3x3 operator *(btMatrix3x3 m1, btMatrix3x3 m2)
        {
            return new btMatrix3x3(
                m2.tdotx(m1.el0), m2.tdoty(m1.el0), m2.tdotz(m1.el0),
                m2.tdotx(m1.el1), m2.tdoty(m1.el1), m2.tdotz(m1.el1),
                m2.tdotx(m1.el2), m2.tdoty(m1.el2), m2.tdotz(m1.el2));
        }*/
        public static void Multiply(ref btMatrix3x3 m1, ref btMatrix3x3 m2, out btMatrix3x3 result)
        {
            result.el0.X = m2.tdotx(ref m1.el0);
            result.el0.Y = m2.tdoty(ref m1.el0);
            result.el0.Z = m2.tdotz(ref m1.el0);
            result.el0.W = 0;
            result.el1.X = m2.tdotx(ref m1.el1);
            result.el1.Y = m2.tdoty(ref m1.el1);
            result.el1.Z = m2.tdotz(ref m1.el1);
            result.el1.W = 0;
            result.el2.X = m2.tdotx(ref m1.el2);
            result.el2.Y = m2.tdoty(ref m1.el2);
            result.el2.Z = m2.tdotz(ref m1.el2);
            result.el2.W = 0;
        }
        #endregion

        /** @brief Set the values of the matrix explicitly (row major)
	    *  @param xx Top left
	    *  @param xy Top Middle
	    *  @param xz Top Right
	    *  @param yx Middle Left
	    *  @param yy Middle Middle
	    *  @param yz Middle Right
	    *  @param zx Bottom Left
	    *  @param zy Bottom Middle
	    *  @param zz Bottom Right*/
	    public void setValue(float xx,float xy,float xz, 
		    float yx, float yy, float yz, 
		    float zx, float zy, float zz)
	    {
		    el0.setValue(xx,xy,xz);
		    el1.setValue(yx,yy,yz);
		    el2.setValue(zx,zy,zz);
	    }

        public void setIdentity()
        {
            setValue(1.0f, 0.0f, 0.0f,
                    0.0f, 1.0f, 0.0f,
                    0.0f, 0.0f, 1.0f);
        }

        
        /** @brief Get a column of the matrix as a vector 
	    *  @param i Column number 0 indexed */
        public void getColumn(int i, out btVector3 result)
        {
            result.X = el0[i];
            result.Y = el1[i];
            result.Z = el2[i];
            result.W = 0;
        }
        /**@brief Get the matrix represented as a quaternion 
	    * @param q The quaternion which will be set */
        public void getRotation(out btQuaternion q)
        {
            StackPtr<float> temp = StackPtr<float>.Allocate(4);
            try
            {
                float trace = el0.X + el1.Y + el2.Z;
                //float[] temp = new float[4];

                if (trace > 0.0f)
                {
                    float s = (float)Math.Sqrt(trace + 1.0);
                    temp[3] = (s * 0.5f);
                    s = 0.5f / s;

                    temp[0] = ((el2.Y - el1.Z) * s);
                    temp[1] = ((el0.Z - el2.X) * s);
                    temp[2] = ((el1.X - el0.Y) * s);
                }
                else
                {
                    int i = el0.X < el1.Y ?
                        (el1.Y < el2.Z ? 2 : 1) :
                        (el0.X < el2.Z ? 2 : 0);
                    int j = (i + 1) % 3;
                    int k = (i + 2) % 3;

                    float s = (float)Math.Sqrt(this[i][i] - this[j][j] - this[k][k] + 1.0f);
                    temp[i] = s * 0.5f;
                    s = 0.5f / s;

                    temp[3] = (this[k][j] - this[j][k]) * s;
                    temp[j] = (this[j][i] + this[i][j]) * s;
                    temp[k] = (this[k][i] + this[i][k]) * s;
                }
                q = new btQuaternion(temp[0], temp[1], temp[2], temp[3]);
            }
            finally
            {
                temp.Dispose();
            }
        }
        /** @brief Set the matrix from a quaternion
	    *  @param q The Quaternion to match */
        public void setRotation(ref btQuaternion q)
        {
            float d = q.Length2;
            Debug.Assert(d != 0.0f);
            float s = 2.0f / d;
            float xs = q.X * s, ys = q.Y * s, zs = q.Z * s;
            float wx = q.W * xs, wy = q.W * ys, wz = q.W * zs;
            float xx = q.X * xs, xy = q.X * ys, xz = q.X * zs;
            float yy = q.Y * ys, yz = q.Y * zs, zz = q.Z * zs;
            setValue(1.0f - (yy + zz), xy - wz, xz + wy,
                xy + wz, 1.0f - (xx + zz), yz - wx,
                xz - wy, yz + wx, 1.0f - (xx + yy));
        }

        #region 演算系
        /*public btMatrix3x3 transpose() 
        {
	        return new btMatrix3x3(el0.X, el1.X, el2.X,
		        el0.Y, el1.Y, el2.Y,
		        el0.Z, el1.Z, el2.Z);
        }*/
        public void transpose(out btMatrix3x3 result)
        {
            result.el0.X = el0.X;
            result.el0.Y = el1.X;
            result.el0.Z = el2.X;
            result.el0.W = 0;
            result.el1.X = el0.Y;
            result.el1.Y = el1.Y;
            result.el1.Z = el2.Y;
            result.el1.W = 0;
            result.el2.X = el0.Z;
            result.el2.Y = el1.Z;
            result.el2.Z = el2.Z;
            result.el2.W = 0;
        }

        public float tdotx(ref btVector3 v) 
	    {
		    return el0.X * v.X + el1.X * v.Y + el2.X * v.Z;
	    }
        public float tdoty(ref btVector3 v) 
	    {
		    return el0.Y * v.X + el1.Y * v.Y + el2.Y * v.Z;
	    }
	    public float tdotz(ref btVector3 v) 
	    {
		    return el0.Z * v.X + el1.Z * v.Y + el2.Z * v.Z;
	    }
        /*public btMatrix3x3 scaled(btVector3 s)
	    {
		    return new btMatrix3x3(el0.X * s.X, el0.Y * s.Y, el0.Z * s.Z,
			    el1.X * s.X, el1.Y * s.Y, el1.Z * s.Z,
			    el2.X * s.X, el2.Y * s.Y, el2.Z * s.Z);
	    }*/
        public void scaled(ref btVector3 s, out btMatrix3x3 result)
        {
            result.el0.X = el0.X * s.X;
            result.el0.Y = el0.Y * s.Y;
            result.el0.Z = el0.Z * s.Z;
            result.el0.W = 0;
            result.el1.X = el1.X * s.X;
            result.el1.Y = el1.Y * s.Y;
            result.el1.Z = el1.Z * s.Z;
            result.el1.W = 0;
            result.el2.X = el2.X * s.X;
            result.el2.Y = el2.Y * s.Y;
            result.el2.Z = el2.Z * s.Z;
            result.el2.W = 0;
        }
        /*public btMatrix3x3 absolute()
        {
            return new btMatrix3x3(
                Math.Abs(el0.X), Math.Abs(el0.Y), Math.Abs(el0.Z),
                Math.Abs(el1.X), Math.Abs(el1.Y), Math.Abs(el1.Z),
                Math.Abs(el2.X), Math.Abs(el2.Y), Math.Abs(el2.Z));
        }*/
        public void absolute(out btMatrix3x3 result)
        {
            result.el0.X = Math.Abs(el0.X);
            result.el0.Y = Math.Abs(el0.Y);
            result.el0.Z = Math.Abs(el0.Z);
            result.el0.W = 0;
            result.el1.X = Math.Abs(el1.X);
            result.el1.Y = Math.Abs(el1.Y);
            result.el1.Z = Math.Abs(el1.Z);
            result.el1.W = 0;
            result.el2.X = Math.Abs(el2.X);
            result.el2.Y = Math.Abs(el2.Y);
            result.el2.Z = Math.Abs(el2.Z);
            result.el2.W = 0;
        }
        /*public btMatrix3x3 inverse()
        {
	        btVector3 co=new btVector3(cofac(1, 1, 2, 2), cofac(1, 2, 2, 0), cofac(1, 0, 2, 1));
	        float det = el0.dot(co);
	        Debug.Assert(det != 0.0);
	        float s = 1.0f / det;
	        return new btMatrix3x3(co.X * s, cofac(0, 2, 2, 1) * s, cofac(0, 1, 1, 2) * s,
		        co.Y * s, cofac(0, 0, 2, 2) * s, cofac(0, 2, 1, 0) * s,
		        co.Z * s, cofac(0, 1, 2, 0) * s, cofac(0, 0, 1, 1) * s);
        }*/
        public void inverse(out btMatrix3x3 result)
        {
            btVector3 co = new btVector3(cofac(1, 1, 2, 2), cofac(1, 2, 2, 0), cofac(1, 0, 2, 1));
            float det = el0.dot(co);
            Debug.Assert(det != 0.0);
            float s = 1.0f / det;
            result = new btMatrix3x3(co.X * s, cofac(0, 2, 2, 1) * s, cofac(0, 1, 1, 2) * s,
                co.Y * s, cofac(0, 0, 2, 2) * s, cofac(0, 2, 1, 0) * s,
                co.Z * s, cofac(0, 1, 2, 0) * s, cofac(0, 0, 1, 1) * s);
        }
        /**@brief Calculate the matrix cofactor 
	    * @param r1 The first row to use for calculating the cofactor
	    * @param c1 The first column to use for calculating the cofactor
	    * @param r1 The second row to use for calculating the cofactor
	    * @param c1 The second column to use for calculating the cofactor
	    * See http://en.wikipedia.org/wiki/Cofactor_(linear_algebra) for more details
	    */
	    float cofac(int r1, int c1, int r2, int c2) 
	    {
		    return this[r1][c1] * this[r2][c2] - this[r1][c2] * this[r2][c1];
	    }
        #endregion

        /*public btMatrix3x3 transposeTimes(btMatrix3x3 m)
        {
	        return new btMatrix3x3(
		        el0.X * m.el0.X + el1.X * m.el1.X + el2.X * m.el2.X,
		        el0.X * m.el0.Y + el1.X * m.el1.Y + el2.X * m.el2.Y,
		        el0.X * m.el0.Z + el1.X * m.el1.Z + el2.X * m.el2.Z,
		        el0.Y * m.el0.X + el1.Y * m.el1.X + el2.Y * m.el2.X,
		        el0.Y * m.el0.Y + el1.Y * m.el1.Y + el2.Y * m.el2.Y,
		        el0.Y * m.el0.Z + el1.Y * m.el1.Z + el2.Y * m.el2.Z,
		        el0.Z * m.el0.X + el1.Z * m.el1.X + el2.Z * m.el2.X,
		        el0.Z * m.el0.Y + el1.Z * m.el1.Y + el2.Z * m.el2.Y,
		        el0.Z * m.el0.Z + el1.Z * m.el1.Z + el2.Z * m.el2.Z);
        }*/
        public void transposeTimes(ref btMatrix3x3 m, out btMatrix3x3 result)
        {
            result.el0.X = el0.X * m.el0.X + el1.X * m.el1.X + el2.X * m.el2.X;
            result.el0.Y = el0.X * m.el0.Y + el1.X * m.el1.Y + el2.X * m.el2.Y;
            result.el0.Z = el0.X * m.el0.Z + el1.X * m.el1.Z + el2.X * m.el2.Z;
            result.el0.W = 0;
            result.el1.X = el0.Y * m.el0.X + el1.Y * m.el1.X + el2.Y * m.el2.X;
            result.el1.Y = el0.Y * m.el0.Y + el1.Y * m.el1.Y + el2.Y * m.el2.Y;
            result.el1.Z = el0.Y * m.el0.Z + el1.Y * m.el1.Z + el2.Y * m.el2.Z;
            result.el1.W = 0;
            result.el2.X = el0.Z * m.el0.X + el1.Z * m.el1.X + el2.Z * m.el2.X;
            result.el2.Y = el0.Z * m.el0.Y + el1.Z * m.el1.Y + el2.Z * m.el2.Y;
            result.el2.Z = el0.Z * m.el0.Z + el1.Z * m.el1.Z + el2.Z * m.el2.Z;
            result.el2.W = 0;
        }
    }
}
