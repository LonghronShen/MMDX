
namespace BulletX.BulletCollision.BroadphaseCollision
{
    public interface IOverlapCallback
    {
        bool	processOverlap(BroadphasePair pair);
    }
}
