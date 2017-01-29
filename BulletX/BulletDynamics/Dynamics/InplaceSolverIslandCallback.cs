using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.BulletDynamics.ConstraintSolver;
using BulletX.LinerMath;

namespace BulletX.BulletDynamics.Dynamics
{
    public class InplaceSolverIslandCallback
    {

        ContactSolverInfo m_solverInfo;
        IConstraintSolver m_solver;
        IList<TypedConstraint> m_sortedConstraints;
        int m_numConstraints;
        IDebugDraw m_debugDrawer;
        //btStackAlloc*			m_stackAlloc;
        IDispatcher m_dispatcher;

        List<CollisionObject> m_bodies = new List<CollisionObject>();
        List<PersistentManifold> m_manifolds = new List<PersistentManifold>();
        List<TypedConstraint> m_constraints = new List<TypedConstraint>();

        public InplaceSolverIslandCallback() { }
        public void Constructor(
            ContactSolverInfo solverInfo,
            IConstraintSolver solver,
            IList<TypedConstraint> sortedConstraints,
            int numConstraints,
            IDebugDraw debugDrawer,
            //btStackAlloc*			stackAlloc,
            IDispatcher dispatcher)
        {
            m_solverInfo = solverInfo;
            m_solver = solver;
            m_sortedConstraints = sortedConstraints;
            m_numConstraints = numConstraints;
            m_debugDrawer = debugDrawer;
            //m_stackAlloc(stackAlloc),
            m_dispatcher = dispatcher;
            m_bodies.Clear();
            m_manifolds.Clear();
            m_constraints.Clear();
        }


        /*InplaceSolverIslandCallback& operator=(InplaceSolverIslandCallback& other)
        {
            btAssert(0);
            (void)other;
            return *this;
        }*/
        List<TypedConstraint> CurConstraintsTemp = new List<TypedConstraint>();
        public virtual void ProcessIsland(IList<CollisionObject> bodies, IList<PersistentManifold> manifolds, int islandId)
        {
            BulletGlobal.StartProfile("0-3-1-1 ProcessIslands");
            if (islandId < 0)
            {
                if (manifolds.Count + m_numConstraints != 0)
                {
                    ///we don't split islands, so all constraints/contact manifolds/bodies are passed into the solver regardless the island id
                    m_solver.solveGroup(bodies, manifolds, m_sortedConstraints, m_solverInfo, m_debugDrawer, m_dispatcher);
                }
            }
            else
            {
                //also add all non-contact constraints/joints for this island
                //TypedConstraint startConstraint = null;
                //int numCurConstraints = 0;
                CurConstraintsTemp.Clear();
                int i;

                //find the first constraint for this island
                for (i = 0; i < m_numConstraints; i++)
                {
                    if (btGetConstraintIslandId(m_sortedConstraints[i]) == islandId)
                    {
                        CurConstraintsTemp.Add(m_sortedConstraints[i]);
                        ++i;
                        break;
                    }
                }
                //count the number of constraints in this island
                for (; i < m_numConstraints; i++)
                {
                    if (btGetConstraintIslandId(m_sortedConstraints[i]) == islandId)
                    {
                        CurConstraintsTemp.Add(m_sortedConstraints[i]);
                        //numCurConstraints++;
                    }
                }
                
                if (m_solverInfo.m_minimumSolverBatchSize <= 1)
                {
                    ///only call solveGroup if there is some work: avoid virtual function call, its overhead can be excessive
                    if (manifolds.Count + m_sortedConstraints.Count != 0)
                    {
                        m_solver.solveGroup(bodies, manifolds, CurConstraintsTemp, m_solverInfo, m_debugDrawer, m_dispatcher);
                    }
                }
                else
                {

                    for (i = 0; i < bodies.Count; i++)
                        m_bodies.Add(bodies[i]);
                    for (i=0;i<manifolds.Count;i++)
                        m_manifolds.Add(manifolds[i]);
                    for (i = 0; i < CurConstraintsTemp.Count; i++)
                        m_constraints.Add(CurConstraintsTemp[i]);
                    if ((m_constraints.Count + m_manifolds.Count) > m_solverInfo.m_minimumSolverBatchSize)
                    {
                        processConstraints();
                    }
                    else
                    {
                        //printf("deferred\n");
                    }
                }
            }
            BulletGlobal.EndProfile("0-3-1-1 ProcessIslands");
        }
        public void processConstraints()
        {
            if (m_manifolds.Count + m_constraints.Count > 0)
            {
                m_solver.solveGroup(m_bodies, m_manifolds, m_constraints, m_solverInfo, m_debugDrawer, m_dispatcher);
            }
            m_bodies.Clear();
            m_manifolds.Clear();
            m_constraints.Clear();

        }
        int btGetConstraintIslandId(TypedConstraint lhs)
        {
            int islandId;

            CollisionObject rcolObj0 = lhs.RigidBodyA;
            CollisionObject rcolObj1 = lhs.RigidBodyB;
            islandId = rcolObj0.IslandTag >= 0 ? rcolObj0.IslandTag : rcolObj1.IslandTag;
            return islandId;

        }
    };

}
