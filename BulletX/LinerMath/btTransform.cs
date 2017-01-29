
namespace BulletX.LinerMath
{
    public struct btTransform
    {
        ///Storage for the rotation
        public btMatrix3x3 Basis;
        ///Storage for the translation
        public btVector3 Origin;

        public static btTransform Identity { get { return new btTransform(btMatrix3x3.Identity); } }
        public btQuaternion Rotation
        {
            get
            {
                btQuaternion q;
                Basis.getRotation(out q);
                return q;
            }
            set { Basis.setRotation(ref value); }
        }
        public btTransform(btMatrix3x3 b)
        {
            Basis = b;
            Origin = new btVector3();
        }
        public btTransform(btMatrix3x3 b, btVector3 c)
        {
            Basis = b;
            Origin = c;
        }
        
        #region 演算子オーバーロード
        public static btVector3 operator *(btTransform t, btVector3 x)
        {
            return new btVector3(t.Basis.el0.dot(x) + t.Origin.X,
                    t.Basis.el1.dot(x) + t.Origin.Y,
                    t.Basis.el2.dot(x) + t.Origin.Z);
        }
        public static void Multiply(ref btTransform t, ref btVector3 x, out btVector3 result)
        {
            result.X = t.Basis.el0.dot(x) + t.Origin.X;
            result.Y = t.Basis.el1.dot(x) + t.Origin.Y;
            result.Z = t.Basis.el2.dot(x) + t.Origin.Z;
            result.W = 0;
        }
        public static btTransform operator*(btTransform value1, btTransform t2)
        {
	        /*return new btTransform(value1.Basis * t2.Basis, 
		        value1 * t2.Origin);*/
            btMatrix3x3 temp;
            btMatrix3x3.Multiply(ref value1.Basis, ref t2.Basis, out temp);
            return new btTransform(temp, value1 * t2.Origin);
        }
        #endregion

        
        #region 演算系
        public void setIdentity()
        {
            Basis.setIdentity();
            Origin.setValue(0.0f, 0.0f, 0.0f);
        }
        public btTransform inverse()
        {
            btMatrix3x3 inv;// = Basis.transpose();
            Basis.transpose(out inv);
            btVector3 origin, temp;
            //return new btTransform(inv, inv * -Origin);
            temp = -Origin;
            btMatrix3x3.Multiply(ref inv, ref temp, out origin);
            return new btTransform(inv, origin);
        }
        #endregion

        public btVector3 invXform(btVector3 inVec)
        {
            btVector3 v = inVec - Origin;
            //return (m_basis.transpose() * v);
            btMatrix3x3 tp;
            btVector3 result;
            //tp = Basis.transpose();
            Basis.transpose(out tp);
            btMatrix3x3.Multiply(ref tp, ref v, out result);
            return result;
        }

        public btTransform inverseTimes(btTransform t)
        {
            btVector3 v = t.Origin - Origin;
            /*return new btTransform(Basis.transposeTimes(t.Basis),
                v * Basis);*/
            btVector3 temp;
            btMatrix3x3 temp2;
            btMatrix3x3.Multiply(ref  v, ref Basis, out temp);
            Basis.transposeTimes(ref t.Basis, out temp2);
            return new btTransform(temp2, temp);
        }
    }
}
