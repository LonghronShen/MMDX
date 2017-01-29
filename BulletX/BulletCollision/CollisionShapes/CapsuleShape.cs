using System;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    /// <summary>
    /// カプセル型衝突スキン
    /// </summary>
    public class CapsuleShape : ConvexInternalShape
    {
        public override BroadphaseNativeTypes ShapeType
        {
            get { return BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE; }
        }

        protected int m_upAxis;
        
        protected CapsuleShape() : base() { }
        public CapsuleShape(float radius, float height)
            :base()
        {
            m_upAxis = 1;
            m_implicitShapeDimensions.setValue(radius, 0.5f * height, radius);
        }

        //CollisionShape Interface
        public override void calculateLocalInertia(float mass, out btVector3 inertia)
        {
            btTransform ident = btTransform.Identity;


            float radius = Radius;

            btVector3 halfExtents = new btVector3(radius, radius, radius);
            halfExtents[UpAxis] += HalfHeight;

            float margin = CollisionMargin.CONVEX_DISTANCE_MARGIN;

            float lx = 2f * (halfExtents.X + margin);
            float ly = 2f * (halfExtents.Y + margin);
            float lz = 2f * (halfExtents.Z + margin);
            float x2 = lx * lx;
            float y2 = ly * ly;
            float z2 = lz * lz;
            float scaledmass = mass * 0.08333333f;

            inertia = new btVector3(scaledmass * (y2 + z2), scaledmass * (x2 + z2), scaledmass * (x2 + y2));
        }

        // btConvexShape Interface
        public override void localGetSupportingVertexWithoutMargin(ref btVector3 vec0, out btVector3 supVec)
        {
            supVec = btVector3.Zero;

            float maxDot = (-BulletGlobal.BT_LARGE_FLOAT);

            btVector3 vec = vec0;
            float lenSqr = vec.Length2;
            if (lenSqr < 0.0001f)
            {
                vec.setValue(1, 0, 0);
            }
            else
            {
                float rlen = 1f / (float)Math.Sqrt(lenSqr);
                vec *= rlen;
            }

            btVector3 vtx;
            float newDot;

            float radius = Radius;


            {
                btVector3 pos = btVector3.Zero;
                pos[UpAxis] = HalfHeight;

                #region vtx = pos + vec * m_localScaling * (radius) - vec * Margin;
                {
                    btVector3 temp1, temp2, temp3, temp4;
                    btVector3.Multiply(ref vec, ref m_localScaling, out temp1);
                    btVector3.Multiply(ref temp1, radius, out temp2);
                    btVector3.Add(ref pos, ref temp2, out temp3);
                    btVector3.Multiply(ref vec, Margin, out temp4);
                    btVector3.Add(ref temp3, ref temp4, out vtx);
                }
                #endregion
                newDot = vec.dot(vtx);
                if (newDot > maxDot)
                {
                    maxDot = newDot;
                    supVec = vtx;
                }
            }
            {
                btVector3 pos = btVector3.Zero;
                pos[UpAxis] = -HalfHeight;

                #region vtx = pos + vec * m_localScaling * (radius) - vec * Margin;
                {
                    btVector3 temp1, temp2, temp3, temp4;
                    btVector3.Multiply(ref vec, ref m_localScaling, out temp1);
                    btVector3.Multiply(ref temp1, radius, out temp2);
                    btVector3.Add(ref pos, ref temp2, out temp3);
                    btVector3.Multiply(ref vec, Margin, out temp4);
                    btVector3.Add(ref temp3, ref temp4, out vtx);
                }
                #endregion
                newDot = vec.dot(vtx);
                if (newDot > maxDot)
                {
                    maxDot = newDot;
                    supVec = vtx;
                }
            }

            //return supVec;
        }

        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(btVector3[] vectors, btVector3[] supportVerticesOut, int numVectors)
        {
            float radius = Radius;

            for (int j = 0; j < numVectors; j++)
            {
                float maxDot = -BulletGlobal.BT_LARGE_FLOAT;
                btVector3 vec = vectors[j];

                btVector3 vtx;
                float newDot;
                {
                    btVector3 pos = btVector3.Zero;
                    pos[UpAxis] = HalfHeight;
                    #region vtx = pos + vec * m_localScaling * (radius) - vec * Margin;
                    {
                        btVector3 temp1, temp2, temp3, temp4;
                        btVector3.Multiply(ref vec, ref m_localScaling, out temp1);
                        btVector3.Multiply(ref temp1, radius, out temp2);
                        btVector3.Multiply(ref vec, Margin, out temp3);
                        btVector3.Add(ref pos, ref temp2, out temp4);
                        btVector3.Subtract(ref temp4, ref temp3, out vtx);
                    }
                    #endregion
                    newDot = vec.dot(ref vtx);
                    if (newDot > maxDot)
                    {
                        maxDot = newDot;
                        supportVerticesOut[j] = vtx;
                    }
                }
                {
                    btVector3 pos = btVector3.Zero;
                    pos[UpAxis] = -HalfHeight;
                    #region vtx = pos + vec * m_localScaling * (radius) - vec * Margin;
                    {
                        btVector3 temp1, temp2, temp3, temp4;
                        btVector3.Multiply(ref vec, ref m_localScaling, out temp1);
                        btVector3.Multiply(ref temp1, radius, out temp2);
                        btVector3.Multiply(ref vec, Margin, out temp3);
                        btVector3.Add(ref pos, ref temp2, out temp4);
                        btVector3.Subtract(ref temp4, ref temp3, out vtx);
                    }
                    #endregion
                    newDot = vec.dot(ref vtx);
                    if (newDot > maxDot)
                    {
                        maxDot = newDot;
                        supportVerticesOut[j] = vtx;
                    }
                }

            }
        }
        public override float Margin
        {
            set
            {
                //correct the m_implicitShapeDimensions for the margin
                btVector3 oldMargin = new btVector3(Margin, Margin, Margin);
                btVector3 implicitShapeDimensionsWithMargin;// = m_implicitShapeDimensions + oldMargin;
                btVector3.Add(ref m_implicitShapeDimensions, ref oldMargin, out implicitShapeDimensionsWithMargin);

                base.Margin = value;
                btVector3 newMargin = new btVector3(Margin, Margin, Margin);
                m_implicitShapeDimensions = implicitShapeDimensionsWithMargin - newMargin;
            }
        }

        public override void getAabb(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            btVector3 halfExtents = new btVector3(Radius, Radius, Radius);
            halfExtents[m_upAxis] = Radius + HalfHeight;
            #region halfExtents += new btVector3(Margin, Margin, Margin);
            {
                btVector3 temp = new btVector3(Margin, Margin, Margin);
                halfExtents.Add(ref temp);
            }
            #endregion
            btMatrix3x3 abs_b;// = t.Basis.absolute();
            t.Basis.absolute(out abs_b);
            btVector3 center = t.Origin;
            btVector3 extent = new btVector3(abs_b.el0.dot(halfExtents), abs_b.el1.dot(halfExtents), abs_b.el2.dot(halfExtents));

            //aabbMin = center - extent;
            btVector3.Subtract(ref center, ref extent, out aabbMin);
            //aabbMax = center + extent;
            btVector3.Add(ref center, ref extent, out aabbMax);
        }

        public override string Name { get { return "CapsuleShape"; } }


        public int UpAxis { get { return m_upAxis; } }

        
        public float Radius
        {
            get
            {
                int radiusAxis = (m_upAxis + 2) % 3;
                return m_implicitShapeDimensions[radiusAxis];
            }
        }
        public float HalfHeight { get { return m_implicitShapeDimensions[m_upAxis]; } }

        public override btVector3 LocalScaling
        {
            set
            {

                btVector3 oldMargin=new btVector3(Margin,Margin,Margin);
                btVector3 implicitShapeDimensionsWithMargin;// = m_implicitShapeDimensions + oldMargin;
                btVector3.Add(ref m_implicitShapeDimensions, ref oldMargin, out implicitShapeDimensionsWithMargin);

		        btVector3 unScaledImplicitShapeDimensionsWithMargin = implicitShapeDimensionsWithMargin / m_localScaling;
                base.LocalScaling = value;
            
		        m_implicitShapeDimensions = (unScaledImplicitShapeDimensionsWithMargin * m_localScaling) - oldMargin;

            }
        }

#if false
        virtual	int	calculateSerializeBufferSize() const;

	    ///fills the dataBuffer and returns the struct name (and 0 on failure)
	    virtual	const char*	serialize(void* dataBuffer, btSerializer* serializer) const;
#endif
        
    }
}
