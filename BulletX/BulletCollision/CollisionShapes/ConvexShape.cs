using System;
using System.Diagnostics;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    delegate void LsDelegate(ref btVector3 vec,out btVector3 result);

    public abstract class ConvexShape : CollisionShape
    {
        //MinkowskiDiffからのデータ
        internal LsDelegate Ls;
        internal LsDelegate localGetSupportVertexNonVirtualDelegate;
        internal LsDelegate localGetSupportVertexWithoutMarginNonVirtualDelegate;
        public ConvexShape() : base() 
        {
            localGetSupportVertexNonVirtualDelegate = localGetSupportVertexNonVirtual;
            localGetSupportVertexWithoutMarginNonVirtualDelegate = localGetSupportVertexWithoutMarginNonVirtual;
        }


        public abstract void localGetSupportingVertex(ref btVector3 vec,out btVector3 result);

        public abstract void localGetSupportingVertexWithoutMargin(ref btVector3 vec,out btVector3 result);

        public void localGetSupportVertexWithoutMarginNonVirtual(ref btVector3 localDir,out btVector3 result)
        {
            switch (ShapeType)
            {
                case BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE:
                    {
                        result= btVector3.Zero;
                        return;
                    }
                case BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE:
                    {
                        BoxShape convexShape = (BoxShape)this;
                        btVector3 halfExtents = convexShape.ImplicitShapeDimensions;
                        //btFsel(btScalar a, btScalar b, btScalar c) return a >= 0 ? b : c;

                        result= new btVector3(localDir.X >= 0 ? halfExtents.X : -halfExtents.X,
                            localDir.Y >= 0 ? halfExtents.Y : -halfExtents.Y,
                            localDir.Z >= 0 ? halfExtents.Z : -halfExtents.Z);
                        return;
                    }
                case BroadphaseNativeTypes.TRIANGLE_SHAPE_PROXYTYPE:
                    {
                        throw new NotImplementedException("Traiangle Shape is not Implemented");
#if false
		                TriangleShape triangleShape = (TriangleShape)this;
		                btVector3 dir(localDir.getX(),localDir.getY(),localDir.getZ());
		                btVector3* vertices = &triangleShape->m_vertices1[0];
		                btVector3 dots(dir.dot(vertices[0]), dir.dot(vertices[1]), dir.dot(vertices[2]));
		                btVector3 sup = vertices[dots.maxAxis()];
		                return btVector3(sup.getX(),sup.getY(),sup.getZ());
#endif
                    }
                case BroadphaseNativeTypes.CYLINDER_SHAPE_PROXYTYPE:
                    {
                        throw new NotImplementedException("Cylinder Shape is not Implemented");
#if false
		                btCylinderShape* cylShape = (btCylinderShape*)this;
		                //mapping of halfextents/dimension onto radius/height depends on how cylinder local orientation is (upAxis)

		                btVector3 halfExtents = cylShape->getImplicitShapeDimensions();
		                btVector3 v(localDir.getX(),localDir.getY(),localDir.getZ());
		                int cylinderUpAxis = cylShape->getUpAxis();
		                int XX(1),YY(0),ZZ(2);

		                switch (cylinderUpAxis)
		                {
		                case 0:
		                {
			                XX = 1;
			                YY = 0;
			                ZZ = 2;
		                }
		                break;
		                case 1:
		                {
			                XX = 0;
			                YY = 1;
			                ZZ = 2;	
		                }
		                break;
		                case 2:
		                {
			                XX = 0;
			                YY = 2;
			                ZZ = 1;
                			
		                }
		                break;
		                default:
			                btAssert(0);
		                break;
		                };

		                btScalar radius = halfExtents[XX];
		                btScalar halfHeight = halfExtents[cylinderUpAxis];

		                btVector3 tmp;
		                btScalar d ;

		                btScalar s = btSqrt(v[XX] * v[XX] + v[ZZ] * v[ZZ]);
		                if (s != btScalar(0.0))
		                {
			                d = radius / s;  
			                tmp[XX] = v[XX] * d;
			                tmp[YY] = v[YY] < 0.0 ? -halfHeight : halfHeight;
			                tmp[ZZ] = v[ZZ] * d;
			                return btVector3(tmp.getX(),tmp.getY(),tmp.getZ());
		                } else {
			                tmp[XX] = radius;
			                tmp[YY] = v[YY] < 0.0 ? -halfHeight : halfHeight;
			                tmp[ZZ] = btScalar(0.0);
			                return btVector3(tmp.getX(),tmp.getY(),tmp.getZ());
		                }
#endif
                    }
                case BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE:
                    {
                        btVector3 vec0 = localDir;

                        CapsuleShape capsuleShape = (CapsuleShape)this;
                        float halfHeight = capsuleShape.HalfHeight;
                        int capsuleUpAxis = capsuleShape.UpAxis;

                        float radius = capsuleShape.Radius;
                        btVector3 supVec = btVector3.Zero;//(0,0,0);

                        float maxDot = -BulletGlobal.BT_LARGE_FLOAT;

                        btVector3 vec = vec0;
                        float lenSqr = vec.Length2;
                        if (lenSqr < 0.0001f)
                        {
                            vec.setValue(1, 0, 0);
                        }
                        else
                        {
                            float rlen = 1f / (float)Math.Sqrt(lenSqr);
                            vec *= rlen;
                        }
                        btVector3 vtx;
                        float newDot;
                        {
                            btVector3 pos = btVector3.Zero;//(0,0,0);
                            pos[capsuleUpAxis] = halfHeight;

                            //vtx = pos +vec*(radius);
                            #region vtx = pos + vec * capsuleShape.LocalScalingNV * (radius) - vec * capsuleShape.MarginNV;
                            {
                                btVector3 temp1, temp2, temp3, temp4, temp5 = capsuleShape.LocalScalingNV;
                                btVector3.Multiply(ref vec, ref temp5, out temp1);
                                btVector3.Multiply(ref temp1, radius, out temp2);
                                btVector3.Add(ref pos, ref temp2, out temp3);
                                btVector3.Multiply(ref vec, capsuleShape.MarginNV, out temp4);
                                btVector3.Subtract(ref temp3, ref temp4, out vtx);
                            }
                            #endregion
                            newDot = vec.dot(ref vtx);


                            if (newDot > maxDot)
                            {
                                maxDot = newDot;
                                supVec = vtx;
                            }
                        }
                        {
                            btVector3 pos = btVector3.Zero;//(0,0,0);
                            pos[capsuleUpAxis] = -halfHeight;

                            //vtx = pos +vec*(radius);
                            #region vtx = pos + vec * capsuleShape.LocalScalingNV * (radius) - vec * capsuleShape.MarginNV;
                            {
                                btVector3 temp1, temp2, temp3, temp4, temp5 = capsuleShape.LocalScalingNV;
                                btVector3.Multiply(ref vec, ref temp5, out temp1);
                                btVector3.Multiply(ref temp1, radius, out temp2);
                                btVector3.Add(ref pos, ref temp2, out temp3);
                                btVector3.Multiply(ref vec, capsuleShape.MarginNV, out temp4);
                                btVector3.Subtract(ref temp3, ref temp4, out vtx);
                            }
                            #endregion
                            newDot = vec.dot(ref vtx);
                            if (newDot > maxDot)
                            {
                                maxDot = newDot;
                                supVec = vtx;
                            }
                        }
                        result= new btVector3(supVec.X, supVec.Y, supVec.Z);
                        return;
                    }
                case BroadphaseNativeTypes.CONVEX_POINT_CLOUD_SHAPE_PROXYTYPE:
                    {
                        throw new NotImplementedException("Convex Point Cloud Shape is not implemented");
#if false
		                ConvexPointCloudShape* convexPointCloudShape = (ConvexPointCloudShape*)this;
		                btVector3* points = convexPointCloudShape->getUnscaledPoints ();
		                int numPoints = convexPointCloudShape->getNumPoints ();
		                return convexHullSupport (localDir, points, numPoints,convexPointCloudShape->getLocalScalingNV());
#endif
                    }
                case BroadphaseNativeTypes.CONVEX_HULL_SHAPE_PROXYTYPE:
                    {
                        throw new NotImplementedException("Convex Hull Shape is not implemented");
#if false
		                ConvexHullShape convexHullShape = (ConvexHullShape)this;
		                btVector3* points = convexHullShape->getUnscaledPoints();
		                int numPoints = convexHullShape->getNumPoints ();
		                return convexHullSupport (localDir, points, numPoints,convexHullShape->getLocalScalingNV());
#endif
                    }
                default:
                    this.localGetSupportingVertexWithoutMargin(ref localDir,out result);
                    break;
            }

            // should never reach here
            //Debug.Assert(false);
            //return btVector3.Zero;// (btScalar(0.0f), btScalar(0.0f), btScalar(0.0f));
        }
        public void localGetSupportVertexNonVirtual(ref btVector3 localDir,out btVector3 result)
        {
            btVector3 localDirNorm = localDir;
            if (localDirNorm.Length2 < (BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON))
            {
                localDirNorm.setValue(-1f, -1f, -1f);
            }
            localDirNorm.normalize();

            //return localGetSupportVertexWithoutMarginNonVirtual(localDirNorm) + MarginNonVirtual * localDirNorm;
            btVector3 temp1, temp2;
            localGetSupportVertexWithoutMarginNonVirtual(ref localDirNorm, out temp1);
            btVector3.Multiply(ref localDirNorm, MarginNonVirtual, out temp2);
            btVector3.Add(ref temp1, ref temp2, out result);
        }
        public float MarginNonVirtual
        {
            get
            {
                switch (ShapeType)
                {
                    case BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE:
                        {
                            SphereShape sphereShape = (SphereShape)this;
                            return sphereShape.Radius;
                        }
                    case BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE:
                        {
                            BoxShape convexShape = (BoxShape)this;
                            return convexShape.MarginNV;
                        }
                    case BroadphaseNativeTypes.TRIANGLE_SHAPE_PROXYTYPE:
                        {
                            throw new NotImplementedException("traiangle shape is not implemented");
                            //btTriangleShape* triangleShape = (btTriangleShape*)this;
                            //return triangleShape->getMarginNV();
                        }
                    case BroadphaseNativeTypes.CYLINDER_SHAPE_PROXYTYPE:
                        {
                            throw new NotImplementedException("sylinder shape is not implemented");
                            //btCylinderShape* cylShape = (btCylinderShape*)this;
                            //return cylShape->getMarginNV();
                        }
                    case BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE:
                        {
                            CapsuleShape capsuleShape = (CapsuleShape)this;
                            return capsuleShape.MarginNV;
                        }
                    case BroadphaseNativeTypes.CONVEX_POINT_CLOUD_SHAPE_PROXYTYPE:
                    /* fall through */
                    case BroadphaseNativeTypes.CONVEX_HULL_SHAPE_PROXYTYPE:
                        {
                            throw new NotImplementedException("convex point and convex hull shape is not implemented");
                            //btPolyhedralConvexShape* convexHullShape = (btPolyhedralConvexShape*)this;
                            //return convexHullShape->getMarginNV();
                        }
                    default:
                        return this.Margin;
                }
            }
        }

        public void getAabbNonVirtual(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            switch (ShapeType)
            {
                case BroadphaseNativeTypes.SPHERE_SHAPE_PROXYTYPE:
                    {
                        SphereShape sphereShape = (SphereShape)this;
                        float radius = sphereShape.ImplicitShapeDimensions.X;// * convexShape->getLocalScaling().getX();
                        float margin = radius + sphereShape.MarginNonVirtual;
                        btVector3 center = t.Origin;
                        btVector3 extent = new btVector3(margin, margin, margin);
                        //aabbMin = center - extent;
                        btVector3.Subtract(ref center, ref extent, out aabbMin);
                        //aabbMax = center + extent;
                        btVector3.Add(ref center, ref extent, out aabbMax);
                    }
                    break;
                case BroadphaseNativeTypes.CYLINDER_SHAPE_PROXYTYPE:
                /* fall through */
                case BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE:
                    {
                        BoxShape convexShape = (BoxShape)this;
                        float margin = convexShape.MarginNonVirtual;
                        btVector3 halfExtents = convexShape.ImplicitShapeDimensions;
                        #region halfExtents += new btVector3(margin, margin, margin);
                        {
                            btVector3 temp = new btVector3(margin, margin, margin);
                            halfExtents.Add(ref temp);
                        }
                        #endregion
                        btMatrix3x3 abs_b;// = t.Basis.absolute();
                        t.Basis.absolute(out abs_b);
                        btVector3 center = t.Origin;
                        btVector3 extent = new btVector3(abs_b.el0.dot(halfExtents), abs_b.el1.dot(halfExtents), abs_b.el2.dot(halfExtents));

                        //aabbMin = center - extent;
                        btVector3.Subtract(ref center, ref extent, out aabbMin);
                        //aabbMax = center + extent;
                        btVector3.Add(ref center, ref extent, out aabbMax);
                        break;
                    }
                case BroadphaseNativeTypes.TRIANGLE_SHAPE_PROXYTYPE:
                    throw new NotImplementedException();
#if false
	            {
		            btTriangleShape* triangleShape = (btTriangleShape*)this;
		            float margin = triangleShape->getMarginNonVirtual();
		            for (int i=0;i<3;i++)
		            {
			            btVector3 vec(btScalar(0.),btScalar(0.),btScalar(0.));
			            vec[i] = btScalar(1.);

			            btVector3 sv = localGetSupportVertexWithoutMarginNonVirtual(vec*t.getBasis());

			            btVector3 tmp = t(sv);
			            aabbMax[i] = tmp[i]+margin;
			            vec[i] = btScalar(-1.);
			            tmp = t(localGetSupportVertexWithoutMarginNonVirtual(vec*t.getBasis()));
			            aabbMin[i] = tmp[i]-margin;
		            }	
	            }
	            break;
#endif
                case BroadphaseNativeTypes.CAPSULE_SHAPE_PROXYTYPE:
                    {
                        CapsuleShape capsuleShape = (CapsuleShape)this;
                        btVector3 halfExtents = new btVector3(capsuleShape.Radius, capsuleShape.Radius, capsuleShape.Radius);
                        int m_upAxis = capsuleShape.UpAxis;
                        halfExtents[m_upAxis] = capsuleShape.Radius + capsuleShape.HalfHeight;
                        #region halfExtents += new btVector3(capsuleShape.MarginNonVirtual, capsuleShape.MarginNonVirtual, capsuleShape.MarginNonVirtual);
                        {
                            btVector3 temp = new btVector3(capsuleShape.MarginNonVirtual, capsuleShape.MarginNonVirtual, capsuleShape.MarginNonVirtual);
                            halfExtents.Add(ref temp);
                        }
                        #endregion
                        btMatrix3x3 abs_b;// = t.Basis.absolute();
                        t.Basis.absolute(out abs_b);
                        btVector3 center = t.Origin;
                        btVector3 extent = new btVector3(abs_b.el0.dot(halfExtents), abs_b.el1.dot(halfExtents), abs_b.el2.dot(halfExtents));
                        //aabbMin = center - extent;
                        btVector3.Subtract(ref center, ref extent, out aabbMin);
                        //aabbMax = center + extent;
                        btVector3.Add(ref center, ref extent, out aabbMax);
                    }
                    break;
                case BroadphaseNativeTypes.CONVEX_POINT_CLOUD_SHAPE_PROXYTYPE:
                case BroadphaseNativeTypes.CONVEX_HULL_SHAPE_PROXYTYPE:
                    throw new NotImplementedException();
#if false
	                {
                        btPolyhedralConvexAabbCachingShape* convexHullShape = (btPolyhedralConvexAabbCachingShape*)this;
		                btScalar margin = convexHullShape->getMarginNonVirtual();
		                convexHullShape->getNonvirtualAabb (t, aabbMin, aabbMax, margin);
	                }
	                break;
#endif
                default:
                    this.getAabb(t, out aabbMin, out aabbMax);
                    Debug.Assert(false);
                    break;
            }
            // should never reach here
            Debug.Assert(false);
        }

        //notice that the vectors should be unit length
        public abstract void batchedUnitVectorGetSupportingVertexWithoutMargin(btVector3[] vectors, btVector3[] supportVerticesOut, int numVectors);

        ///getAabb's default implementation is brute force, expected derived classes to implement a fast dedicated version
        //public abstract void getAabb(btTransform t,out btVector3 aabbMin,out btVector3 aabbMax);
        public abstract void getAabbSlow(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax);

        //public abstract btVector3 LocalScaling { get; set; }
        //public abstract float Margin { get; set; }
        public abstract int NumPreferredPenetrationDirections { get; }

        public abstract void getPreferredPenetrationDirection(int index, out btVector3 penetrationVector);

    }
}

