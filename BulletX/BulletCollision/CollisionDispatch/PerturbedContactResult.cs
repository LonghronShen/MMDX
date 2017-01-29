using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    struct PerturbedContactResult
    {
        //ManifoldResult* m_originalManifoldResult;
        btTransform m_transformA;
        btTransform m_transformB;
        btTransform m_unPerturbedTransform;
        bool m_perturbA;
        IDebugDraw m_debugDrawer;


        public PerturbedContactResult(/*ManifoldResult* originalResult,*/ btTransform transformA, btTransform transformB, btTransform unPerturbedTransform, bool perturbA, IDebugDraw debugDrawer)
        {
            //m_originalManifoldResult = originalResult;
            m_transformA = transformA;
            m_transformB = transformB;
            m_unPerturbedTransform = unPerturbedTransform;
            m_perturbA = perturbA;
            m_debugDrawer = debugDrawer;

        }
        public void addContactPoint(btVector3 normalOnBInWorld,btVector3 pointInWorld,float orgDepth,ref ManifoldResult originalManifoldResult)
	    {
		    btVector3 endPt,startPt;
		    float newDepth;
		    //btVector3 newNormal;

		    if (m_perturbA)
            {
                btVector3 endPtOrg;// = pointInWorld + normalOnBInWorld*orgDepth;
                {
                    btVector3 temp;
                    btVector3.Multiply(ref normalOnBInWorld, orgDepth, out temp);
                    btVector3.Add(ref pointInWorld, ref temp, out endPtOrg);
                }
                endPt = btVector3.Transform(endPtOrg, m_unPerturbedTransform * m_transformA.inverse());
                #region newDepth = (endPt - pointInWorld).dot(normalOnBInWorld);
                {
                    btVector3 temp;
                    btVector3.Subtract(ref endPt, ref pointInWorld, out temp);
                    newDepth = temp.dot(ref normalOnBInWorld);
                }
                #endregion
                #region startPt = endPt + normalOnBInWorld * newDepth;
                {
                    btVector3 temp;
                    btVector3.Multiply(ref normalOnBInWorld, newDepth, out temp);
                    btVector3.Multiply(ref endPt, ref temp, out startPt);
                }
                #endregion
            }
            else
            {
                #region endPt = pointInWorld + normalOnBInWorld*orgDepth;
                {
                    btVector3 temp;
                    btVector3.Multiply(ref normalOnBInWorld, orgDepth, out temp);
                    btVector3.Add(ref pointInWorld, ref temp, out endPt);
                }
                #endregion
                startPt = btVector3.Transform(pointInWorld, m_unPerturbedTransform * m_transformB.inverse());
                #region newDepth = (endPt - startPt).dot(normalOnBInWorld);
                {
                    btVector3 temp;
                    btVector3.Subtract(ref endPt, ref startPt, out temp);
                    newDepth = temp.dot(ref normalOnBInWorld);
                }
                #endregion

            }
    		
		    originalManifoldResult.addContactPoint(ref normalOnBInWorld,ref startPt,newDepth);
	    }

    }
}
