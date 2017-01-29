using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    public abstract class CollisionShape
    {
        //member variable
        //protected int m_shapeType;//ShapeTypeに吸収
        public abstract BroadphaseNativeTypes ShapeType { get; }
        public object UserData { get; set; }

        //constructor
        public CollisionShape()
        {
            UserData = null;
        }

        ///getAabb returns the axis aligned bounding box in the coordinate frame of the given transform t.
        public abstract void getAabb(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax);

        public void getBoundingSphere(out btVector3 center, out float radius)
        {
            btTransform tr = btTransform.Identity;
            btVector3 aabbMin, aabbMax;

            getAabb(tr, out aabbMin, out aabbMax);

            #region radius = (aabbMax - aabbMin).Length * 0.5f;
            {
                btVector3 temp;
                btVector3.Subtract(ref aabbMax, ref aabbMin, out temp);
                radius = temp.Length * 0.5f;
            }
            #endregion
            #region center = (aabbMin + aabbMax) * 0.5f;
            {
                btVector3 temp;
                btVector3.Add(ref aabbMin, ref aabbMax, out temp);
                btVector3.Multiply(ref temp, 0.5f, out center);
            }
            #endregion
        }
        public virtual float getAngularMotionDisc()
        {
            ///@todo cache this value, to improve performance
            btVector3 center;
            float disc;
            getBoundingSphere(out center, out disc);
            disc += (center).Length;
            return disc;
        }
        public virtual float getContactBreakingThreshold(float defaultContactThreshold)
        {
            return getAngularMotionDisc() * defaultContactThreshold;
        }

        ///calculateTemporalAabb calculates the enclosing aabb for the moving object over interval [0..timeStep)
	    ///result is conservative
        public void calculateTemporalAabb(btTransform curTrans, btVector3 linvel, btVector3 angvel, float timeStep, out btVector3 temporalAabbMin, out btVector3 temporalAabbMax)
        {
            //start with static aabb
            getAabb(curTrans, out temporalAabbMin, out temporalAabbMax);

            float temporalAabbMaxx = temporalAabbMax.X;
            float temporalAabbMaxy = temporalAabbMax.Y;
            float temporalAabbMaxz = temporalAabbMax.Z;
            float temporalAabbMinx = temporalAabbMin.X;
            float temporalAabbMiny = temporalAabbMin.Y;
            float temporalAabbMinz = temporalAabbMin.Z;

            // add linear motion
            btVector3 linMotion;// = linvel * timeStep;
            btVector3.Multiply(ref linvel, timeStep, out linMotion);
            ///@todo: simd would have a vector max/min operation, instead of per-element access
            if (linMotion.X > 0f)
                temporalAabbMaxx += linMotion.X;
            else
                temporalAabbMinx += linMotion.X;
            if (linMotion.Y > 0f)
                temporalAabbMaxy += linMotion.Y;
            else
                temporalAabbMiny += linMotion.Y;
            if (linMotion.Z > 0f)
                temporalAabbMaxz += linMotion.Z;
            else
                temporalAabbMinz += linMotion.Z;

            //add conservative angular motion
            float angularMotion = angvel.Length * getAngularMotionDisc() * timeStep;
            btVector3 angularMotion3d = new btVector3(angularMotion, angularMotion, angularMotion);
            temporalAabbMin = new btVector3(temporalAabbMinx, temporalAabbMiny, temporalAabbMinz);
            temporalAabbMax = new btVector3(temporalAabbMaxx, temporalAabbMaxy, temporalAabbMaxz);

            //temporalAabbMin -= angularMotion3d;
            //temporalAabbMax += angularMotion3d;
            temporalAabbMin.Subtract(ref angularMotion3d);
            temporalAabbMax.Add(ref angularMotion3d);
        }

        public bool isPolyhedral { get { return BroadphaseProxy.isPolyhedral(ShapeType); } }
        public bool isConvex2d { get { return BroadphaseProxy.isConvex2d(ShapeType); } }
        public bool isConvex { get { return BroadphaseProxy.isConvex(ShapeType); } }
        public bool	isNonMoving { get {return BroadphaseProxy.isNonMoving(ShapeType);}}
        public bool isConcave { get { return BroadphaseProxy.isConcave(ShapeType); } }
        public bool isCompound { get { return BroadphaseProxy.isCompound(ShapeType); } }
	    public bool	isSoftBody { get {return BroadphaseProxy.isSoftBody(ShapeType);}}

	    ///isInfinite is used to catch simulation error (aabb check)
        public bool isInfinite { get { return BroadphaseProxy.isInfinite(ShapeType); } }

        public abstract btVector3 LocalScaling { get; set; }
        public abstract void calculateLocalInertia(float mass, out btVector3 inertia);

        
        //debugging support
        public abstract string Name { get; }

        public abstract float Margin { get; set; }

#if false
        virtual	int	calculateSerializeBufferSize() const;

	    ///fills the dataBuffer and returns the struct name (and 0 on failure)
	    virtual	const char*	serialize(void* dataBuffer, btSerializer* serializer) const;

	    virtual void	serializeSingleShape(btSerializer* serializer) const;
#endif
        

        
    }
}
