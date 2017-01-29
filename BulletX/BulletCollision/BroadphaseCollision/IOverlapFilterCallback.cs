
namespace BulletX.BulletCollision.BroadphaseCollision
{
    public interface IOverlapFilterCallback
    {
        bool needBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1);
    }
}
