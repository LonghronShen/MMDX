using System;
using System.Diagnostics;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    public abstract class ConvexInternalShape : ConvexShape
    {
        //local scaling. collisionMargin is not scaled !
        protected btVector3 m_localScaling;
        protected btVector3 m_implicitShapeDimensions;
        protected float m_collisionMargin;
#if false
        protected float m_padding;
#endif
        protected ConvexInternalShape()
            : base()
        {
            m_localScaling = new btVector3(1f, 1f, 1f);
            m_collisionMargin = CollisionMargin.CONVEX_DISTANCE_MARGIN;
        }

        public override void localGetSupportingVertex(ref btVector3 vec, out btVector3 supVertex)
        {
            // = localGetSupportingVertexWithoutMargin(vec);
            localGetSupportingVertexWithoutMargin(ref vec, out supVertex);

            if (Margin != 0)
            {
                btVector3 vecnorm = vec;
                if (vecnorm.Length2 < (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
                {
                    vecnorm.setValue(-1f, -1f, -1f);
                }
                vecnorm.normalize();
                #region supVertex += Margin * vecnorm;
                {
                    btVector3 temp;
                    btVector3.Multiply(ref vecnorm, Margin, out temp);
                    supVertex.Add(ref temp);
                }
                #endregion
            }
            //return supVertex;
        }
        public btVector3 ImplicitShapeDimensions { get { return m_implicitShapeDimensions; } }

        public override void getAabb(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            getAabbSlow(t, out aabbMin,out aabbMax);
        }
        public override void getAabbSlow(btTransform trans, out btVector3 minAabb, out btVector3 maxAabb)
        {
            minAabb = btVector3.Zero;
            maxAabb = btVector3.Zero;
            //use localGetSupportingVertexWithoutMargin?
	        float margin = Margin;
	        for (int i=0;i<3;i++)
            {
                btVector3 vec = btVector3.Zero;
                vec[i] = 1f;
                btVector3 sv;
                #region btVector3 sv = localGetSupportingVertex(vec*trans.Basis);
                {
                    btVector3 temp;
                    btMatrix3x3.Multiply(ref vec, ref trans.Basis, out temp);
                    //sv = localGetSupportingVertex(temp);
                    localGetSupportingVertex(ref temp, out sv);
                }
                #endregion
                btVector3 tmp = trans * sv;
                maxAabb[i] = tmp[i] + margin;
                vec[i] = -1f;
                #region tmp = trans * localGetSupportingVertex(vec * trans.Basis);
                {
                    btVector3 temp,temp2;
                    btMatrix3x3.Multiply(ref vec, ref trans.Basis, out temp);
                    localGetSupportingVertex(ref temp, out temp2);
                    btTransform.Multiply(ref trans, ref temp2, out tmp);
                    //tmp = trans * localGetSupportingVertex(temp);
                }
                #endregion
                minAabb[i] = tmp[i] - margin;
            }
        }

        public override btVector3 LocalScaling { get { return m_localScaling; } set { m_localScaling = value.absolute(); } }
        public btVector3 LocalScalingNV { get { return m_localScaling; } }

        public override float Margin { get { return m_collisionMargin; } set { m_collisionMargin = value; } }
        public float MarginNV { get { return m_collisionMargin; } }

        public override int NumPreferredPenetrationDirections
        {
            get { return 0; }
        }
        public override void getPreferredPenetrationDirection(int index, out btVector3 penetrationVector)
        {
            Debug.Assert(false);
            throw new Exception();
        }
#if false
	    virtual	int	calculateSerializeBufferSize() const;

	    ///fills the dataBuffer and returns the struct name (and 0 on failure)
	    virtual	const char*	serialize(void* dataBuffer, btSerializer* serializer) const;
#endif
        
    }
}
