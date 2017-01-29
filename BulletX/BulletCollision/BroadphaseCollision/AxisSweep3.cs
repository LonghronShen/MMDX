#define USE_OVERLAP_TEST_ON_REMOVES
#define CLEAN_INVALID_PAIRS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class AxisSweep3 : IBroadphaseInterface
    {
        protected UInt16 m_bpHandleMask;
        protected UInt16 m_handleSentinel;

        public struct Edge
        {
            public UInt16 m_pos;			// low bit is min/max

            public UInt16 m_handle;

            public bool IsMax { get { return (m_pos & 1) == 1; } }
        }
        public class Handle : BroadphaseProxy
        {
            // indexes into the edge arrays
            public UInt16[] m_minEdges = new UInt16[3];
            public UInt16[] m_maxEdges = new UInt16[3];		// 6 * 2 = 12
            public BroadphaseProxy m_dbvtProxy;//for faster raycast

            internal void SetNextFree(UInt16 next)
            {
                m_minEdges[0] = next;
            }
            public ushort GetNextFree() { return m_minEdges[0]; }
        }

        
        protected btVector3 m_worldAabbMin;						// overall system bounds
        protected btVector3 m_worldAabbMax;						// overall system bounds

        protected btVector3 m_quantize;						// scaling factor for quantization

        protected UInt16 m_numHandles;						// number of active handles
#if false
        //m_pHandles.Lengthで代用
        //protected BP_FP_INT_TYPE m_maxHandles;						// max number of handles
#endif
        protected Handle[] m_pHandles;						// handles pool

        protected UInt16 m_firstFreeHandle;		// free handles list

        protected Edge[][] m_pEdges = new Edge[3][];						// edge arrays for the 3 axes (each array has m_maxHandles * 2 + 2 sentinel entries)
        //メモリ位置保持/開放用なので削除
        //protected void* m_pEdgesRawPtr[3];

        protected IOverlappingPairCache m_pairCache;

        ///btOverlappingPairCallback is an additional optional user callback for adding/removing overlapping pairs, similar interface to btOverlappingPairCache.
        protected IOverlappingPairCallback m_userPairCallback;

        //m_pairChacheを削除するためのフラグなので削除
        //protected bool	m_ownsPairCache;

        protected int m_invalidPair;

        ///additional dynamic aabb structure, used to accelerate ray cast queries.
        ///can be disabled using a optional argument in the constructor
        protected DbvtBroadphase m_raycastAccelerator;
        protected IOverlappingPairCache m_nullPairCache;
        protected ushort allocHandle()
        {
            Debug.Assert(m_firstFreeHandle != 0);

            ushort handle = m_firstFreeHandle;
            m_firstFreeHandle = getHandle(handle).GetNextFree();
            m_numHandles++;

            return handle;
        }
        protected void freeHandle(ushort handle)
        {
            Debug.Assert(handle > 0 && handle < m_pHandles.Length);

            getHandle(handle).SetNextFree(m_firstFreeHandle);
            m_firstFreeHandle = handle;

            m_numHandles--;
        }
        protected bool testOverlap2D(Handle pHandleA, Handle pHandleB, int axis0, int axis1)
        {
            if (pHandleA.m_maxEdges[axis0] < pHandleB.m_minEdges[axis0] ||
                pHandleB.m_maxEdges[axis0] < pHandleA.m_minEdges[axis0] ||
                pHandleA.m_maxEdges[axis1] < pHandleB.m_minEdges[axis1] ||
                pHandleB.m_maxEdges[axis1] < pHandleA.m_minEdges[axis1])
            {
                return false;
            }
            return true;
        }

#if DEBUG_BROADPHASE
	    protected void debugPrintAxis(int axis,bool checkCardinality=true);
#endif //DEBUG_BROADPHASE

        protected unsafe void sortMinDown(int axis, ushort edge, IDispatcher dispatcher, bool updateOverlaps)
        {
            fixed (Edge* pEdgeBase = &m_pEdges[axis][0])
            {
                Edge* pEdge = pEdgeBase + edge;
                Edge* pPrev = pEdge - 1;
                Handle pHandleEdge = getHandle(pEdge->m_handle);

                while (pEdge->m_pos < pPrev->m_pos)
                {
                    Handle pHandlePrev = getHandle(pPrev->m_handle);

                    if (pPrev->IsMax)
                    {
                        // if previous edge is a maximum check the bounds and add an overlap if necessary
                        int axis1 = (1 << axis) & 3;
                        int axis2 = (1 << axis1) & 3;
                        if (updateOverlaps && testOverlap2D(pHandleEdge, pHandlePrev, axis1, axis2))
                        {
                            m_pairCache.addOverlappingPair(pHandleEdge, pHandlePrev);
                            if (m_userPairCallback != null)
                                m_userPairCallback.addOverlappingPair(pHandleEdge, pHandlePrev);

                            //AddOverlap(pEdge->m_handle, pPrev->m_handle);

                        }

                        // update edge reference in other handle
                        pHandlePrev.m_maxEdges[axis]++;
                    }
                    else
                        pHandlePrev.m_minEdges[axis]++;

                    pHandleEdge.m_minEdges[axis]--;

                    // swap the edges
                    Edge swap = *pEdge;
                    *pEdge = *pPrev;
                    *pPrev = swap;

                    // decrement
                    pEdge--;
                    pPrev--;
                }
            }
#if DEBUG_BROADPHASE
	        debugPrintAxis(axis);
#endif //DEBUG_BROADPHASE
        }
        protected unsafe void sortMinUp(int axis, ushort edge, IDispatcher dispatcher, bool updateOverlaps)
        {
            fixed (Edge* m_pEdges0 = &m_pEdges[axis][0])
            {
                Edge* pEdge = m_pEdges0 + edge;
                Edge* pNext = pEdge + 1;
                Handle pHandleEdge = getHandle(pEdge->m_handle);

                while (pNext->m_handle != 0 && (pEdge->m_pos >= pNext->m_pos))
                {
                    Handle pHandleNext = getHandle(pNext->m_handle);

                    if (pNext->IsMax)
                    {
                        Handle handle0 = getHandle(pEdge->m_handle);
                        Handle handle1 = getHandle(pNext->m_handle);
                        int axis1 = (1 << axis) & 3;
                        int axis2 = (1 << axis1) & 3;

                        // if next edge is maximum remove any overlap between the two handles
                        if (updateOverlaps
#if USE_OVERLAP_TEST_ON_REMOVES
                            && testOverlap2D(handle0, handle1, axis1, axis2)
#endif
                            )
                        {


                            m_pairCache.removeOverlappingPair(handle0, handle1, dispatcher);
                            if (m_userPairCallback != null)
                                m_userPairCallback.removeOverlappingPair(handle0, handle1, dispatcher);

                        }


                        // update edge reference in other handle
                        pHandleNext.m_maxEdges[axis]--;
                    }
                    else
                        pHandleNext.m_minEdges[axis]--;

                    pHandleEdge.m_minEdges[axis]++;

                    // swap the edges
                    Edge swap = *pEdge;
                    *pEdge = *pNext;
                    *pNext = swap;

                    // increment
                    pEdge++;
                    pNext++;
                }
            }

        }
        protected unsafe void sortMaxDown(int axis, ushort edge, IDispatcher dispatcher, bool updateOverlaps)
        {
            fixed (Edge* m_pEdges0 = &m_pEdges[axis][0])
            {
                Edge* pEdge = m_pEdges0 + edge;
                Edge* pPrev = pEdge - 1;
                Handle pHandleEdge = getHandle(pEdge->m_handle);

                while (pEdge->m_pos < pPrev->m_pos)
                {
                    Handle pHandlePrev = getHandle(pPrev->m_handle);

                    if (!pPrev->IsMax)
                    {
                        // if previous edge was a minimum remove any overlap between the two handles
                        Handle handle0 = getHandle(pEdge->m_handle);
                        Handle handle1 = getHandle(pPrev->m_handle);
                        int axis1 = (1 << axis) & 3;
                        int axis2 = (1 << axis1) & 3;

                        if (updateOverlaps
#if USE_OVERLAP_TEST_ON_REMOVES
                            && testOverlap2D(handle0, handle1, axis1, axis2)
#endif
                            )
                        {
                            //this is done during the overlappingpairarray iteration/narrowphase collision


                            m_pairCache.removeOverlappingPair(handle0, handle1, dispatcher);
                            if (m_userPairCallback != null)
                                m_userPairCallback.removeOverlappingPair(handle0, handle1, dispatcher);



                        }

                        // update edge reference in other handle
                        pHandlePrev.m_minEdges[axis]++; ;
                    }
                    else
                        pHandlePrev.m_maxEdges[axis]++;

                    pHandleEdge.m_maxEdges[axis]--;

                    // swap the edges
                    Edge swap = *pEdge;
                    *pEdge = *pPrev;
                    *pPrev = swap;

                    // decrement
                    pEdge--;
                    pPrev--;
                }
            }

#if DEBUG_BROADPHASE
	debugPrintAxis(axis);
#endif //DEBUG_BROADPHASE

        }
        protected unsafe void sortMaxUp(int axis, ushort edge, IDispatcher dispatcher, bool updateOverlaps)
        {
            fixed (Edge* m_pEdges0 = &m_pEdges[axis][0])
            {
                Edge* pEdge = m_pEdges0 + edge;
                Edge* pNext = pEdge + 1;
                Handle pHandleEdge = getHandle(pEdge->m_handle);

                while (pNext->m_handle != 0 && (pEdge->m_pos >= pNext->m_pos))
                {
                    Handle pHandleNext = getHandle(pNext->m_handle);

                    int axis1 = (1 << axis) & 3;
                    int axis2 = (1 << axis1) & 3;

                    if (!pNext->IsMax)
                    {
                        // if next edge is a minimum check the bounds and add an overlap if necessary
                        if (updateOverlaps && testOverlap2D(pHandleEdge, pHandleNext, axis1, axis2))
                        {
                            Handle handle0 = getHandle(pEdge->m_handle);
                            Handle handle1 = getHandle(pNext->m_handle);
                            m_pairCache.addOverlappingPair(handle0, handle1);
                            if (m_userPairCallback != null)
                                m_userPairCallback.addOverlappingPair(handle0, handle1);
                        }

                        // update edge reference in other handle
                        pHandleNext.m_minEdges[axis]--;
                    }
                    else
                        pHandleNext.m_maxEdges[axis]--;

                    pHandleEdge.m_maxEdges[axis]++;

                    // swap the edges
                    Edge swap = *pEdge;
                    *pEdge = *pNext;
                    *pNext = swap;

                    // increment
                    pEdge++;
                    pNext++;
                }
            }
        }
        public AxisSweep3(btVector3 worldAabbMin, btVector3 worldAabbMax)
        {
            Constructor(worldAabbMin, worldAabbMax, 0xfffe, 0xffff, 16384, null, false);
        }
        public AxisSweep3(btVector3 worldAabbMin, btVector3 worldAabbMax, ushort maxHandles)
        {
            Debug.Assert(maxHandles > 1 && maxHandles < 32767);
            Constructor(worldAabbMin, worldAabbMax, 0xfffe, 0xffff, maxHandles, null, false);
        }
        public AxisSweep3(btVector3 worldAabbMin, btVector3 worldAabbMax, ushort maxHandles, IOverlappingPairCache pairCache)
        {
            Debug.Assert(maxHandles > 1 && maxHandles < 32767);
            Constructor(worldAabbMin, worldAabbMax, 0xfffe, 0xffff, maxHandles, pairCache, false);
        }
        public AxisSweep3(btVector3 worldAabbMin, btVector3 worldAabbMax, ushort maxHandles, IOverlappingPairCache pairCache, bool disableRaycastAccelerator)
        {
            Debug.Assert(maxHandles > 1 && maxHandles < 32767);
            Constructor(worldAabbMin, worldAabbMax, 0xfffe, 0xffff, maxHandles, pairCache, disableRaycastAccelerator);
        }
        public void Constructor(btVector3 worldAabbMin, btVector3 worldAabbMax, UInt16 handleMask, UInt16 handleSentinel, UInt16 userMaxHandles, IOverlappingPairCache pairCache, bool disableRaycastAccelerator)
        {
            m_bpHandleMask = handleMask;
            m_handleSentinel = handleSentinel;
            m_pairCache = pairCache;
            m_userPairCallback = null;
            //m_ownsPairCache=false;
            m_invalidPair = 0;
            m_raycastAccelerator = null;

            UInt16 maxHandles = (ushort)(userMaxHandles + 1);//need to add one sentinel handle

            if (m_pairCache == null)
            {
                m_pairCache = new HashedOverlappingPairCache();
                //m_ownsPairCache = true;
            }

            if (!disableRaycastAccelerator)
            {
                m_nullPairCache = new NullPairCache();
                m_raycastAccelerator = new DbvtBroadphase(m_nullPairCache);//m_pairCache);
                m_raycastAccelerator.m_deferedcollide = true;//don't add/remove pairs
            }

            // init bounds
            m_worldAabbMin = worldAabbMin;
            m_worldAabbMax = worldAabbMax;

            btVector3 aabbSize = m_worldAabbMax - m_worldAabbMin;

            UInt16 maxInt = m_handleSentinel;

            m_quantize = new btVector3(maxInt, maxInt, maxInt) / aabbSize;

            // allocate handles buffer, using btAlignedAlloc, and put all handles on free list
            m_pHandles = new Handle[maxHandles];
            for (int i = 0; i < maxHandles; i++)
                m_pHandles[i] = new Handle();

            //m_maxHandles = maxHandles;
            m_numHandles = 0;

            // handle 0 is reserved as the null index, and is also used as the sentinel
            m_firstFreeHandle = 1;
            {
                for (UInt16 i = m_firstFreeHandle; i < m_pHandles.Length; i++)
                    m_pHandles[i].SetNextFree((ushort)(i + 1));
                m_pHandles[m_pHandles.Length - 1].SetNextFree(0);
            }

            {
                // allocate edge buffers
                for (int i = 0; i < 3; i++)
                {
                    m_pEdges[i] = new Edge[maxHandles * 2];
                }
            }
            //removed overlap management

            // make boundary sentinels

            m_pHandles[0].m_clientObject = null;

            for (int axis = 0; axis < 3; axis++)
            {
                m_pHandles[0].m_minEdges[axis] = 0;
                m_pHandles[0].m_maxEdges[axis] = 1;

                m_pEdges[axis][0].m_pos = 0;
                m_pEdges[axis][0].m_handle = 0;
                m_pEdges[axis][1].m_pos = m_handleSentinel;
                m_pEdges[axis][1].m_handle = 0;

#if DEBUG_BROADPHASE
		        debugPrintAxis(axis);
#endif //DEBUG_BROADPHASE

            }
        }
        public int NumHandles { get { return m_numHandles; } }
        //ソート用変数
        static BroadphasePairSortPredicate sortPredicate = new BroadphasePairSortPredicate();
        public virtual void calculateOverlappingPairs(IDispatcher dispatcher)
        {
            if (m_pairCache.hasDeferredRemoval())
            {

                List<BroadphasePair> overlappingPairArray = m_pairCache.OverlappingPairArray;

                //perform a sort, to find duplicates and to sort 'invalid' pairs to the end
                overlappingPairArray.Sort(sortPredicate);

                //overlappingPairArray.resize(overlappingPairArray.size() - m_invalidPair);
                overlappingPairArray.RemoveRange(overlappingPairArray.Count - m_invalidPair, m_invalidPair);
                m_invalidPair = 0;


                //int i;

                BroadphasePair previousPair = null;
                previousPair.m_pProxy0 = null;
                previousPair.m_pProxy1 = null;
                previousPair.m_algorithm = null;
                

                for(int i=0;i<overlappingPairArray.Count;i++)
                {
                    BroadphasePair pair = overlappingPairArray[i];
                    bool isDuplicate = (pair == previousPair);

                    previousPair = pair;

                    bool needsRemoval = false;

                    if (!isDuplicate)
                    {
                        ///important to use an AABB test that is consistent with the broadphase
                        bool hasOverlap = testAabbOverlap(pair.m_pProxy0, pair.m_pProxy1);

                        if (hasOverlap)
                        {
                            needsRemoval = false;//callback->processOverlap(pair);
                        }
                        else
                        {
                            needsRemoval = true;
                        }
                    }
                    else
                    {
                        //remove duplicate
                        needsRemoval = true;
                        //should have no algorithm
                        Debug.Assert(pair.m_algorithm == null);
                    }

                    if (needsRemoval)
                    {
                        m_pairCache.cleanOverlappingPair(pair, dispatcher);

                        //		m_overlappingPairArray.swap(i,m_overlappingPairArray.size()-1);
                        //		m_overlappingPairArray.pop_back();
                        pair.m_pProxy0 = null;
                        pair.m_pProxy1 = null;
                        m_invalidPair++;
                        //gOverlappingPairs--;
                    }

                }

                ///if you don't like to skip the invalid pairs in the array, execute following code:
                
#if CLEAN_INVALID_PAIRS

                //perform a sort, to sort 'invalid' pairs to the end
                overlappingPairArray.Sort(sortPredicate);

                //overlappingPairArray.resize(overlappingPairArray.size() - m_invalidPair);
                overlappingPairArray.RemoveRange(overlappingPairArray.Count - m_invalidPair, m_invalidPair);
                m_invalidPair = 0;
#endif//CLEAN_INVALID_PAIRS

                //printf("overlappingPairArray.size()=%d\n",overlappingPairArray.size());
            }

        }
        public ushort addHandle(btVector3 aabbMin, btVector3 aabbMax, object pOwner, short collisionFilterGroup, short collisionFilterMask, IDispatcher dispatcher, object multiSapProxy)
        {
            // quantize the bounds
            ushort[] min = new ushort[3];
            ushort[] max = new ushort[3];
            quantize(min, aabbMin, 0);
            quantize(max, aabbMax, 1);

            // allocate a handle
            ushort handle = allocHandle();


            Handle pHandle = getHandle(handle);

            pHandle.m_uniqueId = (int)(handle);
            //pHandle->m_pOverlaps = 0;
            pHandle.m_clientObject = pOwner;
            pHandle.m_collisionFilterGroup = collisionFilterGroup;
            pHandle.m_collisionFilterMask = collisionFilterMask;
            pHandle.m_multiSapParentProxy = multiSapProxy;

            // compute current limit of edge arrays
            ushort limit = (ushort)(m_numHandles * 2);


            // insert new edges just inside the max boundary edge
            for (ushort axis = 0; axis < 3; axis++)
            {

                m_pHandles[0].m_maxEdges[axis] += 2;

                m_pEdges[axis][limit + 1] = m_pEdges[axis][limit - 1];

                m_pEdges[axis][limit - 1].m_pos = min[axis];
                m_pEdges[axis][limit - 1].m_handle = handle;

                m_pEdges[axis][limit].m_pos = max[axis];
                m_pEdges[axis][limit].m_handle = handle;

                pHandle.m_minEdges[axis] = (ushort)(limit - 1);
                pHandle.m_maxEdges[axis] = limit;
            }

            // now sort the new edges to their correct position
            sortMinDown(0, pHandle.m_minEdges[0], dispatcher, false);
            sortMaxDown(0, pHandle.m_maxEdges[0], dispatcher, false);
            sortMinDown(1, pHandle.m_minEdges[1], dispatcher, false);
            sortMaxDown(1, pHandle.m_maxEdges[1], dispatcher, false);
            sortMinDown(2, pHandle.m_minEdges[2], dispatcher, true);
            sortMaxDown(2, pHandle.m_maxEdges[2], dispatcher, true);


            return handle;
        }
        public void removeHandle(ushort handle, IDispatcher dispatcher)
        {

            Handle pHandle = getHandle(handle);

            //explicitly remove the pairs containing the proxy
            //we could do it also in the sortMinUp (passing true)
            ///@todo: compare performance
            if (!m_pairCache.hasDeferredRemoval())
            {
                m_pairCache.removeOverlappingPairsContainingProxy(pHandle, dispatcher);
            }

            // compute current limit of edge arrays
            int limit = (int)(m_numHandles * 2);

            int axis;

            for (axis = 0; axis < 3; axis++)
            {
                m_pHandles[0].m_maxEdges[axis] -= 2;
            }

            // remove the edges by sorting them up to the end of the list
            for (axis = 0; axis < 3; axis++)
            {
                ushort max = pHandle.m_maxEdges[axis];
                m_pEdges[axis][max].m_pos = m_handleSentinel;

                sortMaxUp(axis, max, dispatcher, false);


                ushort i = pHandle.m_minEdges[axis];
                m_pEdges[axis][i].m_pos = m_handleSentinel;


                sortMinUp(axis, i, dispatcher, false);

                m_pEdges[axis][limit - 1].m_handle = 0;
                m_pEdges[axis][limit - 1].m_pos = m_handleSentinel;

#if DEBUG_BROADPHASE
			        debugPrintAxis(axis,false);
#endif //DEBUG_BROADPHASE


            }
            // free the handle
            freeHandle(handle);

        }
        public void updateHandle(ushort handle, btVector3 aabbMin, btVector3 aabbMax, IDispatcher dispatcher)
        {
            Handle pHandle = getHandle(handle);

            // quantize the new bounds
            //ushort* min = stackalloc ushort[3];
            //ushort* max = stackalloc ushort[3];
            StackPtr<ushort> min = StackPtr<ushort>.Allocate(3);
            StackPtr<ushort> max = StackPtr<ushort>.Allocate(3);
            try
            {
                quantize(min, aabbMin, 0);
                quantize(max, aabbMax, 1);

                // update changed edges
                for (int axis = 0; axis < 3; axis++)
                {
                    ushort emin = pHandle.m_minEdges[axis];
                    ushort emax = pHandle.m_maxEdges[axis];

                    int dmin = Convert.ToInt32(min[axis]) - Convert.ToInt32(m_pEdges[axis][emin].m_pos);
                    int dmax = Convert.ToInt32(max[axis]) - Convert.ToInt32(m_pEdges[axis][emax].m_pos);

                    m_pEdges[axis][emin].m_pos = min[axis];
                    m_pEdges[axis][emax].m_pos = max[axis];

                    // expand (only adds overlaps)
                    if (dmin < 0)
                        sortMinDown(axis, emin, dispatcher, true);

                    if (dmax > 0)
                        sortMaxUp(axis, emax, dispatcher, true);

                    // shrink (only removes overlaps)
                    if (dmin > 0)
                        sortMinUp(axis, emin, dispatcher, true);

                    if (dmax < 0)
                        sortMaxDown(axis, emax, dispatcher, true);
                }
#if DEBUG_BROADPHASE
	        debugPrintAxis(axis);
#endif //DEBUG_BROADPHASE
            }
            finally
            {
                min.Dispose();
                max.Dispose();
            }
        }
        public Handle getHandle(ushort index)
        {
            return m_pHandles[index];
        }
        public virtual void resetPool(IDispatcher dispatcher)
        {
            if (m_numHandles == 0)
            {
                m_firstFreeHandle = 1;
                {
                    for (ushort i = m_firstFreeHandle; i < m_pHandles.Length; i++)
                        m_pHandles[i].SetNextFree((ushort)(i + 1));
                    m_pHandles[m_pHandles.Length - 1].SetNextFree(0);
                }
            }
        }
        public virtual BroadphaseProxy createProxy(ref btVector3 aabbMin,ref btVector3 aabbMax, BroadphaseNativeTypes shapeType, object userPtr, short collisionFilterGroup, short collisionFilterMask, IDispatcher dispatcher, object multiSapProxy)
        {
            ushort handleId = addHandle(aabbMin, aabbMax, userPtr, collisionFilterGroup, collisionFilterMask, dispatcher, multiSapProxy);

            Handle handle = getHandle(handleId);

            if (m_raycastAccelerator != null)
            {
                BroadphaseProxy rayProxy = m_raycastAccelerator.createProxy(ref aabbMin,ref aabbMax, shapeType, userPtr, collisionFilterGroup, collisionFilterMask, dispatcher, null);
                handle.m_dbvtProxy = rayProxy;
            }
            return handle;
        }
        public virtual void destroyProxy(BroadphaseProxy proxy, IDispatcher dispatcher)
        {
            Handle handle = (Handle)(proxy);
            if (m_raycastAccelerator != null)
                m_raycastAccelerator.destroyProxy(handle.m_dbvtProxy, dispatcher);
            removeHandle((ushort)(handle.m_uniqueId), dispatcher);
        }
        public virtual void setAabb(BroadphaseProxy proxy,ref btVector3 aabbMin,ref btVector3 aabbMax, IDispatcher dispatcher)
        {
            Handle handle = (Handle)proxy;
            handle.m_aabbMin = aabbMin;
            handle.m_aabbMax = aabbMax;
            updateHandle((ushort)handle.m_uniqueId, aabbMin, aabbMax, dispatcher);
            if (m_raycastAccelerator != null)
                m_raycastAccelerator.setAabb(handle.m_dbvtProxy,ref aabbMin,ref aabbMax, dispatcher);
        }
        public virtual void getAabb(BroadphaseProxy proxy, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            Handle pHandle = (Handle)(proxy);
            aabbMin = pHandle.m_aabbMin;
            aabbMax = pHandle.m_aabbMax;
        }
        public virtual void	rayTest(ref btVector3 rayFrom,ref btVector3 rayTo, BroadphaseRayCallback rayCallback,ref btVector3 aabbMin,ref btVector3 aabbMax)
        {
	        if (m_raycastAccelerator!=null)
	        {
		        m_raycastAccelerator.rayTest(ref rayFrom,ref rayTo,rayCallback,ref aabbMin,ref aabbMax);
	        } else
	        {
		        //choose axis?
		        ushort axis = 0;
		        //for each proxy
                for (ushort i = 1; i < m_numHandles * 2 + 1; i++)
		        {
			        if (m_pEdges[axis][i].IsMax)
			        {
				        rayCallback.process(getHandle(m_pEdges[axis][i].m_handle));
			        }
		        }
	        }
        }
        public virtual void	aabbTest(ref btVector3 aabbMin,ref btVector3 aabbMax, IBroadphaseAabbCallback callback)
        {
	        if (m_raycastAccelerator!=null)
	        {
		        m_raycastAccelerator.aabbTest(ref aabbMin,ref aabbMax,callback);
	        } else
	        {
		        //choose axis?
		        ushort axis = 0;
		        //for each proxy
		        for (ushort i=1;i<m_numHandles*2+1;i++)
		        {
			        if (m_pEdges[axis][i].IsMax)
			        {
				        Handle handle = getHandle(m_pEdges[axis][i].m_handle);
				        if (AabbUtil2.TestAabbAgainstAabb2(aabbMin,aabbMax,handle.m_aabbMin,handle.m_aabbMax))
				        {
					        callback.process(handle);
				        }
			        }
		        }
	        }
        }
        public void quantize(ushort[] outvalue, btVector3 point, int isMax)
        {
            btVector3 v = (point - m_worldAabbMin) * m_quantize;
            outvalue[0] = (v.X <= 0) ? (ushort)isMax : (v.X >= m_handleSentinel) ? (ushort)((m_handleSentinel & m_bpHandleMask) | isMax) : (ushort)(((ushort)v.X & m_bpHandleMask) | isMax);
            outvalue[1] = (v.Y <= 0) ? (ushort)isMax : (v.Y >= m_handleSentinel) ? (ushort)((m_handleSentinel & m_bpHandleMask) | isMax) : (ushort)(((ushort)v.Y & m_bpHandleMask) | isMax);
            outvalue[2] = (v.Z <= 0) ? (ushort)isMax : (v.Z >= m_handleSentinel) ? (ushort)((m_handleSentinel & m_bpHandleMask) | isMax) : (ushort)(((ushort)v.Z & m_bpHandleMask) | isMax);
        }
        /*public unsafe void quantize(ushort* outvalue, btVector3 point, int isMax)
        {
            btVector3 v = (point - m_worldAabbMin) * m_quantize;
            outvalue[0] = (v[0] <= 0) ? (ushort)isMax : (v[0] >= m_handleSentinel) ? (ushort)((m_handleSentinel & m_bpHandleMask) | isMax) : (ushort)(((ushort)v[0] & m_bpHandleMask) | isMax);
            outvalue[1] = (v[1] <= 0) ? (ushort)isMax : (v[1] >= m_handleSentinel) ? (ushort)((m_handleSentinel & m_bpHandleMask) | isMax) : (ushort)(((ushort)v[1] & m_bpHandleMask) | isMax);
            outvalue[2] = (v[2] <= 0) ? (ushort)isMax : (v[2] >= m_handleSentinel) ? (ushort)((m_handleSentinel & m_bpHandleMask) | isMax) : (ushort)(((ushort)v[2] & m_bpHandleMask) | isMax);
        }*/
#if false
        ///unQuantize should be conservative: aabbMin/aabbMax should be larger then 'getAabb' result
	    void unQuantize(btBroadphaseProxy* proxy,btVector3& aabbMin, btVector3& aabbMax ) const;
#endif
        public bool testAabbOverlap(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            Handle pHandleA = (Handle)(proxy0);
            Handle pHandleB = (Handle)(proxy1);

            //optimization 1: check the array index (memory address), instead of the m_pos

            for (int axis = 0; axis < 3; axis++)
            {
                if (pHandleA.m_maxEdges[axis] < pHandleB.m_minEdges[axis] ||
                    pHandleB.m_maxEdges[axis] < pHandleA.m_minEdges[axis])
                {
                    return false;
                }
            }
            return true;
        }
        public IOverlappingPairCache OverlappingPairCache
        {
            get { return m_pairCache; }
        }
        public IOverlappingPairCallback OverlappingPairUserCallback { get { return m_userPairCallback; } set { m_userPairCallback = value; } }
        ///getAabb returns the axis aligned bounding box in the 'global' coordinate frame
	    ///will add some transform later
        public virtual void getBroadphaseAabb(out btVector3 aabbMin, out btVector3 aabbMax)
        {
            aabbMin = m_worldAabbMin;
            aabbMax = m_worldAabbMax;
        }
        public virtual void printStats()
        {
            /*		printf("btAxisSweep3.h\n");
                    printf("numHandles = %d, maxHandles = %d\n",m_numHandles,m_maxHandles);
                    printf("aabbMin=%f,%f,%f,aabbMax=%f,%f,%f\n",m_worldAabbMin.getX(),m_worldAabbMin.getY(),m_worldAabbMin.getZ(),
                        m_worldAabbMax.getX(),m_worldAabbMax.getY(),m_worldAabbMax.getZ());
                        */

        }
    }
}
