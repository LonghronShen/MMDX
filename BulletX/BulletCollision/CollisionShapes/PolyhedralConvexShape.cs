using System;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.CollisionShapes
{
    //The btPolyhedralConvexShape is an internal interface class for polyhedral convex shapes.
    public abstract class PolyhedralConvexShape : ConvexInternalShape
    {
        public PolyhedralConvexShape() : base() { }

        //brute force implementations
        public override void localGetSupportingVertex(ref btVector3 vec0, out btVector3 supVec)
        {
            supVec = btVector3.Zero;
            int i;
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

            for (i = 0; i < NumVertices; i++)
            {
                getVertex(i, out vtx);
                newDot = vec.dot(vtx);
                if (newDot > maxDot)
                {
                    maxDot = newDot;
                    supVec = vtx;
                }
            }


            //return supVec;
        }

        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(btVector3[] vectors, btVector3[] supportVerticesOut, int numVectors)
        {
            int i;

            btVector3 vtx;
            float newDot;

            for (i = 0; i < numVectors; i++)
            {
                supportVerticesOut[i].W = -BulletGlobal.BT_LARGE_FLOAT;
            }

            for (int j = 0; j < numVectors; j++)
            {

                btVector3 vec = vectors[j];

                for (i = 0; i < NumVertices; i++)
                {
                    getVertex(i, out vtx);
                    newDot = vec.dot(vtx);
                    if (newDot > supportVerticesOut[j].W)
                    {
                        //WARNING: don't swap next lines, the w component would get overwritten!
                        supportVerticesOut[j] = vtx;
                        supportVerticesOut[j].W = newDot;
                    }
                }
            }
        }
        public override void calculateLocalInertia(float mass, out btVector3 inertia)
        {
            //not yet, return box inertia

            float margin = Margin;

            btTransform ident;
            ident = btTransform.Identity;
            btVector3 aabbMin, aabbMax;
            getAabb(ident, out aabbMin, out aabbMax);
            btVector3 halfExtents = (aabbMax - aabbMin) * 0.5f;

            float lx = 2f * (halfExtents.X + margin);
            float ly = 2f * (halfExtents.Y + margin);
            float lz = 2f * (halfExtents.Z + margin);
            float x2 = lx * lx;
            float y2 = ly * ly;
            float z2 = lz * lz;
            float scaledmass = mass * 0.08333333f;

            inertia = scaledmass * (new btVector3(y2 + z2, x2 + z2, x2 + y2));
        }

        public abstract int NumVertices { get; }
        public abstract int NumEdges { get; }

        public abstract void getEdge(int i, out btVector3 pa, out btVector3 pb);
        public abstract void getVertex(int i, out btVector3 vtx);
        public abstract int NumPlanes { get; }
        public abstract void getPlane(out btVector3 planeNormal, out btVector3 planeSupport, int i);

        public abstract bool isInside(btVector3 pt, float tolerance);
    }
}