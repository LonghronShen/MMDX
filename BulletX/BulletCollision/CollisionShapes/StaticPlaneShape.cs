using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    public class StaticPlaneShape : ConcaveShape
    {
        public override BroadphaseNativeTypes ShapeType
        {
            get { return BroadphaseNativeTypes.STATIC_PLANE_PROXYTYPE; }
        }
#if false//未移植
        btVector3 m_localAabbMin;
        btVector3 m_localAabbMax;
#endif

        btVector3 m_planeNormal;
        float m_planeConstant;
        btVector3 m_localScaling;


        public StaticPlaneShape(btVector3 planeNormal, float planeConstant)
            : base()
        {
            //m_planeNormal = planeNormal.normalized();
            planeNormal.normalized(out m_planeNormal);
            m_planeConstant = planeConstant;
            m_localScaling = btVector3.Zero;

            //	btAssert( btFuzzyZero(m_planeNormal.length() - btScalar(1.)) );

        }
        public override void getAabb(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            aabbMin = new btVector3(-BulletGlobal.BT_LARGE_FLOAT, -BulletGlobal.BT_LARGE_FLOAT, -BulletGlobal.BT_LARGE_FLOAT);
            aabbMax = new btVector3(BulletGlobal.BT_LARGE_FLOAT, BulletGlobal.BT_LARGE_FLOAT, BulletGlobal.BT_LARGE_FLOAT);
        }
#if false//未移植
        virtual void	processAllTriangles(btTriangleCallback* callback,const btVector3& aabbMin,const btVector3& aabbMax) const;
#endif
        public override void calculateLocalInertia(float mass, out btVector3 inertia)
        {
            inertia = btVector3.Zero;
        }
        public override btVector3 LocalScaling { get { return m_localScaling; } set { m_localScaling = value; } }

        public btVector3 PlaneNormal { get { return m_planeNormal; } }
        public float PlaneConstant { get { return m_planeConstant; } }

        public override string Name { get { return "STATICPLANE"; } }

#if false//未移植
        virtual	int	calculateSerializeBufferSize() const;

	    ///fills the dataBuffer and returns the struct name (and 0 on failure)
	    virtual	const char*	serialize(void* dataBuffer, btSerializer* serializer) const;
#endif
    }
}
