using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    struct ClosestPointInput
    {
        public void Initialize()
        {
            m_maximumDistanceSquared = BulletGlobal.BT_LARGE_FLOAT;
        }

        public btTransform m_transformA;
        public btTransform m_transformB;
        public float m_maximumDistanceSquared;
        //btStackAlloc* m_stackAlloc;
    }
}
