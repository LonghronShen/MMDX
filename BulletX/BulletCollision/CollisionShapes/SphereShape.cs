using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    /// <summary>
    /// スフィア型の衝突スキン
    /// </summary>
    public class SphereShape : ConvexInternalShape
    {

        public override BroadphaseNativeTypes ShapeType
        {
            get { return BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE; }
        }

        public SphereShape(float radius)
            : base()
        {
            m_implicitShapeDimensions.X = radius;
            m_collisionMargin = radius;
        }

        public override void localGetSupportingVertex(ref btVector3 vec, out btVector3 supVertex)
        {
            ;
            //supVertex = localGetSupportingVertexWithoutMargin(vec);
            localGetSupportingVertexWithoutMargin(ref vec, out supVertex);

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
            //return supVertex;
        }
        public override void localGetSupportingVertexWithoutMargin(ref btVector3 vec,out btVector3 result)
        {
            result= btVector3.Zero;
        }
        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(btVector3[] vectors, btVector3[] supportVerticesOut, int numVectors)
        {
            for (int i=0;i<numVectors;i++)
	        {
		        supportVerticesOut[i]=btVector3.Zero;
	        }
        }
        public override void calculateLocalInertia(float mass, out btVector3 inertia)
        {
            float elem = 0.4f * mass * Margin * Margin;
            inertia = new btVector3(elem, elem, elem);
        }
        public override void getAabb(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            btVector3 center = t.Origin;
            btVector3 extent = new btVector3(Margin, Margin, Margin);
            //aabbMin = center - extent;
            //aabbMax = center + extent;
            btVector3.Subtract(ref center, ref extent, out aabbMin);
            btVector3.Add(ref center, ref extent, out aabbMax);
        }

        public virtual float Radius { get { return m_implicitShapeDimensions.X * m_localScaling.X; } }

        void setUnscaledRadius(float radius)
        {
            m_implicitShapeDimensions.X = radius;
            base.Margin = radius;
        }
        public override string Name { get { return "SPHERE"; } }

        public override float Margin
        {
            get
            {
                //to improve gjk behaviour, use radius+margin as the full margin, so never get into the penetration case
                //this means, non-uniform scaling is not supported anymore
                return Radius;
            }
        }
    }
}
