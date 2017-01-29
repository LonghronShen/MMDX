using System.Collections.Generic;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public abstract class CollisionAlgorithm
    {
        static int nextID = 0;
        int m_AlgorithmID;
        public int AlgorithmID { get { return m_AlgorithmID; } }//ソート用

        public IDispatcher m_dispatcher;

        protected void Constructor(CollisionAlgorithmConstructionInfo ci)
        {
            m_dispatcher = ci.m_dispatcher1;
            m_AlgorithmID = nextID++;
        }
        public abstract void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut);
        public abstract float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut);
        public abstract void getAllContactManifolds(List<PersistentManifold> manifoldArray);
        //オブジェクトプールへの返却(XNAでのメモリ管理方法)
        public abstract void free();

        
    }
}
