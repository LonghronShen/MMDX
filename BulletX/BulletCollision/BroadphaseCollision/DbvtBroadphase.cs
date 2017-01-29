using System;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class DbvtBroadphase : IBroadphaseInterface
    {
        /* Config		*/
        const float DBVT_BP_MARGIN = 0.05f;
        //const int DYNAMIC_SET			=	0;	/* Dynamic set index	*/ 
        //const int FIXED_SET			=	1;	/* Fixed set index		*/ 
        const int STAGECOUNT = 2;	/* Number of stages		*/
        /* Fields		*/
        public Dbvt[] m_sets = new Dbvt[2];					// Dbvt sets
        public DbvtProxy[] m_stageRoots = new DbvtProxy[STAGECOUNT + 1];	// Stages list
        public IOverlappingPairCache m_paircache;				// Pair cache
        public float m_prediction;				// Velocity prediction
        public int m_stageCurrent;				// Current stage
        public int m_fupdates;					// % of fixed updates per frame
        public int m_dupdates;					// % of dynamic updates per frame
        public int m_cupdates;					// % of cleanup updates per frame
        public int m_newpairs;					// Number of pairs created
        public int m_fixedleft;				    // Fixed optimization left
        public uint m_updates_call;				// Number of updates call
        public uint m_updates_done;				// Number of updates done
        public float m_updates_ratio;			// m_updates_done/m_updates_call
        public int m_pid;						// Parse id
        public int m_cid;						// Cleanup index
        public int m_gid;						// Gen id
        public bool m_releasepaircache;			// Release pair cache on delete
        public bool m_deferedcollide;			// Defere dynamic/static collision to collide call
        public bool m_needcleanup;				// Need to run cleanup?

        public DbvtBroadphase(IOverlappingPairCache paircache)
        {
            for (int i = 0; i < m_sets.Length; i++)
                m_sets[i] = new Dbvt();
            m_deferedcollide = false;
            m_needcleanup = true;
            m_releasepaircache = (paircache != null) ? false : true;
            m_prediction = 0;
            m_stageCurrent = 0;
            m_fixedleft = 0;
            m_fupdates = 1;
            m_dupdates = 0;
            m_cupdates = 10;
            m_newpairs = 1;
            m_updates_call = 0;
            m_updates_done = 0;
            m_updates_ratio = 0;
            m_paircache = (paircache != null) ? paircache : new HashedOverlappingPairCache();
            m_gid = 0;
            m_pid = 0;
            m_cid = 0;
            for (int i = 0; i <= STAGECOUNT; ++i)
            {
                m_stageRoots[i] = null;
            }
#if DBVT_BP_PROFILE
	        clear(m_profiling);
#endif
        }

        #region IBroadphaseInterface メンバ

        public BroadphaseProxy createProxy(ref btVector3 aabbMin,ref btVector3 aabbMax,
            BroadphaseNativeTypes shapeType, object userPtr,
            short collisionFilterGroup, short collisionFilterMask, IDispatcher dispatcher,
            object multiSapProxy)
        {

            DbvtProxy proxy = new DbvtProxy(aabbMin, aabbMax, userPtr,
                collisionFilterGroup,
                collisionFilterMask);

            DbvtAabbMm aabb;// = DbvtAabbMm.FromMM(aabbMin, aabbMax);
            DbvtAabbMm.FromMM(ref aabbMin, ref aabbMax, out aabb);

            //bproxy->aabb			=	btDbvtVolume::FromMM(aabbMin,aabbMax);
            proxy.stage = m_stageCurrent;
            proxy.m_uniqueId = ++m_gid;
            proxy.leaf = m_sets[0].insert(ref aabb, proxy);
            listappend(ref proxy, ref m_stageRoots[m_stageCurrent]);
            if (!m_deferedcollide)
            {
                DbvtTreeCollider collider = new DbvtTreeCollider(this);
                collider.proxy = proxy;
                m_sets[0].collideTV(m_sets[0].m_root, ref aabb, collider);
                m_sets[1].collideTV(m_sets[1].m_root,ref aabb, collider);
            }
            return (proxy);
        }
        public void calculateOverlappingPairs(IDispatcher m_dispatcher1)
        {
            throw new NotImplementedException();
        }
        #endregion

        static void listappend(ref DbvtProxy item, ref DbvtProxy list)
        {
            item.links[0] = null;
            item.links[1] = list;
            if (list != null) list.links[0] = item;
            list = item;
        }

        public void setAabb(BroadphaseProxy absproxy,ref btVector3 aabbMin,ref btVector3 aabbMax, IDispatcher dispatcher)
        {
            DbvtProxy proxy = (DbvtProxy)absproxy;

            DbvtAabbMm aabb;// = DbvtAabbMm.FromMM(aabbMin, aabbMax);
            DbvtAabbMm.FromMM(ref aabbMin, ref aabbMax, out aabb);
#if DBVT_BP_PREVENTFALSEUPDATE
	        if(NotEqual(aabb,proxy->leaf->volume))
#endif
            {
                bool docollide = false;
                if (proxy.stage == STAGECOUNT)
                {/* fixed -> dynamic set	*/
                    m_sets[1].remove(proxy.leaf);
                    proxy.leaf = m_sets[0].insert(ref aabb, proxy);
                    docollide = true;
                }
                else
                {/* dynamic set				*/
                    ++m_updates_call;
                    if (Dbvt.Intersect(ref proxy.leaf.volume,ref aabb))
                    {/* Moving				*/

                        btVector3 delta = aabbMin - proxy.m_aabbMin;
                        btVector3 velocity = (((proxy.m_aabbMax - proxy.m_aabbMin) / 2) * m_prediction);
                        if (delta.X < 0) velocity.X = -velocity.X;
                        if (delta.Y < 0) velocity.Y = -velocity.Y;
                        if (delta.Z < 0) velocity.Z = -velocity.Z;
                        if (
                            m_sets[0].update(proxy.leaf, ref aabb,ref velocity, DBVT_BP_MARGIN)
                            )
                        {
                            ++m_updates_done;
                            docollide = true;
                        }
                    }
                    else
                    {/* Teleporting			*/
                        m_sets[0].update(proxy.leaf, ref aabb);
                        ++m_updates_done;
                        docollide = true;
                    }
                }
                listremove(proxy, m_stageRoots[proxy.stage]);
                proxy.m_aabbMin = aabbMin;
                proxy.m_aabbMax = aabbMax;
                proxy.stage = m_stageCurrent;
                listappend(ref proxy, ref m_stageRoots[m_stageCurrent]);
                if (docollide)
                {
                    m_needcleanup = true;
                    if (!m_deferedcollide)
                    {
                        //DbvtTreeCollider	collider(this);
                        //m_sets[1].collideTTpersistentStack(m_sets[1].m_root,proxy.leaf,collider);
                        //m_sets[0].collideTTpersistentStack(m_sets[0].m_root,proxy.leaf,collider);
                        //GC回避のために以下のように関数を変更
                        m_sets[1].collideTTpersistentStack(m_sets[1].m_root, proxy.leaf, this);
                        m_sets[0].collideTTpersistentStack(m_sets[0].m_root, proxy.leaf, this);
                    }
                }
            }
        }

        static void listremove(DbvtProxy item, DbvtProxy list)
        {
            if (item.links[0] != null) item.links[0].links[1] = item.links[1]; else list = item.links[1];
            if (item.links[1] != null) item.links[1].links[0] = item.links[0];
        }

        public IOverlappingPairCache OverlappingPairCache
        {
            get { return m_paircache; }
        }



        #region IBroadphaseInterface メンバ


        public void destroyProxy(BroadphaseProxy absproxy, IDispatcher dispatcher)
        {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            if (proxy.stage == STAGECOUNT)
                m_sets[1].remove(proxy.leaf);
            else
                m_sets[0].remove(proxy.leaf);
            listremove(proxy, m_stageRoots[proxy.stage]);
            m_paircache.removeOverlappingPairsContainingProxy(proxy, dispatcher);
            m_needcleanup = true;
        }

        #endregion

        public void rayTest(ref btVector3 rayFrom,ref btVector3 rayTo, BroadphaseRayCallback rayCallback,ref btVector3 aabbMin,ref btVector3 aabbMax)
        {
            throw new NotImplementedException();
        }

        public void aabbTest(ref btVector3 aabbMin,ref btVector3 aabbMax, IBroadphaseAabbCallback callback)
        {
            throw new NotImplementedException();
        }

        #region IBroadphaseInterface メンバ


        public void getAabb(BroadphaseProxy proxy, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            throw new NotImplementedException();
        }

        /*public void rayTest(ref btVector3 rayFrom,ref btVector3 rayTo, BroadphaseRayCallback rayCallback,ref btVector3 aabbMin,ref btVector3 aabbMax)
        {
            throw new NotImplementedException();
        }

        public void aabbTest(ref btVector3 aabbMin,ref btVector3 aabbMax, IBroadphaseAabbCallback callback)
        {
            throw new NotImplementedException();
        }*/

        public void getBroadphaseAabb(out btVector3 aabbMin, out btVector3 aabbMax)
        {
            throw new NotImplementedException();
        }

        public void resetPool(IDispatcher dispatcher)
        {
            throw new NotImplementedException();
        }

        public void printStats()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
