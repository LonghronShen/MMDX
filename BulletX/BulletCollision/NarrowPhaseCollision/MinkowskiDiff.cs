using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    struct MinkowskiDiff
    {
        public ConvexShape m_shapes0;

        public ConvexShape m_shapes1;

        
        public btMatrix3x3 m_toshape1;
        public btTransform m_toshape0;

        public void					EnableMargin(bool enable)
		{
            if(enable)
            {
                m_shapes0.Ls = m_shapes0.localGetSupportVertexNonVirtualDelegate;
                m_shapes1.Ls = m_shapes1.localGetSupportVertexNonVirtualDelegate;
            }
            else{
                m_shapes0.Ls = m_shapes0.localGetSupportVertexWithoutMarginNonVirtualDelegate;
                m_shapes1.Ls = m_shapes1.localGetSupportVertexWithoutMarginNonVirtualDelegate;
            }
		}	

        void Support0(ref btVector3 d,out btVector3 result)
        {
            m_shapes0.Ls(ref d, out result);
        }
        void Support1(ref btVector3 d,out btVector3 result)
        {
            #region return (m_toshape0 * (m_shapes1).Ls(m_toshape1 * d));
            btVector3 temp,temp2;
            btMatrix3x3.Multiply(ref m_toshape1, ref d, out temp);
            //return (m_toshape0 * (m_shapes1).Ls(temp));
            (m_shapes1).Ls(ref temp, out temp2);
            btTransform.Multiply(ref m_toshape0, ref temp2, out result);
            #endregion
        }
        public void Support(ref btVector3 d, out btVector3 result)
        {
            //return (Support0(d) - Support1(-d));
            btVector3 temp1, temp2, temp3;
            temp1 = -d;
            Support0(ref d, out temp2);
            Support1(ref temp1, out temp3);
            btVector3.Subtract(ref temp2, ref temp3, out result);
        }
        public void Support(ref btVector3 d, uint index,out btVector3 result)
        {
            if (index != 0)
                Support1(ref d,out result);
            else
                Support0(ref d,out result);
        }
    }
}
