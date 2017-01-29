using System.Collections.Generic;
using System.Diagnostics;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public struct ProxyPair
    {
        public int proxyId1;
        public int proxyId2;
    }
    class HashedOverlappingPairCache : IOverlappingPairCache
    {
        //statics...
        static int gAddedPairs = 0;

        //二つのコレクションを使うことで実装。BroadphasePairは両方に登録し、このクラス内のアクセスはDictionaryで高速化。外部へはListを渡す
        List<BroadphasePair> m_overlappingPairArray;
        //Dictionary<ProxyPair, BroadphasePair> m_overlappingPairTable;
        IOverlapFilterCallback m_overlapFilterCallback;
#if false//未使用
        bool m_blockedForChanges;
#endif
#if false//ハッシュテーブル系。Dictionaryに統合
        protected btAlignedObjectArray<int> m_hashTable;
        protected btAlignedObjectArray<int> m_next;
#endif
        protected IOverlappingPairCallback m_ghostPairCallback;

        public HashedOverlappingPairCache()
        {
            m_overlapFilterCallback = null;
            //m_blockedForChanges = false;
            m_ghostPairCallback = null;
            //int initialAllocatedSize= 2;
            m_overlappingPairArray = new List<BroadphasePair>();
            //m_overlappingPairTable = new Dictionary<ProxyPair, BroadphasePair>();
            //多分メモリ確保系の処理……(m_hashTableとm_nextとリンク)
            //growTables();
        }
        class RemovePairCallback : IOverlapCallback
        {
            BroadphaseProxy m_obsoleteProxy;
            public void Constructor(BroadphaseProxy obsoleteProxy)
            {
                m_obsoleteProxy = obsoleteProxy;

            }
            public bool processOverlap(BroadphasePair pair)
            {
                return ((pair.m_pProxy0 == m_obsoleteProxy) ||
                    (pair.m_pProxy1 == m_obsoleteProxy));
            }

        };
        RemovePairCallback removeCallback = new RemovePairCallback();
        public void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy, IDispatcher dispatcher)
        {
            removeCallback.Constructor(proxy);

            processAllOverlappingPairs(removeCallback, dispatcher);
        }
        public virtual object removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, IDispatcher dispatcher)
        {
            //gRemovePairs++;//使ってない？
            if (proxy0.m_uniqueId > proxy1.m_uniqueId)
            {
                BroadphaseProxy tmp = proxy0;
                proxy0 = proxy1;
                proxy1 = tmp;
                //btSwap(proxy0,proxy1);
            }
            int proxyId1 = proxy0.UID;
            int proxyId2 = proxy1.UID;

            /*if (proxyId1 > proxyId2) 
                btSwap(proxyId1, proxyId2);*/

            //int	hash = (int)(getHash((uint)(proxyId1),(uint)(proxyId2)) & (m_overlappingPairArray.Count-1));

            BroadphasePair pair = internalFindPair(proxy0, proxy1);
            if (pair == null)
            {
                return null;
            }

            cleanOverlappingPair(pair, dispatcher);

            object userData = null;//pair.m_internalInfo1;//どうも使われてない……？

            Debug.Assert(pair.m_pProxy0.UID == proxyId1);
            Debug.Assert(pair.m_pProxy1.UID == proxyId2);

            int pairIndex = m_overlappingPairArray.IndexOf(pair);
            Debug.Assert(pairIndex < m_overlappingPairArray.Count);

            // Remove the pair from the hash table.
            //単純にハッシュテーブルから削除
            //m_overlappingPairArray[pairIndex].free();
            m_overlappingPairArray.RemoveAt(pairIndex);
            pair.free();
            //m_overlappingPairTable.Remove(new ProxyPair { proxyId1 = proxyId1, proxyId2 = proxyId2 });

#if false //ハッシュテーブルからの検索と削除
	        int index = m_hashTable[hash];
	        btAssert(index != BT_NULL_PAIR);

	        int previous = BT_NULL_PAIR;
	        while (index != pairIndex)
	        {
		        previous = index;
		        index = m_next[index];
	        }

	        if (previous != BT_NULL_PAIR)
	        {
		        btAssert(m_next[previous] == pairIndex);
		        m_next[previous] = m_next[pairIndex];
	        }
	        else
	        {
		        m_hashTable[hash] = m_next[pairIndex];
	        }
#endif
            // We now move the last pair into spot of the
            // pair being removed. We need to fix the hash
            // table indices to support the move.
#if false
	        int lastPairIndex = m_overlappingPairArray.size() - 1;
#endif
            if (m_ghostPairCallback != null)
                m_ghostPairCallback.removeOverlappingPair(proxy0, proxy1, dispatcher);
#if false//ハッシュテーブルの再構築作業？
	        // If the removed pair is the last pair, we are done.
	        if (lastPairIndex == pairIndex)
	        {
		        m_overlappingPairArray.pop_back();
		        return userData;
	        }

	        // Remove the last pair from the hash table.
	        const btBroadphasePair* last = &m_overlappingPairArray[lastPairIndex];
		        /* missing swap here too, Nat. */ 
	        int lastHash = static_cast<int>(getHash(static_cast<unsigned int>(last->m_pProxy0->getUid()), static_cast<unsigned int>(last->m_pProxy1->getUid())) & (m_overlappingPairArray.capacity()-1));

	        index = m_hashTable[lastHash];
	        btAssert(index != BT_NULL_PAIR);

	        previous = BT_NULL_PAIR;
	        while (index != lastPairIndex)
	        {
		        previous = index;
		        index = m_next[index];
	        }

	        if (previous != BT_NULL_PAIR)
	        {
		        btAssert(m_next[previous] == lastPairIndex);
		        m_next[previous] = m_next[lastPairIndex];
	        }
	        else
	        {
		        m_hashTable[lastHash] = m_next[lastPairIndex];
	        }

	        // Copy the last pair into the remove pair's spot.
	        m_overlappingPairArray[pairIndex] = m_overlappingPairArray[lastPairIndex];

	        // Insert the last pair into the hash table
	        m_next[pairIndex] = m_hashTable[lastHash];
	        m_hashTable[lastHash] = pairIndex;

	        m_overlappingPairArray.pop_back();
#endif
            return userData;
        }
        public bool needsBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            if (m_overlapFilterCallback != null)
                return m_overlapFilterCallback.needBroadphaseCollision(proxy0, proxy1);

            bool collides = (proxy0.m_collisionFilterGroup & proxy1.m_collisionFilterMask) != 0;
            collides = collides && ((proxy1.m_collisionFilterGroup & proxy0.m_collisionFilterMask) != 0);

            return collides;
        }
        public virtual BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            gAddedPairs++;

            if (!needsBroadphaseCollision(proxy0, proxy1))
                return null;

            return internalAddPair(proxy0, proxy1);
        }
        class CleanPairCallback : IOverlapCallback
        {
            BroadphaseProxy m_cleanProxy;
            IOverlappingPairCache m_pairCache;
            IDispatcher m_dispatcher;

            public void Constructor(BroadphaseProxy cleanProxy, IOverlappingPairCache pairCache, IDispatcher dispatcher)
            {
                m_cleanProxy = cleanProxy;
                m_pairCache = pairCache;
                m_dispatcher = dispatcher;
            }
            public bool processOverlap(BroadphasePair pair)
            {
                if ((pair.m_pProxy0 == m_cleanProxy) ||
                    (pair.m_pProxy1 == m_cleanProxy))
                {
                    m_pairCache.cleanOverlappingPair(pair, m_dispatcher);
                }
                return false;
            }

        }
        CleanPairCallback cleanPairs = new CleanPairCallback();
        public void cleanProxyFromPairs(BroadphaseProxy proxy, IDispatcher dispatcher)
        {
            cleanPairs.Constructor(proxy, this, dispatcher);
            processAllOverlappingPairs(cleanPairs, dispatcher);
        }
        public virtual void processAllOverlappingPairs(IOverlapCallback callback, IDispatcher dispatcher)
        {
            int i;

            //	printf("m_overlappingPairArray.size()=%d\n",m_overlappingPairArray.size());
            for (i = 0; i < m_overlappingPairArray.Count; )
            {

                BroadphasePair pair = m_overlappingPairArray[i];
                if (callback.processOverlap(pair))
                {
                    removeOverlappingPair(pair.m_pProxy0, pair.m_pProxy1, dispatcher);

                    //gOverlappingPairs--;
                }
                else
                {
                    i++;
                }
            }
            //	printf("m_overlappingPairArray.size()=%d\n",m_overlappingPairArray.size());
        }
        public virtual IList<BroadphasePair> OverlappingPairArrayPtr
        {
            get { return m_overlappingPairArray; }
        }
        public List<BroadphasePair> OverlappingPairArray { get { return m_overlappingPairArray; } }
        public void cleanOverlappingPair(BroadphasePair pair, IDispatcher dispatcher)
        {
            if (pair.m_algorithm != null)
            {
                {
                    /*pair.m_algorithm->~btCollisionAlgorithm();
                    dispatcher->freeCollisionAlgorithm(pair.m_algorithm);
                    pair.m_algorithm=0;*/
                    dispatcher.freeCollisionAlgorithm(pair.m_algorithm);
                    pair.m_algorithm = null;
                }
            }
        }
#if false//未移植
        btBroadphasePair* findPair(btBroadphaseProxy* proxy0, btBroadphaseProxy* proxy1);
#endif
        //public int GetCount() { return m_overlappingPairArray.Count; }
        public IOverlapFilterCallback OverlapFilterCallback { get { return m_overlapFilterCallback; } set { m_overlapFilterCallback = value; } }
        public int NumOverlappingPairs
        {
            get { return m_overlappingPairArray.Count; }
        }
        private BroadphasePair internalAddPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            //どうもここはBroadphasePairを作成した上でm_overlappingPairArrayに追加しているようだ。
            //その際にダブらないような工夫をしている
            //ただ、やはりプールは使ったほうがよさそうだ……

            if (proxy0.m_uniqueId > proxy1.m_uniqueId)
                BulletGlobal.Swap(ref proxy0, ref proxy1);
            int proxyId1 = proxy0.UID;
            int proxyId2 = proxy1.UID;

            //既存の中にあるか検索
            BroadphasePair pair = internalFindPair(proxy0, proxy1);
            if (pair != null)
            {
                return pair;
            }

            //this is where we add an actual pair, so also call the 'ghost'
            if (m_ghostPairCallback != null)
                m_ghostPairCallback.addOverlappingPair(proxy0, proxy1);

            pair = BroadphasePair.Allocate(proxy0, proxy1);
            //m_overlappingPairTable.Add(new ProxyPair { proxyId1 = proxyId1, proxyId2 = proxyId2 }, pair);
            m_overlappingPairArray.Add(pair);
            pair.m_algorithm = null;
            pair.m_internalTmpValue = 0;

            return pair;
        }

#if false//未移植
        void	growTables();
#endif
        bool equalsPair(BroadphasePair pair, int proxyId1, int proxyId2)
	    {	
		    return pair.m_pProxy0.UID == proxyId1 && pair.m_pProxy1.UID == proxyId2;
	    }
#if false//未移植
        unsigned int getHash(unsigned int proxyId1, unsigned int proxyId2)
#endif
        private BroadphasePair internalFindPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            int proxyId1 = proxy0.UID;
            int proxyId2 = proxy1.UID;

            for(int i=0;i<m_overlappingPairArray.Count;i++)
            {
                var it = m_overlappingPairArray[i];
                if (it.m_pProxy0.UID == proxyId1 && it.m_pProxy1.UID == proxyId2)
                    return it;
            }
            /*ProxyPair pair = new ProxyPair { proxyId1 = proxy0.UID, proxyId2 = proxy1.UID };
            if (m_overlappingPairTable.ContainsKey(pair))
                return m_overlappingPairTable[pair];*/

            return null;
        }
        public virtual bool hasDeferredRemoval()
        {
            return false;
        }
        public virtual IOverlappingPairCallback InternalGhostPairCallback { set { m_ghostPairCallback = value; } }
#if false//未移植
        private virtual void sortOverlappingPairs(IDispatcher dispatcher);
#endif
    }
}
