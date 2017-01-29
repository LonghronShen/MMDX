
namespace BulletX.BulletCollision.CollisionDispatch
{
    public enum ActivationStateFlags
    {
        //island management, m_activationState1
        NONE = 0,
        ACTIVE_TAG = 1,
        ISLAND_SLEEPING = 2,
        WANTS_DEACTIVATION = 3,
        DISABLE_DEACTIVATION = 4,
        DISABLE_SIMULATION = 5
    }
}
