using BulletX.BulletCollision.BroadphaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public abstract class CollisionAlgorithmCreateFunc
    {
        public bool m_swapped;

        public CollisionAlgorithmCreateFunc()
        {
            m_swapped = false;
        }
        public abstract CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo info, CollisionObject body0, CollisionObject body1);

    }
}
