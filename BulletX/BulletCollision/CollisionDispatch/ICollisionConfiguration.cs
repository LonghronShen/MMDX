using BulletX.BulletCollision.BroadphaseCollision;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public interface ICollisionConfiguration
    {
#if false//未使用らしきデータへのアクセッサ
        PoolAllocator getCollisionAlgorithmPool();

        PoolAllocator getPersistentManifoldPool();
#endif
        CollisionAlgorithmCreateFunc getCollisionAlgorithmCreateFunc(BroadphaseNativeTypes proxyType0, BroadphaseNativeTypes proxyType1);
    }
}
