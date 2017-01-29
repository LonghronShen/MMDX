using System;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.BulletCollision.CollisionDispatch;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    struct GjkPairDetector
    {
        const float REL_ERROR2 = 1.0e-6f;

        btVector3 m_cachedSeparatingAxis;
        IConvexPenetrationDepthSolver m_penetrationDepthSolver;
        ISimplexSolver m_simplexSolver;
        ConvexShape m_minkowskiA;
        ConvexShape m_minkowskiB;
        BroadphaseNativeTypes m_shapeTypeA;
        BroadphaseNativeTypes m_shapeTypeB;
        float m_marginA;
        float m_marginB;

        bool m_ignoreMargin;
        float m_cachedSeparatingDistance;
        //some debugging to fix degeneracy problems
        public int m_lastUsedMethod;
        public int m_curIter;
        public int m_degenerateSimplex;
        public int m_catchDegeneracies;

        public GjkPairDetector(ConvexShape objectA, ConvexShape objectB, ISimplexSolver simplexSolver, IConvexPenetrationDepthSolver penetrationDepthSolver)
        {
            m_cachedSeparatingAxis = new btVector3(0f, 1f, 0f);
            m_penetrationDepthSolver = penetrationDepthSolver;
            m_simplexSolver = simplexSolver;
            m_minkowskiA = objectA;
            m_minkowskiB = objectB;
            m_shapeTypeA = objectA.ShapeType;
            m_shapeTypeB = objectB.ShapeType;
            m_marginA = objectA.Margin;
            m_marginB = objectB.Margin;
            m_ignoreMargin = false;
            m_lastUsedMethod = -1;
            m_catchDegeneracies = 1;

            m_cachedSeparatingDistance = 0f;
            m_curIter = -1;
            m_degenerateSimplex = -1;
        }

        public ConvexShape MinkowskiA { set { m_minkowskiA = value; } }

        public ConvexShape MinkowskiB { set { m_minkowskiB = value; } }
        
        public btVector3 getCachedSeparatingAxis()
        {
            throw new NotImplementedException();
        }

        public void getClosestPoints(ref ClosestPointInput input, ref PerturbedContactResult output, ref ManifoldResult originalManifoldResult, IDebugDraw debugDraw)
        {
            m_cachedSeparatingDistance = 0f;

            float distance = 0f;
            btVector3 normalInB = new btVector3(0f, 0f, 0f);
            btVector3 pointOnA, pointOnB = btVector3.Zero;
            btTransform localTransA = input.m_transformA;
            btTransform localTransB = input.m_transformB;
            btVector3 positionOffset;// = (localTransA.Origin + localTransB.Origin) * 0.5f;
            {
                btVector3 temp;
                btVector3.Add(ref localTransA.Origin, ref localTransB.Origin, out temp);
                btVector3.Multiply(ref temp, 0.5f, out positionOffset);
            }
            localTransA.Origin -= positionOffset;
            localTransB.Origin -= positionOffset;

            bool check2d = m_minkowskiA.isConvex2d && m_minkowskiB.isConvex2d;

            float marginA = m_marginA;
            float marginB = m_marginB;

            //gNumGjkChecks++;
            //for CCD we don't use margins
            if (m_ignoreMargin)
            {
                marginA = 0f;
                marginB = 0f;

            }

            m_curIter = 0;
            int gGjkMaxIter = 1000;//this is to catch invalid input, perhaps check for #NaN?
            m_cachedSeparatingAxis.setValue(0, 1, 0);

            bool isValid = false;
            bool checkSimplex = false;
            bool checkPenetration = true;
            m_degenerateSimplex = 0;

            m_lastUsedMethod = -1;

            {
                float squaredDistance = BulletGlobal.BT_LARGE_FLOAT;
                float delta = 0f;

                float margin = marginA + marginB;



                m_simplexSolver.reset();

                for (; ; )
                //while (true)
                {

                    btVector3 seperatingAxisInA;// = (-m_cachedSeparatingAxis) * input.m_transformA.Basis;
                    {
                        btVector3 temp = -m_cachedSeparatingAxis;
                        btMatrix3x3.Multiply(ref temp, ref input.m_transformA.Basis, out seperatingAxisInA);
                    }
                    btVector3 seperatingAxisInB;// = m_cachedSeparatingAxis * input.m_transformB.Basis;
                    btMatrix3x3.Multiply(ref m_cachedSeparatingAxis, ref input.m_transformB.Basis, out seperatingAxisInB);

                    btVector3 pInA;// = m_minkowskiA.localGetSupportVertexWithoutMarginNonVirtual(seperatingAxisInA);
                    m_minkowskiA.localGetSupportVertexWithoutMarginNonVirtual(ref seperatingAxisInA, out pInA);
                    btVector3 qInB;// = m_minkowskiB.localGetSupportVertexWithoutMarginNonVirtual(seperatingAxisInB);
                    m_minkowskiB.localGetSupportVertexWithoutMarginNonVirtual(ref seperatingAxisInB, out qInB);

                    btVector3 pWorld = btVector3.Transform(pInA, localTransA);
                    btVector3 qWorld = btVector3.Transform(qInB, localTransB);

                    if (check2d)
                    {
                        pWorld.Z = 0f;
                        qWorld.Z = 0f;
                    }

                    btVector3 w = pWorld - qWorld;
                    delta = m_cachedSeparatingAxis.dot(w);

                    // potential exit, they don't overlap
                    if ((delta > 0.0f) && (delta * delta > squaredDistance * input.m_maximumDistanceSquared))
                    {
                        m_degenerateSimplex = 10;
                        checkSimplex = true;
                        //checkPenetration = false;
                        break;
                    }

                    //exit 0: the new point is already in the simplex, or we didn't come any closer
                    if (m_simplexSolver.inSimplex(w))
                    {
                        m_degenerateSimplex = 1;
                        checkSimplex = true;
                        break;
                    }
                    // are we getting any closer ?
                    float f0 = squaredDistance - delta;
                    float f1 = squaredDistance * REL_ERROR2;

                    if (f0 <= f1)
                    {
                        if (f0 <= 0f)
                        {
                            m_degenerateSimplex = 2;
                        }
                        else
                        {
                            m_degenerateSimplex = 11;
                        }
                        checkSimplex = true;
                        break;
                    }
                    //add current vertex to simplex
                    m_simplexSolver.addVertex(w, pWorld, qWorld);

                    btVector3 newCachedSeparatingAxis;

                    //calculate the closest point to the origin (update vector v)
                    if (!m_simplexSolver.closest(out newCachedSeparatingAxis))
                    {
                        m_degenerateSimplex = 3;
                        checkSimplex = true;
                        break;
                    }

                    if (newCachedSeparatingAxis.Length2 < REL_ERROR2)
                    {
                        m_cachedSeparatingAxis = newCachedSeparatingAxis;
                        m_degenerateSimplex = 6;
                        checkSimplex = true;
                        break;
                    }

                    float previousSquaredDistance = squaredDistance;
                    squaredDistance = newCachedSeparatingAxis.Length2;

                    m_cachedSeparatingAxis = newCachedSeparatingAxis;

                    //redundant m_simplexSolver->compute_points(pointOnA, pointOnB);

                    //are we getting any closer ?
                    if (previousSquaredDistance - squaredDistance <= BulletGlobal.SIMD_EPSILON * previousSquaredDistance)
                    {
                        m_simplexSolver.backup_closest(ref m_cachedSeparatingAxis);
                        checkSimplex = true;
                        m_degenerateSimplex = 12;

                        break;
                    }

                    //degeneracy, this is typically due to invalid/uninitialized worldtransforms for a btCollisionObject   
                    if (m_curIter++ > gGjkMaxIter)
                    {
                        /*#if defined(DEBUG) || defined (_DEBUG) || defined (DEBUG_SPU_COLLISION_DETECTION)

                                printf("btGjkPairDetector maxIter exceeded:%i\n",m_curIter);   
                                printf("sepAxis=(%f,%f,%f), squaredDistance = %f, shapeTypeA=%i,shapeTypeB=%i\n",   
                                m_cachedSeparatingAxis.getX(),   
                                m_cachedSeparatingAxis.getY(),   
                                m_cachedSeparatingAxis.getZ(),   
                                squaredDistance,   
                                m_minkowskiA->getShapeType(),   
                                m_minkowskiB->getShapeType());   

                        #endif   */
                        break;

                    }


                    bool check = (!m_simplexSolver.fullSimplex);
                    //bool check = (!m_simplexSolver->fullSimplex() && squaredDistance > SIMD_EPSILON * m_simplexSolver->maxVertex());

                    if (!check)
                    {
                        //do we need this backup_closest here ?
                        m_simplexSolver.backup_closest(ref m_cachedSeparatingAxis);
                        m_degenerateSimplex = 13;
                        break;
                    }
                }

                if (checkSimplex)
                {
                    m_simplexSolver.compute_points(out pointOnA, out pointOnB);
                    normalInB = pointOnA - pointOnB;
                    float lenSqr = m_cachedSeparatingAxis.Length2;

                    //valid normal
                    if (lenSqr < 0.0001)
                    {
                        m_degenerateSimplex = 5;
                    }
                    if (lenSqr > BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON)
                    {
                        float rlen = 1f / (float)Math.Sqrt(lenSqr);
                        normalInB *= rlen; //normalize
                        float s = (float)Math.Sqrt(squaredDistance);

                        Debug.Assert(s > 0.0f);
                        pointOnA -= m_cachedSeparatingAxis * (marginA / s);
                        pointOnB += m_cachedSeparatingAxis * (marginB / s);
                        distance = ((1f / rlen) - margin);
                        isValid = true;

                        m_lastUsedMethod = 1;
                    }
                    else
                    {
                        m_lastUsedMethod = 2;
                    }
                }

                bool catchDegeneratePenetrationCase =
                    (m_catchDegeneracies != 0 && m_penetrationDepthSolver != null && m_degenerateSimplex != 0 && ((distance + margin) < 0.01));

                //if (checkPenetration && !isValid)
                if (checkPenetration && (!isValid || catchDegeneratePenetrationCase))
                {
                    //penetration case

                    //if there is no way to handle penetrations, bail out
                    if (m_penetrationDepthSolver != null)
                    {
                        // Penetration depth case.
                        btVector3 tmpPointOnA, tmpPointOnB;

                        //gNumDeepPenetrationChecks++;
                        m_cachedSeparatingAxis.setZero();

                        bool isValid2 = m_penetrationDepthSolver.calcPenDepth(
                            m_simplexSolver,
                            m_minkowskiA, m_minkowskiB,
                            localTransA, localTransB,
                            ref m_cachedSeparatingAxis, out tmpPointOnA, out tmpPointOnB,
                            debugDraw//, input.m_stackAlloc
                            );


                        if (isValid2)
                        {
                            btVector3 tmpNormalInB = tmpPointOnB - tmpPointOnA;
                            float lenSqr = tmpNormalInB.Length2;
                            if (lenSqr <= (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
                            {
                                tmpNormalInB = m_cachedSeparatingAxis;
                                lenSqr = m_cachedSeparatingAxis.Length2;
                            }

                            if (lenSqr > (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
                            {
                                tmpNormalInB /= (float)Math.Sqrt(lenSqr);
                                float distance2 = -(tmpPointOnA - tmpPointOnB).Length;
                                //only replace valid penetrations when the result is deeper (check)
                                if (!isValid || (distance2 < distance))
                                {
                                    distance = distance2;
                                    pointOnA = tmpPointOnA;
                                    pointOnB = tmpPointOnB;
                                    normalInB = tmpNormalInB;
                                    isValid = true;
                                    m_lastUsedMethod = 3;
                                }
                                else
                                {
                                    m_lastUsedMethod = 8;
                                }
                            }
                            else
                            {
                                m_lastUsedMethod = 9;
                            }
                        }
                        else
                        {
                            ///this is another degenerate case, where the initial GJK calculation reports a degenerate case
                            ///EPA reports no penetration, and the second GJK (using the supporting vector without margin)
                            ///reports a valid positive distance. Use the results of the second GJK instead of failing.
                            ///thanks to Jacob.Langford for the reproduction case
                            ///http://code.google.com/p/bullet/issues/detail?id=250


                            if (m_cachedSeparatingAxis.Length2 > 0f)
                            {
                                float distance2 = (tmpPointOnA - tmpPointOnB).Length - margin;
                                //only replace valid distances when the distance is less
                                if (!isValid || (distance2 < distance))
                                {
                                    distance = distance2;
                                    pointOnA = tmpPointOnA;
                                    pointOnB = tmpPointOnB;
                                    pointOnA -= m_cachedSeparatingAxis * marginA;
                                    pointOnB += m_cachedSeparatingAxis * marginB;
                                    normalInB = m_cachedSeparatingAxis;
                                    normalInB.normalize();
                                    isValid = true;
                                    m_lastUsedMethod = 6;
                                }
                                else
                                {
                                    m_lastUsedMethod = 5;
                                }
                            }
                        }

                    }

                }
            }



            if (isValid && ((distance < 0) || (distance * distance < input.m_maximumDistanceSquared)))
            {

                m_cachedSeparatingAxis = normalInB;
                m_cachedSeparatingDistance = distance;

                output.addContactPoint(
                    normalInB,
                    pointOnB + positionOffset,
                    distance, ref originalManifoldResult);

            }


        }

        public void getClosestPoints(ref ClosestPointInput input, ref ManifoldResult output, IDebugDraw debugDraw)
        {
            m_cachedSeparatingDistance = 0f;

            float distance = 0f;
            btVector3 normalInB = new btVector3(0f, 0f, 0f);
            btVector3 pointOnA, pointOnB = btVector3.Zero;
            btTransform localTransA = input.m_transformA;
            btTransform localTransB = input.m_transformB;
            btVector3 positionOffset = (localTransA.Origin + localTransB.Origin) * 0.5f;
            localTransA.Origin -= positionOffset;
            localTransB.Origin -= positionOffset;

            bool check2d = m_minkowskiA.isConvex2d && m_minkowskiB.isConvex2d;

            float marginA = m_marginA;
            float marginB = m_marginB;

            //gNumGjkChecks++;
            //for CCD we don't use margins
            if (m_ignoreMargin)
            {
                marginA = 0f;
                marginB = 0f;

            }

            m_curIter = 0;
            int gGjkMaxIter = 1000;//this is to catch invalid input, perhaps check for #NaN?
            m_cachedSeparatingAxis.setValue(0, 1, 0);

            bool isValid = false;
            bool checkSimplex = false;
            bool checkPenetration = true;
            m_degenerateSimplex = 0;

            m_lastUsedMethod = -1;

            {
                float squaredDistance = BulletGlobal.BT_LARGE_FLOAT;
                float delta = 0f;

                float margin = marginA + marginB;



                m_simplexSolver.reset();

                for (; ; )
                //while (true)
                {

                    btVector3 seperatingAxisInA;// = (-m_cachedSeparatingAxis) * input.m_transformA.Basis;
                    {
                        btVector3 temp = -m_cachedSeparatingAxis;
                        btMatrix3x3.Multiply(ref temp, ref input.m_transformA.Basis, out seperatingAxisInA);
                    }
                    btVector3 seperatingAxisInB;// = m_cachedSeparatingAxis * input.m_transformB.Basis;
                    btMatrix3x3.Multiply(ref m_cachedSeparatingAxis, ref input.m_transformB.Basis, out seperatingAxisInB);

                    btVector3 pInA;// = m_minkowskiA.localGetSupportVertexWithoutMarginNonVirtual(seperatingAxisInA);
                    m_minkowskiA.localGetSupportVertexWithoutMarginNonVirtual(ref seperatingAxisInA, out pInA);
                    btVector3 qInB;// = m_minkowskiB.localGetSupportVertexWithoutMarginNonVirtual(seperatingAxisInB);
                    m_minkowskiB.localGetSupportVertexWithoutMarginNonVirtual(ref seperatingAxisInB, out qInB);

                    btVector3 pWorld = btVector3.Transform(pInA, localTransA);
                    btVector3 qWorld = btVector3.Transform(qInB, localTransB);

                    if (check2d)
                    {
                        pWorld.Z = 0f;
                        qWorld.Z = 0f;
                    }

                    btVector3 w = pWorld - qWorld;
                    delta = m_cachedSeparatingAxis.dot(w);

                    // potential exit, they don't overlap
                    if ((delta > 0.0f) && (delta * delta > squaredDistance * input.m_maximumDistanceSquared))
                    {
                        m_degenerateSimplex = 10;
                        checkSimplex = true;
                        //checkPenetration = false;
                        break;
                    }

                    //exit 0: the new point is already in the simplex, or we didn't come any closer
                    if (m_simplexSolver.inSimplex(w))
                    {
                        m_degenerateSimplex = 1;
                        checkSimplex = true;
                        break;
                    }
                    // are we getting any closer ?
                    float f0 = squaredDistance - delta;
                    float f1 = squaredDistance * REL_ERROR2;

                    if (f0 <= f1)
                    {
                        if (f0 <= 0f)
                        {
                            m_degenerateSimplex = 2;
                        }
                        else
                        {
                            m_degenerateSimplex = 11;
                        }
                        checkSimplex = true;
                        break;
                    }
                    //add current vertex to simplex
                    m_simplexSolver.addVertex(w, pWorld, qWorld);

                    btVector3 newCachedSeparatingAxis;

                    //calculate the closest point to the origin (update vector v)
                    if (!m_simplexSolver.closest(out newCachedSeparatingAxis))
                    {
                        m_degenerateSimplex = 3;
                        checkSimplex = true;
                        break;
                    }

                    if (newCachedSeparatingAxis.Length2 < REL_ERROR2)
                    {
                        m_cachedSeparatingAxis = newCachedSeparatingAxis;
                        m_degenerateSimplex = 6;
                        checkSimplex = true;
                        break;
                    }

                    float previousSquaredDistance = squaredDistance;
                    squaredDistance = newCachedSeparatingAxis.Length2;

                    m_cachedSeparatingAxis = newCachedSeparatingAxis;

                    //redundant m_simplexSolver->compute_points(pointOnA, pointOnB);

                    //are we getting any closer ?
                    if (previousSquaredDistance - squaredDistance <= BulletGlobal.SIMD_EPSILON * previousSquaredDistance)
                    {
                        m_simplexSolver.backup_closest(ref m_cachedSeparatingAxis);
                        checkSimplex = true;
                        m_degenerateSimplex = 12;

                        break;
                    }

                    //degeneracy, this is typically due to invalid/uninitialized worldtransforms for a btCollisionObject   
                    if (m_curIter++ > gGjkMaxIter)
                    {
                        /*#if defined(DEBUG) || defined (_DEBUG) || defined (DEBUG_SPU_COLLISION_DETECTION)

                                printf("btGjkPairDetector maxIter exceeded:%i\n",m_curIter);   
                                printf("sepAxis=(%f,%f,%f), squaredDistance = %f, shapeTypeA=%i,shapeTypeB=%i\n",   
                                m_cachedSeparatingAxis.getX(),   
                                m_cachedSeparatingAxis.getY(),   
                                m_cachedSeparatingAxis.getZ(),   
                                squaredDistance,   
                                m_minkowskiA->getShapeType(),   
                                m_minkowskiB->getShapeType());   

                        #endif   */
                        break;

                    }


                    bool check = (!m_simplexSolver.fullSimplex);
                    //bool check = (!m_simplexSolver->fullSimplex() && squaredDistance > SIMD_EPSILON * m_simplexSolver->maxVertex());

                    if (!check)
                    {
                        //do we need this backup_closest here ?
                        m_simplexSolver.backup_closest(ref m_cachedSeparatingAxis);
                        m_degenerateSimplex = 13;
                        break;
                    }
                }

                if (checkSimplex)
                {
                    m_simplexSolver.compute_points(out pointOnA, out pointOnB);
                    normalInB = pointOnA - pointOnB;
                    float lenSqr = m_cachedSeparatingAxis.Length2;

                    //valid normal
                    if (lenSqr < 0.0001)
                    {
                        m_degenerateSimplex = 5;
                    }
                    if (lenSqr > BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON)
                    {
                        float rlen = 1f / (float)Math.Sqrt(lenSqr);
                        normalInB *= rlen; //normalize
                        float s = (float)Math.Sqrt(squaredDistance);

                        Debug.Assert(s > 0.0f);
                        pointOnA -= m_cachedSeparatingAxis * (marginA / s);
                        pointOnB += m_cachedSeparatingAxis * (marginB / s);
                        distance = ((1f / rlen) - margin);
                        isValid = true;

                        m_lastUsedMethod = 1;
                    }
                    else
                    {
                        m_lastUsedMethod = 2;
                    }
                }

                bool catchDegeneratePenetrationCase =
                    (m_catchDegeneracies != 0 && m_penetrationDepthSolver != null && m_degenerateSimplex != 0 && ((distance + margin) < 0.01));

                //if (checkPenetration && !isValid)
                if (checkPenetration && (!isValid || catchDegeneratePenetrationCase))
                {
                    //penetration case

                    //if there is no way to handle penetrations, bail out
                    if (m_penetrationDepthSolver != null)
                    {
                        // Penetration depth case.
                        btVector3 tmpPointOnA, tmpPointOnB;

                        //gNumDeepPenetrationChecks++;
                        m_cachedSeparatingAxis.setZero();

                        bool isValid2 = m_penetrationDepthSolver.calcPenDepth(
                            m_simplexSolver,
                            m_minkowskiA, m_minkowskiB,
                            localTransA, localTransB,
                            ref m_cachedSeparatingAxis, out tmpPointOnA, out tmpPointOnB,
                            debugDraw//, input.m_stackAlloc
                            );


                        if (isValid2)
                        {
                            btVector3 tmpNormalInB = tmpPointOnB - tmpPointOnA;
                            float lenSqr = tmpNormalInB.Length2;
                            if (lenSqr <= (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
                            {
                                tmpNormalInB = m_cachedSeparatingAxis;
                                lenSqr = m_cachedSeparatingAxis.Length2;
                            }

                            if (lenSqr > (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
                            {
                                tmpNormalInB /= (float)Math.Sqrt(lenSqr);
                                float distance2 = -(tmpPointOnA - tmpPointOnB).Length;
                                //only replace valid penetrations when the result is deeper (check)
                                if (!isValid || (distance2 < distance))
                                {
                                    distance = distance2;
                                    pointOnA = tmpPointOnA;
                                    pointOnB = tmpPointOnB;
                                    normalInB = tmpNormalInB;
                                    isValid = true;
                                    m_lastUsedMethod = 3;
                                }
                                else
                                {
                                    m_lastUsedMethod = 8;
                                }
                            }
                            else
                            {
                                m_lastUsedMethod = 9;
                            }
                        }
                        else
                        {
                            ///this is another degenerate case, where the initial GJK calculation reports a degenerate case
                            ///EPA reports no penetration, and the second GJK (using the supporting vector without margin)
                            ///reports a valid positive distance. Use the results of the second GJK instead of failing.
                            ///thanks to Jacob.Langford for the reproduction case
                            ///http://code.google.com/p/bullet/issues/detail?id=250


                            if (m_cachedSeparatingAxis.Length2 > 0f)
                            {
                                float distance2 = (tmpPointOnA - tmpPointOnB).Length - margin;
                                //only replace valid distances when the distance is less
                                if (!isValid || (distance2 < distance))
                                {
                                    distance = distance2;
                                    pointOnA = tmpPointOnA;
                                    pointOnB = tmpPointOnB;
                                    pointOnA -= m_cachedSeparatingAxis * marginA;
                                    pointOnB += m_cachedSeparatingAxis * marginB;
                                    normalInB = m_cachedSeparatingAxis;
                                    normalInB.normalize();
                                    isValid = true;
                                    m_lastUsedMethod = 6;
                                }
                                else
                                {
                                    m_lastUsedMethod = 5;
                                }
                            }
                        }

                    }

                }
            }



            if (isValid && ((distance < 0) || (distance * distance < input.m_maximumDistanceSquared)))
            {

                m_cachedSeparatingAxis = normalInB;
                m_cachedSeparatingDistance = distance;
                btVector3 temp = pointOnB + positionOffset;
                output.addContactPoint(
                    ref normalInB,
                    ref temp,
                    distance);

            }
        }
        
    }
}
