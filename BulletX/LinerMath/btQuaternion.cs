using System;
using System.Diagnostics;

namespace BulletX.LinerMath
{
    //メモ：QuatWordと統合
    public struct btQuaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public btQuaternion(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }
        public btQuaternion(btVector3 axis, float angle)
        {
            float d = axis.Length;
            Debug.Assert(d != 0.0f);
            float s = (float)Math.Sin(angle * 0.5f) / d;
            X = axis.X * s;
            Y = axis.Y * s;
            Z = axis.Z * s;
            W = (float)Math.Cos(angle * 0.5f);
        }
        public static btQuaternion operator *(btQuaternion q1, btQuaternion q2)
        {
            return new btQuaternion(q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y,
                q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z,
                q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X,
                q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z);
        }
        public static btQuaternion operator /(btQuaternion q, float f)
        {
            Debug.Assert(f != 0f);
            return new btQuaternion(q.X / f, q.Y / f, q.Z / f, q.W / f);
        }
        public static void Multiply(ref btQuaternion q1, ref btQuaternion q2, out btQuaternion qout)
        {
            qout.X = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
            qout.Y = q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z;
            qout.Z = q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X;
            qout.W = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
        }
        /**@brief Return the dot product between this quaternion and another
       * @param q The other quaternion */
        public float dot(btQuaternion q)
        {
	        return X * q.X + Y * q.Y + Z * q.Z + W * q.W;
        }
        /**@brief Return the length squared of the quaternion */
        public float Length2
        {
            get
            {
                return dot(this);
            }
        }

      /**@brief Return the length of the quaternion */
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(Length2);
            }
        }
      /**@brief Normalize the quaternion 
       * Such that x^2 + y^2 + z^2 +w^2 = 1 */
        public void normalize()
        {
            this /= Length;
        }
        public void setValue(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }
        /**@brief Return the angle of rotation represented by this quaternion */
	    public float getAngle() 
	    {
		    return 2f * (float)Math.Acos(W);
		}
        /**@brief Return the inverse of this quaternion */
	    public btQuaternion inverse()
	    {
		    return new btQuaternion(-X, -Y, -Z, W);
	    }
    }
}
