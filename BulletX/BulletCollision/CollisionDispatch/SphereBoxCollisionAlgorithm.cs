using System;
using System.Collections.Generic;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class SphereBoxCollisionAlgorithm: ActivatingCollisionAlgorithm
    {
        //オブジェクトプール
        static Queue<SphereBoxCollisionAlgorithm> ObjPool
            = new Queue<SphereBoxCollisionAlgorithm>(new SphereBoxCollisionAlgorithm[1] { new SphereBoxCollisionAlgorithm() });
        static SphereBoxCollisionAlgorithm AllocFromPool(PersistentManifold mf,CollisionAlgorithmConstructionInfo ci,CollisionObject col0,CollisionObject col1, bool isSwapped)
        {
            SphereBoxCollisionAlgorithm result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new SphereBoxCollisionAlgorithm();
            result.Constructor(mf, ci, col0, col1,isSwapped);
            return result;
        }
        public override void free()
        {
            if (m_ownManifold)
	        {
		        if (m_manifoldPtr!=null)
			        m_dispatcher.releaseManifold(m_manifoldPtr);
	        }
            ObjPool.Enqueue(this);
        }

        bool	m_ownManifold;
	    PersistentManifold	m_manifoldPtr;
	    bool	m_isSwapped;
	
        SphereBoxCollisionAlgorithm() { }
        protected void Constructor(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject col0, CollisionObject col1, bool isSwapped)
        {
            base.Constructor(ci);
            m_ownManifold = false;
            m_manifoldPtr=mf;
            m_isSwapped=isSwapped;

	        CollisionObject sphereObj = m_isSwapped? col1 : col0;
	        CollisionObject boxObj = m_isSwapped? col0 : col1;
        	
	        if (m_manifoldPtr==null && m_dispatcher.needsCollision(sphereObj,boxObj))
	        {
		        m_manifoldPtr = m_dispatcher.getNewManifold(sphereObj,boxObj);
		        m_ownManifold = true;
	        }
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            if (m_manifoldPtr == null)
                return;

            CollisionObject sphereObj = m_isSwapped ? body1 : body0;
            CollisionObject boxObj = m_isSwapped ? body0 : body1;


            SphereShape sphere0 = (SphereShape)sphereObj.CollisionShape;

            //btVector3 normalOnSurfaceB;
            btVector3 pOnBox=btVector3.Zero, pOnSphere=btVector3.Zero;
            btVector3 sphereCenter = sphereObj.WorldTransform.Origin;
            float radius = sphere0.Radius;

            float dist = getSphereDistance(boxObj, ref pOnBox, ref pOnSphere, sphereCenter, radius);

            resultOut.PersistentManifold = m_manifoldPtr;

            if (dist < BulletGlobal.SIMD_EPSILON)
            {
                btVector3 normalOnSurfaceB;// = (pOnBox - pOnSphere).normalize();
                (pOnBox - pOnSphere).normalize(out normalOnSurfaceB);
                /// report a contact. internally this will be kept persistent, and contact reduction is done

                resultOut.addContactPoint(ref normalOnSurfaceB, ref pOnBox, dist);

            }

            if (m_ownManifold)
            {
                if (m_manifoldPtr.NumContacts != 0)
                {
                    resultOut.refreshContactPoints();
                }
            }
        }

        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            //not yet
	        return 1f;
        }

        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            if (m_manifoldPtr != null && m_ownManifold)
            {
                manifoldArray.Add(m_manifoldPtr);
            }
        }

        public float getSphereDistance(CollisionObject boxObj,ref btVector3 pointOnBox,ref btVector3 v3PointOnSphere, btVector3 sphereCenter, float fRadius ) 
        {

	        float margins;
	        //btVector3* bounds=stackalloc btVector3[2];
            //btVector3* boundsVec = stackalloc btVector3[2];
            StackPtr<btVector3> bounds = StackPtr<btVector3>.Allocate(2);
            StackPtr<btVector3> boundsVec = StackPtr<btVector3>.Allocate(2);
            try
            {
                BoxShape boxShape = (BoxShape)boxObj.CollisionShape;

                bounds[0] = -boxShape.HalfExtentsWithoutMargin;
                bounds[1] = boxShape.HalfExtentsWithoutMargin;

                margins = boxShape.Margin;//also add sphereShape margin?

                btTransform m44T = boxObj.WorldTransform;

                float fPenetration;

                boundsVec[0] = bounds[0];
                boundsVec[1] = bounds[1];

                btVector3 marginsVec = new btVector3(margins, margins, margins);

                // add margins
                //bounds[0] += marginsVec;
                //bounds[1] -= marginsVec;
                bounds[0].Add(ref marginsVec);
                bounds[1].Subtract(ref marginsVec);

                /////////////////////////////////////////////////

                btVector3 tmp, prel, /*n[6],*/ normal, v3P;
                //btVector3* n = stackalloc btVector3[6];
                StackPtr<btVector3> n = StackPtr<btVector3>.Allocate(6);
                try
                {
                    float fSep = 10000000.0f, fSepThis;

                    n[0] = new btVector3(-1.0f, 0.0f, 0.0f);
                    n[1] = new btVector3(0.0f, -1.0f, 0.0f);
                    n[2] = new btVector3(0.0f, 0.0f, -1.0f);
                    n[3] = new btVector3(1.0f, 0.0f, 0.0f);
                    n[4] = new btVector3(0.0f, 1.0f, 0.0f);
                    n[5] = new btVector3(0.0f, 0.0f, 1.0f);

                    // convert  point in local space
                    prel = m44T.invXform(sphereCenter);

                    bool bFound = false;

                    v3P = prel;

                    for (int i = 0; i < 6; i++)
                    {
                        int j = i < 3 ? 0 : 1;
                        if ((fSepThis = ((v3P - bounds[j]).dot(n[i]))) > 0.0f)
                        {
                            v3P = v3P - n[i] * fSepThis;
                            bFound = true;
                        }
                    }

                    //

                    if (bFound)
                    {
                        bounds[0] = boundsVec[0];
                        bounds[1] = boundsVec[1];

                        //normal = (prel - v3P).normalize();
                        (prel - v3P).normalize(out normal);
                        #region pointOnBox = v3P + normal * margins;
                        {
                            btVector3 temp;
                            btVector3.Multiply(ref normal, margins, out temp);
                            btVector3.Add(ref v3P, ref temp, out pointOnBox);
                        }
                        #endregion
                        #region v3PointOnSphere = prel - normal * fRadius;
                        {
                            btVector3 temp;
                            btVector3.Multiply(ref normal, fRadius, out temp);
                            btVector3.Subtract(ref prel, ref temp, out v3PointOnSphere);
                        }
                        #endregion

                        {
                            #region if (((v3PointOnSphere - pointOnBox).dot(normal)) > 0.0f)
                            btVector3 temp;
                            btVector3.Subtract(ref v3PointOnSphere, ref pointOnBox, out temp);
                            if (temp.dot(ref normal) > 0.0f)
                            #endregion
                            {
                                return 1.0f;
                            }
                        }
                        // transform back in world space
                        //tmp = m44T * pointOnBox;
                        btTransform.Multiply(ref m44T, ref pointOnBox, out tmp);
                        pointOnBox = tmp;
                        //tmp = m44T * v3PointOnSphere;
                        btTransform.Multiply(ref m44T, ref v3PointOnSphere, out tmp);
                        v3PointOnSphere = tmp;
                        float fSeps2 = (pointOnBox - v3PointOnSphere).Length2;

                        //if this fails, fallback into deeper penetration case, below
                        if (fSeps2 > BulletGlobal.SIMD_EPSILON)
                        {
                            fSep = -(float)Math.Sqrt(fSeps2);
                            normal = (pointOnBox - v3PointOnSphere);
                            normal *= 1f / fSep;
                        }

                        return fSep;
                    }

                    //////////////////////////////////////////////////
                    // Deep penetration case

                    fPenetration = getSpherePenetration(boxObj, ref pointOnBox, ref v3PointOnSphere, sphereCenter, fRadius, bounds[0], bounds[1]);

                    bounds[0] = boundsVec[0];
                    bounds[1] = boundsVec[1];

                    if (fPenetration <= 0.0f)
                        return (fPenetration - margins);
                    else
                        return 1.0f;
                }
                finally
                {
                    n.Dispose();
                }
            }
            finally
            {
                bounds.Dispose();
                boundsVec.Dispose();
            }
        }

        public unsafe float getSpherePenetration( CollisionObject boxObj,ref btVector3 pointOnBox,ref btVector3 v3PointOnSphere, btVector3 sphereCenter, float fRadius, btVector3 aabbMin, btVector3 aabbMax) 
        {

	        //btVector3* bounds=stackalloc btVector3[2];
            //btVector3* n = stackalloc btVector3[6];
            StackPtr<btVector3> bounds = StackPtr<btVector3>.Allocate(2);
            StackPtr<btVector3> n = StackPtr<btVector3>.Allocate(6);
            try
            {
                fixed (btVector3* n_ptr = &n.Array[0])
                {
                    bounds[0] = aabbMin;
                    bounds[1] = aabbMax;

                    btVector3 p0, tmp, prel, /*n[6],*/ normal_val;
                    btVector3* normal;
                    float fSep = -10000000.0f, fSepThis;

                    // set p0 and normal to a default value to shup up GCC
                    p0 = btVector3.Zero;
                    normal_val = btVector3.Zero;
                    normal = &normal_val;

                    n[0] = new btVector3((-1.0f), (0.0f), (0.0f));
                    n[1] = new btVector3((0.0f), (-1.0f), (0.0f));
                    n[2] = new btVector3((0.0f), (0.0f), (-1.0f));
                    n[3] = new btVector3((1.0f), (0.0f), (0.0f));
                    n[4] = new btVector3((0.0f), (1.0f), (0.0f));
                    n[5] = new btVector3((0.0f), (0.0f), (1.0f));

                    btTransform m44T = boxObj.WorldTransform;

                    // convert  point in local space
                    prel = m44T.invXform(sphereCenter);

                    ///////////

                    for (int i = 0; i < 6; i++)
                    {
                        int j = i < 3 ? 0 : 1;
                        if ((fSepThis = ((prel - bounds[j]).dot(n[i])) - fRadius) > 0.0f) return 1.0f;
                        if (fSepThis > fSep)
                        {
                            p0 = bounds[j]; normal = &n_ptr[i];
                            fSep = fSepThis;
                        }
                    }

                    pointOnBox = prel - (*normal) * (normal->dot((prel - p0)));
                    #region v3PointOnSphere = pointOnBox + (*normal) * fSep;
                    {
                        btVector3 temp;
                        btVector3.Multiply(ref *normal, fSep, out temp);
                        btVector3.Add(ref pointOnBox, ref temp, out v3PointOnSphere);
                    }
                    #endregion

                    // transform back in world space
                    //tmp = m44T * pointOnBox;
                    btTransform.Multiply(ref m44T, ref pointOnBox, out tmp);
                    pointOnBox = tmp;
                    //tmp = m44T * v3PointOnSphere;
                    btTransform.Multiply(ref m44T, ref v3PointOnSphere, out tmp);
                    v3PointOnSphere = tmp;
                    //*normal = (pointOnBox - v3PointOnSphere).normalize();
                    {
                        btVector3 temp;
                        btVector3.Subtract(ref pointOnBox, ref v3PointOnSphere, out temp);
                        temp.normalize(out *normal);
                    }
                    return fSep;
                }
            }
            finally
            {
                bounds.Dispose();
                n.Dispose();
            }
        }



        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                if (!m_swapped)
                    return AllocFromPool(null, ci, body0, body1, false);
                else
                    return AllocFromPool(null, ci, body0, body1, true);
            }
        }
    }
}
