
namespace BulletX.BulletCollision.BroadphaseCollision
{
    /* Tree collider	*/
    class DbvtTreeCollider : ICollide
    {
        public DbvtBroadphase pbp;
        public DbvtProxy proxy;
        public DbvtTreeCollider(DbvtBroadphase p) { pbp = p; }
        public override void Process(DbvtNode na, DbvtNode nb)
        {
            if (na != nb)
            {
                DbvtProxy pa = (DbvtProxy)na.data;
                DbvtProxy pb = (DbvtProxy)nb.data;
#if DBVT_BP_SORTPAIRS
			    if(pa->m_uniqueId>pb->m_uniqueId) 
				    btSwap(pa,pb);
#endif
                pbp.m_paircache.addOverlappingPair(pa, pb);
                ++pbp.m_newpairs;
            }
        }
        public override void Process(DbvtNode n)
        {
            Process(n, proxy.leaf);
        }
        //C#ではstackにclassのメモリを確保できず、XBoxでは毎フレームヒープにメモリを取るとGCが作動しフレームオチするため
        //このクラスをnewせずに使うルーチンを製作。
        public static void Process(DbvtBroadphase pbp, DbvtNode na, DbvtNode nb)
        {
            if (na != nb)
            {
                DbvtProxy pa = (DbvtProxy)na.data;
                DbvtProxy pb = (DbvtProxy)nb.data;
#if DBVT_BP_SORTPAIRS
			    if(pa->m_uniqueId>pb->m_uniqueId) 
				    btSwap(pa,pb);
#endif
                pbp.m_paircache.addOverlappingPair(pa, pb);
                ++pbp.m_newpairs;
            }
        }
    }
}
