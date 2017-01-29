using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public enum DispatchFunc
    {
        DISPATCH_DISCRETE = 1,
        DISPATCH_CONTINUOUS
    };
    public class DispatcherInfo
    {
        public float m_timeStep;
        public int m_stepCount;
        public DispatchFunc m_dispatchFunc;
        public float m_timeOfImpact;
        public bool m_useContinuous;
        public IDebugDraw m_debugDraw;
        public bool m_enableSatConvex;
        public bool m_enableSPU;
        public bool m_useEpa;
        public float m_allowedCcdPenetration;
        public bool m_useConvexConservativeDistanceUtil;
        public float m_convexConservativeDistanceThreshold;
        //メモリ確保系？
        //btStackAlloc* m_stackAllocator;
        public DispatcherInfo()
        {
            m_timeStep = 0f;
            m_stepCount = 0;
            m_dispatchFunc = DispatchFunc.DISPATCH_DISCRETE;
            m_timeOfImpact = 1f;
            m_useContinuous = false;
            m_debugDraw = null;
            m_enableSatConvex = false;
            m_enableSPU = true;
            m_useEpa = true;
            m_allowedCcdPenetration = 0.04f;
            m_useConvexConservativeDistanceUtil = false;
            m_convexConservativeDistanceThreshold = 0.0f;
        }
    }
}
