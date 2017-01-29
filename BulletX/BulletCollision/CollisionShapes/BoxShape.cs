using System;
using BulletX.BulletCollision.BroadphaseCollision;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    /// <summary>
    /// 箱型衝突スキン
    /// </summary>
    public class BoxShape : PolyhedralConvexShape
    {
        public override BroadphaseNativeTypes ShapeType
        {
            get { return BroadphaseNativeTypes.BOX_SHAPE_PROXYTYPE; }
        }
        public btVector3 HalfExtentsWithMargin
        {
            get
            {
                btVector3 halfExtents = HalfExtentsWithoutMargin;
                btVector3 margin = new btVector3(Margin, Margin, Margin);
                //halfExtents += margin;
                halfExtents.Add(ref margin);
                return halfExtents;
            }
        }
        public btVector3 HalfExtentsWithoutMargin { get { return m_implicitShapeDimensions;/*scaling is included, margin is not*/	} }

        public override void localGetSupportingVertex(ref btVector3 vec,out btVector3 result)
        {
            btVector3 halfExtents = HalfExtentsWithoutMargin;
            btVector3 margin = new btVector3(Margin, Margin, Margin);
            //halfExtents += margin;
            halfExtents.Add(ref margin);

            result= new btVector3((vec.X >= 0 ? halfExtents.X : -halfExtents.X),
                (vec.Y >= 0 ? halfExtents.Y : -halfExtents.Y),
                (vec.Z >= 0 ? halfExtents.Z : -halfExtents.Z));
        }
        public override void localGetSupportingVertexWithoutMargin(ref btVector3 vec,out btVector3 result)
        {
            btVector3 halfExtents = HalfExtentsWithoutMargin;

            result= new btVector3((vec.X >= 0 ? halfExtents.X : -halfExtents.X),
                (vec.Y >= 0 ? halfExtents.Y : -halfExtents.Y),
                (vec.Z >= 0 ? halfExtents.Z : -halfExtents.Z));
        }
        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(btVector3[] vectors, btVector3[] supportVerticesOut, int numVectors)
        {
            btVector3 halfExtents = HalfExtentsWithoutMargin;

            for (int i = 0; i < numVectors; i++)
            {
                btVector3 vec = vectors[i];
                supportVerticesOut[i].setValue((vec.X >= 0 ? halfExtents.X : -halfExtents.X),
                    (vec.Y >= 0 ? halfExtents.Y : -halfExtents.Y),
                    (vec.Z >= 0 ? halfExtents.Z : -halfExtents.Z));
            }
        }

        public BoxShape(btVector3 boxHalfExtents)
            : base()
        {
            btVector3 margin = new btVector3(Margin, Margin, Margin);
            m_implicitShapeDimensions = (boxHalfExtents * m_localScaling) - margin;
        }

        public override float Margin
        {
            set
            {
                //correct the m_implicitShapeDimensions for the margin
                btVector3 oldMargin = new btVector3(Margin, Margin, Margin);
                btVector3 implicitShapeDimensionsWithMargin;// = m_implicitShapeDimensions + oldMargin;
                btVector3.Add(ref m_implicitShapeDimensions, ref oldMargin, out implicitShapeDimensionsWithMargin);

                base.Margin = value;
                btVector3 newMargin = new btVector3(Margin, Margin, Margin);
                //m_implicitShapeDimensions = implicitShapeDimensionsWithMargin - newMargin;
                btVector3.Subtract(ref implicitShapeDimensionsWithMargin, ref newMargin, out m_implicitShapeDimensions);
            }
        }

        public override btVector3 LocalScaling
        {
            set
            {
                btVector3 oldMargin=new btVector3(Margin,Margin,Margin);
		        btVector3 implicitShapeDimensionsWithMargin;// = m_implicitShapeDimensions+oldMargin;
                btVector3.Add(ref m_implicitShapeDimensions, ref oldMargin, out implicitShapeDimensionsWithMargin);

		        btVector3 unScaledImplicitShapeDimensionsWithMargin = implicitShapeDimensionsWithMargin / m_localScaling;

		        base.LocalScaling = value;

		        m_implicitShapeDimensions = (unScaledImplicitShapeDimensionsWithMargin * m_localScaling) - oldMargin;
            }
        }
        public override void getAabb(btTransform t, out btVector3 aabbMin, out btVector3 aabbMax)
        {
            BulletGlobal.btTransformAabb(HalfExtentsWithoutMargin, Margin, t, out aabbMin, out aabbMax);
        }
        public override void calculateLocalInertia(float mass, out btVector3 inertia)
        {
            //btScalar margin = btScalar(0.);
            btVector3 halfExtents = HalfExtentsWithMargin;

            float lx = 2f * (halfExtents.X);
            float ly = 2f * (halfExtents.Y);
            float lz = 2f * (halfExtents.Z);

            inertia = new btVector3(mass / 12.0f * (ly * ly + lz * lz),
                            mass / 12.0f * (lx * lx + lz * lz),
                            mass / 12.0f * (lx * lx + ly * ly));
        }



        public override void getPlane(out btVector3 planeNormal, out btVector3 planeSupport, int i)
        {
            //this plane might not be aligned...
            btVector4 plane;
            getPlaneEquation(out plane, i);
            planeNormal = new btVector3(plane.X, plane.Y, plane.Z);
            btVector3 temp;
            btVector3.Minus(ref planeNormal, out temp);
            //planeSupport = localGetSupportingVertex(-planeNormal);
            localGetSupportingVertex(ref temp, out planeSupport);
        }


        public override int NumPlanes { get { return 6; } }

        public override int NumVertices { get { return 8; } }

        public override int NumEdges { get { return 12; } }


        public override void getVertex(int i, out btVector3 vtx)
        {
            btVector3 halfExtents = HalfExtentsWithoutMargin;

            vtx = new btVector3(
                    halfExtents.X * (1 - (i & 1)) - halfExtents.X * (i & 1),
                    halfExtents.Y * (1 - ((i & 2) >> 1)) - halfExtents.Y * ((i & 2) >> 1),
                    halfExtents.Z * (1 - ((i & 4) >> 2)) - halfExtents.Z * ((i & 4) >> 2));
        }


        public virtual void getPlaneEquation(out btVector4 plane, int i)
        {
            btVector3 halfExtents = HalfExtentsWithoutMargin;

            switch (i)
            {
                case 0:
                    plane = new btVector4(1f, 0f, 0f, -halfExtents.X);
                    break;
                case 1:
                    plane = new btVector4(-1f, 0f, 0f, -halfExtents.X);
                    break;
                case 2:
                    plane = new btVector4(0f, 1f, 0f, -halfExtents.Y);
                    break;
                case 3:
                    plane = new btVector4(0f, -1f, 0f, -halfExtents.Y);
                    break;
                case 4:
                    plane = new btVector4(0f, 0f, 1f, -halfExtents.Z);
                    break;
                case 5:
                    plane = new btVector4(0f, 0f, -1f, -halfExtents.Z);
                    break;
                default:
                    throw new Exception();
            }
        }


        public override void getEdge(int i, out btVector3 pa, out btVector3 pb)
        //virtual void getEdge(int i,Edge& edge) const
        {
            int edgeVert0 = 0;
            int edgeVert1 = 0;

            switch (i)
            {
                case 0:
                    edgeVert0 = 0;
                    edgeVert1 = 1;
                    break;
                case 1:
                    edgeVert0 = 0;
                    edgeVert1 = 2;
                    break;
                case 2:
                    edgeVert0 = 1;
                    edgeVert1 = 3;

                    break;
                case 3:
                    edgeVert0 = 2;
                    edgeVert1 = 3;
                    break;
                case 4:
                    edgeVert0 = 0;
                    edgeVert1 = 4;
                    break;
                case 5:
                    edgeVert0 = 1;
                    edgeVert1 = 5;

                    break;
                case 6:
                    edgeVert0 = 2;
                    edgeVert1 = 6;
                    break;
                case 7:
                    edgeVert0 = 3;
                    edgeVert1 = 7;
                    break;
                case 8:
                    edgeVert0 = 4;
                    edgeVert1 = 5;
                    break;
                case 9:
                    edgeVert0 = 4;
                    edgeVert1 = 6;
                    break;
                case 10:
                    edgeVert0 = 5;
                    edgeVert1 = 7;
                    break;
                case 11:
                    edgeVert0 = 6;
                    edgeVert1 = 7;
                    break;
                default:
                    throw new Exception();
            }

            getVertex(edgeVert0, out pa);
            getVertex(edgeVert1, out pb);
        }





        public override bool isInside(btVector3 pt, float tolerance)
        {
            btVector3 halfExtents = HalfExtentsWithoutMargin;

            //btScalar minDist = 2*tolerance;

            bool result = (pt.X <= (halfExtents.X + tolerance)) &&
                            (pt.X >= (-halfExtents.X - tolerance)) &&
                            (pt.Y <= (halfExtents.Y + tolerance)) &&
                            (pt.Y >= (-halfExtents.Y - tolerance)) &&
                            (pt.Z <= (halfExtents.Z + tolerance)) &&
                            (pt.Z >= (-halfExtents.Z - tolerance));

            return result;
        }

        //debugging
        public override string Name { get { return "Box"; } }

        public override int NumPreferredPenetrationDirections { get { return 6; } }

        public override void getPreferredPenetrationDirection(int index, out btVector3 penetrationVector)
        {
            switch (index)
            {
                case 0:
                    penetrationVector = new btVector3(1f, 0f, 0f);
                    break;
                case 1:
                    penetrationVector = new btVector3(-1f, 0f, 0f);
                    break;
                case 2:
                    penetrationVector = new btVector3(0f, 1f, 0f);
                    break;
                case 3:
                    penetrationVector = new btVector3(0f, -1f, 0f);
                    break;
                case 4:
                    penetrationVector = new btVector3(0f, 0f, 1f);
                    break;
                case 5:
                    penetrationVector = new btVector3(0f, 0f, -1f);
                    break;
                default:
                    throw new Exception();
            }
        }
    }
}
