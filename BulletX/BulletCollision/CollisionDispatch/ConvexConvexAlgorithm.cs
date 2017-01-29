using System;
using System.Collections.Generic;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.BulletCollision.NarrowPhaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionDispatch
{
    class ConvexConvexAlgorithm : ActivatingCollisionAlgorithm
    {
        //オブジェクトプール
        static Queue<ConvexConvexAlgorithm> ObjPool
            = new Queue<ConvexConvexAlgorithm>(new ConvexConvexAlgorithm[3] { new ConvexConvexAlgorithm(), new ConvexConvexAlgorithm(), new ConvexConvexAlgorithm() });

        static CollisionAlgorithm AllocFromPool(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1, ISimplexSolver simplexSolver, IConvexPenetrationDepthSolver pdSolver, int numPerturbationIterations, int minimumPointsPerturbationThreshold)
        {
            ConvexConvexAlgorithm result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new ConvexConvexAlgorithm();
            result.Constructor(mf, ci, body0, body1, simplexSolver, pdSolver, numPerturbationIterations, minimumPointsPerturbationThreshold);
            return result;
        }
        public override void free()
        {
            if (m_ownManifold)
            {
                if (m_manifoldPtr != null)
                    m_dispatcher.releaseManifold(m_manifoldPtr);
            }
            ObjPool.Enqueue(this);
        }

        //メンバ変数
        ISimplexSolver m_simplexSolver;
        IConvexPenetrationDepthSolver m_pdSolver;


        bool m_ownManifold;
        PersistentManifold m_manifoldPtr;
        bool m_lowLevelOfDetail;

        int m_numPerturbationIterations;
        int m_minimumPointsPerturbationThreshold;

        public ConvexConvexAlgorithm() { }
        //オブジェクトプールを使うので初期化処理の代わりを……
        public void Constructor(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1, ISimplexSolver simplexSolver, IConvexPenetrationDepthSolver pdSolver, int numPerturbationIterations, int minimumPointsPerturbationThreshold)
        {
            base.Constructor(ci);
            m_simplexSolver = simplexSolver;
            m_pdSolver = pdSolver;
            m_ownManifold = false;
            m_manifoldPtr = mf;
            m_lowLevelOfDetail = false;
            m_numPerturbationIterations = numPerturbationIterations;
            m_minimumPointsPerturbationThreshold = minimumPointsPerturbationThreshold;
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            if (m_manifoldPtr == null)
            {
                //swapped?
                m_manifoldPtr = m_dispatcher.getNewManifold(body0, body1);
                m_ownManifold = true;
            }
            resultOut.PersistentManifold = m_manifoldPtr;

            //comment-out next line to test multi-contact generation
            //resultOut->getPersistentManifold()->clearManifold();


            ConvexShape min0 = (ConvexShape)(body0.CollisionShape);
            ConvexShape min1 = (ConvexShape)(body1.CollisionShape);

            btVector3 normalOnB;
            btVector3 pointOnBWorld;
            if ((min0.ShapeType == BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE) && (min1.ShapeType == BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE))
            {
                CapsuleShape capsuleA = (CapsuleShape)min0;
                CapsuleShape capsuleB = (CapsuleShape)min1;
                btVector3 localScalingA = capsuleA.LocalScaling;
                btVector3 localScalingB = capsuleB.LocalScaling;

                float threshold = m_manifoldPtr.ContactBreakingThreshold;

                float dist = capsuleCapsuleDistance(out normalOnB, out	pointOnBWorld, capsuleA.HalfHeight, capsuleA.Radius,
                    capsuleB.HalfHeight, capsuleB.Radius, capsuleA.UpAxis, capsuleB.UpAxis,
                    body0.WorldTransform, body1.WorldTransform, threshold);

                if (dist < threshold)
                {
                    Debug.Assert(normalOnB.Length2 >= (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON));
                    resultOut.addContactPoint(ref normalOnB, ref pointOnBWorld, dist);
                }
                resultOut.refreshContactPoints();
                return;
            }


#if USE_SEPDISTANCE_UTIL2
	        if (dispatchInfo.m_useConvexConservativeDistanceUtil)
	        {
		        m_sepDistance.updateSeparatingDistance(body0->getWorldTransform(),body1->getWorldTransform());
	        }

        	if (!dispatchInfo.m_useConvexConservativeDistanceUtil || m_sepDistance.getConservativeSeparatingDistance()<=0.f)
#endif //USE_SEPDISTANCE_UTIL2
            {


                ClosestPointInput input;

                GjkPairDetector gjkPairDetector = new GjkPairDetector(min0, min1, m_simplexSolver, m_pdSolver);
                //TODO: if (dispatchInfo.m_useContinuous)
                gjkPairDetector.MinkowskiA = min0;
                gjkPairDetector.MinkowskiB = min1;

#if USE_SEPDISTANCE_UTIL2
	            if (dispatchInfo.m_useConvexConservativeDistanceUtil)
	            {
		            input.m_maximumDistanceSquared = BT_LARGE_FLOAT;
	            } else
#endif //USE_SEPDISTANCE_UTIL2
                {
                    input.m_maximumDistanceSquared = min0.Margin + min1.Margin + m_manifoldPtr.ContactBreakingThreshold;
                    input.m_maximumDistanceSquared *= input.m_maximumDistanceSquared;
                }

                //input.m_stackAlloc = dispatchInfo.m_stackAllocator;
                input.m_transformA = body0.WorldTransform;
                input.m_transformB = body1.WorldTransform;

                gjkPairDetector.getClosestPoints(ref input, ref resultOut, dispatchInfo.m_debugDraw);

#if USE_SEPDISTANCE_UTIL2
	            btScalar sepDist = 0.f;
	            if (dispatchInfo.m_useConvexConservativeDistanceUtil)
	            {
		            sepDist = gjkPairDetector.getCachedSeparatingDistance();
		            if (sepDist>SIMD_EPSILON)
		            {
			            sepDist += dispatchInfo.m_convexConservativeDistanceThreshold;
			            //now perturbe directions to get multiple contact points
            			
		            }
	            }
#endif //USE_SEPDISTANCE_UTIL2


                //now perform 'm_numPerturbationIterations' collision queries with the perturbated collision objects

                //perform perturbation when more then 'm_minimumPointsPerturbationThreshold' points
                if (m_numPerturbationIterations != 0 && resultOut.PersistentManifold.NumContacts < m_minimumPointsPerturbationThreshold)
                {

                    int i;
                    btVector3 v0, v1;
                    btVector3 sepNormalWorldSpace;

                    //sepNormalWorldSpace = gjkPairDetector.getCachedSeparatingAxis().normalized();
                    gjkPairDetector.getCachedSeparatingAxis().normalized(out sepNormalWorldSpace);
                    btVector3.PlaneSpace1(ref sepNormalWorldSpace, out v0, out v1);


                    bool perturbeA = true;
                    float angleLimit = 0.125f * BulletGlobal.SIMD_PI;
                    float perturbeAngle;
                    float radiusA = min0.getAngularMotionDisc();
                    float radiusB = min1.getAngularMotionDisc();
                    if (radiusA < radiusB)
                    {
                        perturbeAngle = PersistentManifold.gContactBreakingThreshold / radiusA;
                        perturbeA = true;
                    }
                    else
                    {
                        perturbeAngle = PersistentManifold.gContactBreakingThreshold / radiusB;
                        perturbeA = false;
                    }
                    if (perturbeAngle > angleLimit)
                        perturbeAngle = angleLimit;

                    btTransform unPerturbedTransform;
                    if (perturbeA)
                    {
                        unPerturbedTransform = input.m_transformA;
                    }
                    else
                    {
                        unPerturbedTransform = input.m_transformB;
                    }

                    for (i = 0; i < m_numPerturbationIterations; i++)
                    {
                        if (v0.Length2 > BulletGlobal.SIMD_EPSILON)
                        {
                            btQuaternion perturbeRot = new btQuaternion(v0, perturbeAngle);
                            float iterationAngle = i * (BulletGlobal.SIMD_2_PI / m_numPerturbationIterations);
                            btQuaternion rotq = new btQuaternion(sepNormalWorldSpace, iterationAngle);


                            if (perturbeA)
                            {
                                #region input.m_transformA.Basis = new btMatrix3x3(rotq.inverse() * perturbeRot * rotq) * body0.WorldTransform.Basis;
                                {
                                    btMatrix3x3 temp = new btMatrix3x3(rotq.inverse() * perturbeRot * rotq);
                                    btMatrix3x3.Multiply(ref temp, ref body0.WorldTransform.Basis, out input.m_transformA.Basis);
                                }
                                #endregion
                                input.m_transformB = body1.WorldTransform;
#if DEBUG
                                dispatchInfo.m_debugDraw.drawTransform(ref input.m_transformA, 10.0f);
#endif //DEBUG_CONTACTS
                            }
                            else
                            {
                                input.m_transformA = body0.WorldTransform;
                                #region input.m_transformB.Basis = new btMatrix3x3(rotq.inverse() * perturbeRot * rotq) * body1.WorldTransform.Basis;
                                {
                                    btMatrix3x3 temp = new btMatrix3x3(rotq.inverse() * perturbeRot * rotq);
                                    btMatrix3x3.Multiply(ref temp, ref body1.WorldTransform.Basis, out input.m_transformB.Basis);
                                }
                                #endregion
#if DEBUG
                                dispatchInfo.m_debugDraw.drawTransform(ref input.m_transformB, 10.0f);
#endif
                            }
                            PerturbedContactResult perturbedResultOut = new PerturbedContactResult(input.m_transformA, input.m_transformB, unPerturbedTransform, perturbeA, dispatchInfo.m_debugDraw);
                            gjkPairDetector.getClosestPoints(ref input, ref perturbedResultOut, ref resultOut, dispatchInfo.m_debugDraw);

                        }

                    }
                }


#if USE_SEPDISTANCE_UTIL2
	            if (dispatchInfo.m_useConvexConservativeDistanceUtil && (sepDist>SIMD_EPSILON))
	            {
		            m_sepDistance.initSeparatingDistance(gjkPairDetector.getCachedSeparatingAxis(),sepDist,body0->getWorldTransform(),body1->getWorldTransform());
	            }
#endif //USE_SEPDISTANCE_UTIL2



            }

            if (m_ownManifold)
            {
                resultOut.refreshContactPoints();
            }
        }

        public override float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ref ManifoldResult resultOut)
        {
            throw new NotImplementedException();

#if false//未移植
            ///Rather then checking ALL pairs, only calculate TOI when motion exceeds threshold
    
	        ///Linear motion for one of objects needs to exceed m_ccdSquareMotionThreshold
	        ///col0->m_worldTransform,
	        float resultFraction = btScalar(1.);


	        float squareMot0 = (col0->getInterpolationWorldTransform().getOrigin() - col0->getWorldTransform().getOrigin()).length2();
	        float squareMot1 = (col1->getInterpolationWorldTransform().getOrigin() - col1->getWorldTransform().getOrigin()).length2();
            
	        if (squareMot0 < col0->getCcdSquareMotionThreshold() &&
		        squareMot1 < col1->getCcdSquareMotionThreshold())
		        return resultFraction;

	        if (disableCcd)
		        return btScalar(1.);


	        //An adhoc way of testing the Continuous Collision Detection algorithms
	        //One object is approximated as a sphere, to simplify things
	        //Starting in penetration should report no time of impact
	        //For proper CCD, better accuracy and handling of 'allowed' penetration should be added
	        //also the mainloop of the physics should have a kind of toi queue (something like Brian Mirtich's application of Timewarp for Rigidbodies)

        		
	        /// Convex0 against sphere for Convex1
	        {
		        ConvexShape convex0 = static_cast<btConvexShape*>(col0->getCollisionShape());

		        SphereShape	sphere1(col1->getCcdSweptSphereRadius()); //todo: allow non-zero sphere sizes, for better approximation
		        btConvexCast::CastResult result;
		        btVoronoiSimplexSolver voronoiSimplex;
		        //SubsimplexConvexCast ccd0(&sphere,min0,&voronoiSimplex);
		        ///Simplification, one object is simplified as a sphere
		        btGjkConvexCast ccd1( convex0 ,&sphere1,&voronoiSimplex);
		        //ContinuousConvexCollision ccd(min0,min1,&voronoiSimplex,0);
		        if (ccd1.calcTimeOfImpact(col0->getWorldTransform(),col0->getInterpolationWorldTransform(),
			        col1->getWorldTransform(),col1->getInterpolationWorldTransform(),result))
		        {
        		
			        //store result.m_fraction in both bodies
        		
			        if (col0->getHitFraction()> result.m_fraction)
				        col0->setHitFraction( result.m_fraction );

			        if (col1->getHitFraction() > result.m_fraction)
				        col1->setHitFraction( result.m_fraction);

			        if (resultFraction > result.m_fraction)
				        resultFraction = result.m_fraction;

		        }
        		
        		


	        }

	        /// Sphere (for convex0) against Convex1
	        {
		        btConvexShape* convex1 = static_cast<btConvexShape*>(col1->getCollisionShape());

		        btSphereShape	sphere0(col0->getCcdSweptSphereRadius()); //todo: allow non-zero sphere sizes, for better approximation
		        btConvexCast::CastResult result;
		        btVoronoiSimplexSolver voronoiSimplex;
		        //SubsimplexConvexCast ccd0(&sphere,min0,&voronoiSimplex);
		        ///Simplification, one object is simplified as a sphere
		        btGjkConvexCast ccd1(&sphere0,convex1,&voronoiSimplex);
		        //ContinuousConvexCollision ccd(min0,min1,&voronoiSimplex,0);
		        if (ccd1.calcTimeOfImpact(col0->getWorldTransform(),col0->getInterpolationWorldTransform(),
			        col1->getWorldTransform(),col1->getInterpolationWorldTransform(),result))
		        {
        		
			        //store result.m_fraction in both bodies
        		
			        if (col0->getHitFraction()	> result.m_fraction)
				        col0->setHitFraction( result.m_fraction);

			        if (col1->getHitFraction() > result.m_fraction)
				        col1->setHitFraction( result.m_fraction);

			        if (resultFraction > result.m_fraction)
				        resultFraction = result.m_fraction;

		        }
	        }
        	
	        return resultFraction;
#endif
        }
        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            //should we use m_ownManifold to avoid adding duplicates?
            if (m_manifoldPtr != null && m_ownManifold)
                manifoldArray.Add(m_manifoldPtr);
        }
        public bool LowLevelOfDetail { set { m_lowLevelOfDetail = value; } }
        public PersistentManifold Manifold { get { return m_manifoldPtr; } }

        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            IConvexPenetrationDepthSolver m_pdSolver;
            ISimplexSolver m_simplexSolver;
            int m_numPerturbationIterations;
            int m_minimumPointsPerturbationThreshold;

            public CreateFunc(ISimplexSolver simplexSolver, IConvexPenetrationDepthSolver pdSolver)
            {
                m_numPerturbationIterations = 0;
                m_minimumPointsPerturbationThreshold = 3;
                m_simplexSolver = simplexSolver;
                m_pdSolver = pdSolver;
            }

            public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                return AllocFromPool(ci.m_manifold, ci, body0, body1, m_simplexSolver, m_pdSolver, m_numPerturbationIterations, m_minimumPointsPerturbationThreshold);
            }

        }


        #region statics
        static float capsuleCapsuleDistance(
        out btVector3 normalOnB,
        out btVector3 pointOnB,
        float capsuleLengthA,
        float capsuleRadiusA,
        float capsuleLengthB,
        float capsuleRadiusB,
        int capsuleAxisA,
        int capsuleAxisB,
        btTransform transformA,
        btTransform transformB,
        float distanceThreshold)
        {
            btVector3 directionA;// = transformA.Basis.getColumn(capsuleAxisA);
            transformA.Basis.getColumn(capsuleAxisA, out directionA);
            btVector3 translationA = transformA.Origin;
            btVector3 directionB;// = transformB.Basis.getColumn(capsuleAxisB);
            transformB.Basis.getColumn(capsuleAxisB, out directionB);
            btVector3 translationB = transformB.Origin;

            // translation between centers

            btVector3 translation = translationB - translationA;

            // compute the closest points of the capsule line segments

            btVector3 ptsVector;           // the vector between the closest points

            btVector3 offsetA, offsetB;    // offsets from segment centers to their closest points
            float tA, tB;              // parameters on line segment

            segmentsClosestPoints(out ptsVector, out offsetA, out offsetB, out tA, out  tB, ref translation,
                                  ref directionA, capsuleLengthA, ref directionB, capsuleLengthB);

            float distance = ptsVector.Length - capsuleRadiusA - capsuleRadiusB;

            if (distance > distanceThreshold)
            {
                normalOnB = btVector3.Zero;
                pointOnB = btVector3.Zero;
                return distance;
            }

            float lenSqr = ptsVector.Length2;
            if (lenSqr <= (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
            {
                //degenerate case where 2 capsules are likely at the same location: take a vector tangential to 'directionA'
                btVector3 q;
                btVector3.PlaneSpace1(ref directionA, out normalOnB, out q);
            }
            else
            {
                // compute the contact normal
                //normalOnB = ptsVector * (-1.0f / (float)Math.Sqrt(lenSqr));
                btVector3.Multiply(ref ptsVector, (-1.0f / (float)Math.Sqrt(lenSqr)), out normalOnB);
            }
            #region pointOnB = transformB.Origin + offsetB + normalOnB * capsuleRadiusB;
            {
                btVector3 temp1, temp2;
                btVector3.Add(ref transformB.Origin, ref offsetB, out temp1);
                btVector3.Multiply(ref normalOnB, capsuleRadiusB, out temp2);
                btVector3.Add(ref temp1, ref temp2, out pointOnB);
            }
            #endregion
            return distance;
        }
        static void segmentsClosestPoints(
                    out btVector3 ptsVector,
                    out btVector3 offsetA,
                    out btVector3 offsetB,
                    out float tA, out float tB,
                    ref btVector3 translation,
                    ref btVector3 dirA, float hlenA,
                    ref btVector3 dirB, float hlenB)
        {
            // compute the parameters of the closest points on each line segment

            float dirA_dot_dirB = dirA.dot(dirB);
            float dirA_dot_trans = dirA.dot(translation);
            float dirB_dot_trans = dirB.dot(translation);

            float denom = 1.0f - dirA_dot_dirB * dirA_dot_dirB;

            if (denom == 0.0f)
            {
                tA = 0.0f;
            }
            else
            {
                tA = (dirA_dot_trans - dirB_dot_trans * dirA_dot_dirB) / denom;
                if (tA < -hlenA)
                    tA = -hlenA;
                else if (tA > hlenA)
                    tA = hlenA;
            }

            tB = tA * dirA_dot_dirB - dirB_dot_trans;

            if (tB < -hlenB)
            {
                tB = -hlenB;
                tA = tB * dirA_dot_dirB + dirA_dot_trans;

                if (tA < -hlenA)
                    tA = -hlenA;
                else if (tA > hlenA)
                    tA = hlenA;
            }
            else if (tB > hlenB)
            {
                tB = hlenB;
                tA = tB * dirA_dot_dirB + dirA_dot_trans;

                if (tA < -hlenA)
                    tA = -hlenA;
                else if (tA > hlenA)
                    tA = hlenA;
            }

            // compute the closest points relative to segment centers.

            //offsetA = dirA * tA;
            btVector3.Multiply(ref dirA, tA, out offsetA);
            //offsetB = dirB * tB;
            btVector3.Multiply(ref dirB, tB, out offsetB);

            #region ptsVector = translation - offsetA + offsetB;
            {
                btVector3 temp;
                btVector3.Subtract(ref translation, ref offsetA, out temp);
                btVector3.Add(ref temp, ref offsetB, out ptsVector);
            }
            #endregion
        }
        #endregion

    }
}
