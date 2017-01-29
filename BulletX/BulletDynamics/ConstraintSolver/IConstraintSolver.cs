using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public abstract class IConstraintSolver
    {
        public virtual void prepareSolve(int numBodies, int numManifolds) { }
        
        //solve a group of constraints
        public abstract float solveGroup(IList<CollisionObject> bodies, IList<PersistentManifold> manifold, IList<TypedConstraint> constraints, ContactSolverInfo info, IDebugDraw debugDrawer, IDispatcher dispatcher);
        
        public virtual void allSolved(ContactSolverInfo info, IDebugDraw debugDrawer) { }

        //clear internal cached data and reset random seed
	    public abstract	void	reset();
    }
}
