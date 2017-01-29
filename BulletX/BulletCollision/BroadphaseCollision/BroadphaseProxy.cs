using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class BroadphaseProxy
    {

        //Usually the client btCollisionObject or Rigidbody class
        public object m_clientObject;
        public short m_collisionFilterGroup;
        public short m_collisionFilterMask;
        public object m_multiSapParentProxy;
        public int m_uniqueId;//m_uniqueId is introduced for paircache. could get rid of this, by calculating the address offset etc.
        public int UID { get { return m_uniqueId; } }

        public btVector3 m_aabbMin;
        public btVector3 m_aabbMax;

        public BroadphaseProxy()
        {
            m_clientObject = null;
            m_multiSapParentProxy = null;
        }
        public BroadphaseProxy(btVector3 aabbMin, btVector3 aabbMax, object userPtr, short collisionFilterGroup, short collisionFilterMask)
        {
            m_clientObject = userPtr;
            m_collisionFilterGroup = collisionFilterGroup;
            m_collisionFilterMask = collisionFilterMask;
            m_aabbMin = aabbMin;
            m_aabbMax = aabbMax;
            m_multiSapParentProxy = null;
        }
        public static bool isPolyhedral(BroadphaseNativeTypes proxyType)
	    {
            return (proxyType < BroadphaseNativeTypes.IMPLICIT_CONVEX_SHAPES_START_HERE);
	    }
        public static bool isConvex(BroadphaseNativeTypes proxyType)
        {
            return (proxyType < BroadphaseNativeTypes.CONCAVE_SHAPES_START_HERE);
        }
        public static bool isNonMoving(BroadphaseNativeTypes proxyType)
	    {
            return (isConcave(proxyType) && !(proxyType == BroadphaseNativeTypes.GIMPACT_SHAPE_PROXYTYPE));
	    }
        public static bool isConcave(BroadphaseNativeTypes proxyType)
        {
            return ((proxyType > BroadphaseNativeTypes.CONCAVE_SHAPES_START_HERE) &&
                    (proxyType < BroadphaseNativeTypes.CONCAVE_SHAPES_END_HERE));
        }

        public static bool isCompound(BroadphaseNativeTypes proxyType)
        {
            return (proxyType == BroadphaseNativeTypes.COMPOUND_SHAPE_PROXYTYPE);
        }
        public static bool isSoftBody(BroadphaseNativeTypes proxyType)
	    {
            return (proxyType == BroadphaseNativeTypes.SOFTBODY_SHAPE_PROXYTYPE);
	    }

        public static bool isInfinite(BroadphaseNativeTypes proxyType)
	    {
            return (proxyType == BroadphaseNativeTypes.STATIC_PLANE_PROXYTYPE);
	    }
        public static bool isConvex2d(BroadphaseNativeTypes proxyType)
        {
            return (proxyType == BroadphaseNativeTypes.BOX_2D_SHAPE_PROXYTYPE) || (proxyType == BroadphaseNativeTypes.CONVEX_2D_SHAPE_PROXYTYPE);
        }
    }
}
