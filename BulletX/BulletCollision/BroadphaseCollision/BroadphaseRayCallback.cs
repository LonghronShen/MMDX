using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public abstract class BroadphaseRayCallback : IBroadphaseAabbCallback
    {
        ///added some cached data to accelerate ray-AABB tests
        public btVector3 m_rayDirectionInverse;
        public uint[] m_signs;
        public float m_lambda_max;

        public abstract bool process(BroadphaseProxy proxy);
    }
}
