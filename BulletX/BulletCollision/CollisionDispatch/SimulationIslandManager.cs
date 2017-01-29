using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.BulletDynamics.Dynamics;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public class SimulationIslandManager
    {
        UnionFind m_unionFind = new UnionFind();
        public UnionFind UnionFind { get { return m_unionFind; } }
        List<PersistentManifold> m_islandmanifold = new List<PersistentManifold>();
        List<CollisionObject> m_islandBodies = new List<CollisionObject>();

        bool m_splitIslands;
        public SimulationIslandManager()
        {
            m_splitIslands = true;
        }

        public virtual void updateActivationState(CollisionWorld colWorld, IDispatcher dispatcher)
        {
            initUnionFind(colWorld.CollisionObjects.Count);

            // put the index into m_controllers into m_tag	
            {

                int index = 0;
                for (int i = 0; i < colWorld.CollisionObjects.Count; i++)
                {
                    CollisionObject collisionObject = colWorld.CollisionObjects[i];
                    collisionObject.IslandTag = index;
                    collisionObject.CompanionId = -1;
                    collisionObject.HitFraction = 1f;
                    index++;

                }
            }
            // do the union find

            findUnions(dispatcher, colWorld);
        }
        public void initUnionFind(int n)
        {
            m_unionFind.reset(n);
        }
        public void findUnions(IDispatcher dispatcher, CollisionWorld colWorld)
        {
            IOverlappingPairCache pairCachePtr = colWorld.PairCache;
            //int numOverlappingPairs = colWorld.PairCache.NumOverlappingPairs;

            for (int i = 0; i < colWorld.PairCache.NumOverlappingPairs; i++)
            {
                BroadphasePair collisionPair = pairCachePtr.OverlappingPairArrayPtr[i];
                CollisionObject colObj0 = (CollisionObject)collisionPair.m_pProxy0.m_clientObject;
                CollisionObject colObj1 = (CollisionObject)collisionPair.m_pProxy1.m_clientObject;

                if (((colObj0 != null) && ((colObj0).mergesSimulationIslands)) &&
                    ((colObj1 != null) && ((colObj1).mergesSimulationIslands)))
                {

                    m_unionFind.unite((colObj0).IslandTag,
                        (colObj1).IslandTag);
                }
            }
        }
        public virtual void storeIslandActivationState(CollisionWorld colWorld)
        {
            for (int index = 0; index < colWorld.CollisionObjects.Count; index++)
            {
                CollisionObject collisionObject = colWorld.CollisionObjects[index];
                if (!collisionObject.isStaticOrKinematicObject)
                {
                    collisionObject.IslandTag = m_unionFind.find(index);
                    collisionObject.CompanionId = -1;
                }
                else
                {
                    collisionObject.IslandTag = -1;
                    collisionObject.CompanionId = -2;
                }

            }
        }
        PersistentManifoldSortPredicate pred = new PersistentManifoldSortPredicate();
        List<PersistentManifold> islandManifoldTemp = new List<PersistentManifold>();
        public void buildAndProcessIslands(IDispatcher dispatcher, CollisionWorld collisionWorld, InplaceSolverIslandCallback callback)
        {
            BulletGlobal.StartProfile("0-3-1 buildAndProcessIslands");
            List<CollisionObject> collisionObjects = collisionWorld.CollisionObjects;

            buildIslands(dispatcher, collisionWorld);

            int endIslandIndex = 1;
            int startIslandIndex;
            int numElem = UnionFind.NumElements;


            if (!m_splitIslands)
            {
                IList<PersistentManifold> manifold = dispatcher.InternalManifoldPointer;
                int maxNumManifolds = dispatcher.NumManifolds;
                callback.ProcessIsland(collisionObjects, manifold, -1);
            }
            else
            {
                // Sort manifolds, based on islands
                // Sort the vector using predicate and std::sort
                //std::sort(islandmanifold.begin(), islandmanifold.end(), btPersistentManifoldSortPredicate);

                int numManifolds = m_islandmanifold.Count;

                //we should do radix sort, it it much faster (O(n) instead of O (n log2(n))
                m_islandmanifold.Sort(pred);

                //now process all active islands (sets of manifolds for now)

                int startManifoldIndex = 0;
                int endManifoldIndex = 1;

                //int islandId;



                //	printf("Start Islands\n");

                BulletGlobal.StartProfile("0-3-1 buildAndProcessIslands-L");
                //traverse the simulation islands, and call the solver, unless all objects are sleeping/deactivated
                for (startIslandIndex = 0; startIslandIndex < numElem; startIslandIndex = endIslandIndex)
                {
                    BulletGlobal.StartProfile("0-3-1 buildAndProcessIslands-L1");
                    int islandId = UnionFind.getElement(startIslandIndex).m_id;


                    bool islandSleeping = false;

                    for (endIslandIndex = startIslandIndex; (endIslandIndex < numElem) && (UnionFind.getElement(endIslandIndex).m_id == islandId); endIslandIndex++)
                    {
                        int i = UnionFind.getElement(endIslandIndex).m_sz;
                        CollisionObject colObj0 = collisionObjects[i];
                        m_islandBodies.Add(colObj0);
                        if (!colObj0.isActive)
                            islandSleeping = true;
                    }


                    //find the accompanying contact manifold for this islandId
                    int numIslandManifolds = 0;
                    //btPersistentManifold** startManifold = 0;
                    islandManifoldTemp.Clear();

                    if (startManifoldIndex < numManifolds)
                    {
                        int curIslandId = getIslandId(m_islandmanifold[startManifoldIndex]);
                        if (curIslandId == islandId)
                        {
                            //startManifold = &m_islandmanifold[startManifoldIndex];
                            islandManifoldTemp.Add(m_islandmanifold[startManifoldIndex]);
                            for (endManifoldIndex = startManifoldIndex + 1; (endManifoldIndex < numManifolds) && (islandId == getIslandId(m_islandmanifold[endManifoldIndex])); endManifoldIndex++)
                            {
                                islandManifoldTemp.Add(m_islandmanifold[endManifoldIndex]);
                            }
                            /// Process the actual simulation, only if not sleeping/deactivated
                            numIslandManifolds = endManifoldIndex - startManifoldIndex;
                        }

                    }
                    BulletGlobal.EndProfile("0-3-1 buildAndProcessIslands-L1");
                    BulletGlobal.StartProfile("0-3-1 buildAndProcessIslands-L2");
                    
                    if (!islandSleeping)
                    {
                        callback.ProcessIsland(m_islandBodies, islandManifoldTemp, islandId);
                        //			printf("Island callback of size:%d bodies, %d manifolds\n",islandBodies.size(),numIslandManifolds);
                    }

                    if (numIslandManifolds != 0)
                    {
                        startManifoldIndex = endManifoldIndex;
                    }

                    m_islandBodies.Clear();
                    BulletGlobal.EndProfile("0-3-1 buildAndProcessIslands-L2");
                }
                BulletGlobal.EndProfile("0-3-1 buildAndProcessIslands-L");
            } // else if(!splitIslands) 

            BulletGlobal.EndProfile("0-3-1 buildAndProcessIslands");
        }

        private static int getIslandId(PersistentManifold lhs)
        {

            int islandId;
            CollisionObject rcolObj0 = (CollisionObject)(lhs.Body0);
            CollisionObject rcolObj1 = (CollisionObject)(lhs.Body1);
            islandId = rcolObj0.IslandTag >= 0 ? rcolObj0.IslandTag : rcolObj1.IslandTag;
            return islandId;


        }
        public void buildIslands(IDispatcher dispatcher, CollisionWorld collisionWorld)
        {
            BulletGlobal.StartProfile("0-3-1-0 buildIslands");
            List<CollisionObject> collisionObjects = collisionWorld.CollisionObjects;

            m_islandmanifold.Clear();

            //we are going to sort the unionfind array, and store the element id in the size
            //afterwards, we clean unionfind, to make sure no-one uses it anymore

            UnionFind.sortIslands();
            int numElem = UnionFind.NumElements;

            int endIslandIndex = 1;
            int startIslandIndex;


            //update the sleeping state for bodies, if all are sleeping
            for (startIslandIndex = 0; startIslandIndex < numElem; startIslandIndex = endIslandIndex)
            {
                int islandId = UnionFind.getElement(startIslandIndex).m_id;
                for (endIslandIndex = startIslandIndex + 1; (endIslandIndex < numElem) && (UnionFind.getElement(endIslandIndex).m_id == islandId); endIslandIndex++)
                {
                }

                //int numSleeping = 0;

                bool allSleeping = true;

                for (int idx = startIslandIndex; idx < endIslandIndex; idx++)
                {
                    int i = UnionFind.getElement(idx).m_sz;

                    CollisionObject colObj0 = collisionObjects[i];
                    if ((colObj0.IslandTag != islandId) && (colObj0.IslandTag != -1))
                    {
                        //				printf("error in island management\n");
                    }

                    Debug.Assert((colObj0.IslandTag == islandId) || (colObj0.IslandTag == -1));
                    if (colObj0.IslandTag == islandId)
                    {
                        if (colObj0.ActivationState == ActivationStateFlags.ACTIVE_TAG)
                        {
                            allSleeping = false;
                        }
                        if (colObj0.ActivationState == ActivationStateFlags.DISABLE_DEACTIVATION)
                        {
                            allSleeping = false;
                        }
                    }
                }


                if (allSleeping)
                {
                    for (int idx = startIslandIndex; idx < endIslandIndex; idx++)
                    {
                        int i = UnionFind.getElement(idx).m_sz;
                        CollisionObject colObj0 = collisionObjects[i];
                        if ((colObj0.IslandTag != islandId) && (colObj0.IslandTag != -1))
                        {
                            //					printf("error in island management\n");
                        }

                        Debug.Assert((colObj0.IslandTag == islandId) || (colObj0.IslandTag == -1));

                        if (colObj0.IslandTag == islandId)
                        {
                            colObj0.ActivationState = ActivationStateFlags.ISLAND_SLEEPING;
                        }
                    }
                }
                else
                {

                    int idx;
                    for (idx = startIslandIndex; idx < endIslandIndex; idx++)
                    {
                        int i = UnionFind.getElement(idx).m_sz;

                        CollisionObject colObj0 = collisionObjects[i];
                        if ((colObj0.IslandTag != islandId) && (colObj0.IslandTag != -1))
                        {
                            //					printf("error in island management\n");
                        }

                        Debug.Assert((colObj0.IslandTag == islandId) || (colObj0.IslandTag == -1));

                        if (colObj0.IslandTag == islandId)
                        {
                            if (colObj0.ActivationState == ActivationStateFlags.ISLAND_SLEEPING)
                            {
                                colObj0.ActivationState = ActivationStateFlags.WANTS_DEACTIVATION;
                                colObj0.DeactivationTime = 0f;
                            }
                        }
                    }
                }
            }


            int maxNumManifolds = dispatcher.NumManifolds;

            //#define SPLIT_ISLANDS 1
            //#ifdef SPLIT_ISLANDS


            //#endif //SPLIT_ISLANDS


            for (int i = 0; i < maxNumManifolds; i++)
            {
                PersistentManifold manifold = dispatcher.getManifoldByIndexInternal(i);

                CollisionObject colObj0 = manifold.Body0;
                CollisionObject colObj1 = manifold.Body1;

                ///@todo: check sleeping conditions!
                if (((colObj0 != null) && colObj0.ActivationState != ActivationStateFlags.ISLAND_SLEEPING) ||
                   ((colObj1 != null) && colObj1.ActivationState != ActivationStateFlags.ISLAND_SLEEPING))
                {

                    //kinematic objects don't merge islands, but wake up all connected objects
                    if (colObj0.isKinematicObject && colObj0.ActivationState != ActivationStateFlags.ISLAND_SLEEPING)
                    {
                        colObj1.activate(false);
                    }
                    if (colObj1.isKinematicObject && colObj1.ActivationState != ActivationStateFlags.ISLAND_SLEEPING)
                    {
                        colObj0.activate(false);
                    }
                    if (m_splitIslands)
                    {
                        //filtering for response
                        if (dispatcher.needsResponse(colObj0, colObj1))
                            m_islandmanifold.Add(manifold);
                    }
                }
            }
            BulletGlobal.EndProfile("0-3-1-0 buildIslands");
        }
    }
}
