using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class DbvtProxy : BroadphaseProxy
    {
        /* Fields		*/
        public DbvtNode leaf;
        public DbvtProxy[] links = new DbvtProxy[2];
        public int stage;
        /* ctor			*/
        public DbvtProxy(btVector3 aabbMin, btVector3 aabbMax, object userPtr, short collisionFilterGroup, short collisionFilterMask)
            : base(aabbMin, aabbMax, userPtr, collisionFilterGroup, collisionFilterMask)
        {
            links[0] = null;
            links[1] = null;
        }
    }
}
