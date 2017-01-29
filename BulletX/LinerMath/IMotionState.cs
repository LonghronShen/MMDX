
namespace BulletX.LinerMath
{
    public interface IMotionState
    {
        void getWorldTransform(out btTransform worldTrans);
        void setWorldTransform(btTransform worldTrans);
    }
}
