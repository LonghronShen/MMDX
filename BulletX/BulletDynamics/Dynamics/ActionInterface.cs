using BulletX.BulletCollision.CollisionDispatch;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.Dynamics
{
    public abstract class ActionInterface
    {
        static RigidBody s_fixed = new RigidBody(0f, null, null, btVector3.Zero);
        protected static RigidBody FixedBody
        {
            get
            {
                s_fixed.setMassProps(0f, new btVector3(0f, 0f, 0f));
                return s_fixed;
            }
        }

        public abstract void updateAction(CollisionWorld collisionWorld, float deltaTimeStep);


        public abstract void debugDraw(IDebugDraw debugDrawer);
    }
}

