using System;

namespace BulletX.LinerMath
{
    [Flags]
    public enum DebugDrawModes
    {
        DBG_NoDebug = 0,
        DBG_DrawWireframe = 1,
        DBG_DrawAabb = 2,
        DBG_DrawFeaturesText = 4,
        DBG_DrawContactPoints = 8,
        DBG_NoDeactivation = 16,
        DBG_NoHelpText = 32,
        DBG_DrawText = 64,
        DBG_ProfileTimings = 128,
        DBG_EnableSatComparison = 256,
        DBG_DisableBulletLCP = 512,
        DBG_EnableCCD = 1024,
        DBG_DrawConstraints = (1 << 11),
        DBG_DrawConstraintLimits = (1 << 12),
        DBG_FastWireframe = (1 << 13),
        DBG_MAX_DEBUG_DRAW_MODE
    };
    public abstract class IDebugDraw
    {
        public IDebugDraw() { }
        public abstract void drawLine(ref btVector3 from,ref btVector3 to,ref btVector3 color);
        public virtual void drawLine(ref btVector3 from,ref  btVector3 to,ref btVector3 fromColor,ref btVector3 toColor)
        {
            drawLine(ref from, ref to, ref fromColor);
        }

        public void drawSphere(float radius,ref btTransform transform,ref btVector3 color)
        {
            btVector3 start = transform.Origin;

            btVector3 xoffs;// = btMatrix3x3.Multiply(transform.Basis, new btVector3(radius, 0, 0));
            btVector3 yoffs;// = btMatrix3x3.Multiply(transform.Basis, new btVector3(0, radius, 0));
            btVector3 zoffs;// = btMatrix3x3.Multiply(transform.Basis, new btVector3(0, 0, radius));
            {
                btVector3 temp;
                temp = new btVector3(radius, 0, 0);
                btMatrix3x3.Multiply(ref transform.Basis, ref temp, out xoffs);
                temp = new btVector3(0, radius, 0);
                btMatrix3x3.Multiply(ref transform.Basis, ref temp, out yoffs);
                temp = new btVector3(0, 0, radius);
                btMatrix3x3.Multiply(ref transform.Basis, ref temp, out zoffs);
            }
            // XY 
            /*drawLine(start - xoffs, start + yoffs, color);
            drawLine(start + yoffs, start + xoffs, color);
            drawLine(start + xoffs, start - yoffs, color);
            drawLine(start - yoffs, start - xoffs, color);
            

            // XZ
            drawLine(start - xoffs, start + zoffs, color);
            drawLine(start + zoffs, start + xoffs, color);
            drawLine(start + xoffs, start - zoffs, color);
            drawLine(start - zoffs, start - xoffs, color);

            // YZ
            drawLine(start - yoffs, start + zoffs, color);
            drawLine(start + zoffs, start + yoffs, color);
            drawLine(start + yoffs, start - zoffs, color);
            drawLine(start - zoffs, start - yoffs, color);*/
            btVector3 sax, say, saz, smx, smy, smz;
            btVector3.Add(ref start, ref xoffs, out sax);
            btVector3.Add(ref start, ref yoffs, out say);
            btVector3.Add(ref start, ref zoffs, out saz);
            btVector3.Subtract(ref start, ref xoffs, out smx);
            btVector3.Subtract(ref start, ref yoffs, out smy);
            btVector3.Subtract(ref start, ref zoffs, out smz);
            // XY 
            drawLine(ref smx, ref say, ref color);
            drawLine(ref say, ref sax, ref color);
            drawLine(ref sax, ref smy, ref color);
            drawLine(ref smy, ref smx, ref color);


            // XZ
            drawLine(ref smx, ref saz, ref color);
            drawLine(ref saz, ref sax, ref color);
            drawLine(ref sax, ref smz, ref color);
            drawLine(ref smz, ref smx, ref color);

            // YZ
            drawLine(ref smy, ref saz, ref color);
            drawLine(ref saz, ref say, ref color);
            drawLine(ref say, ref smz, ref color);
            drawLine(ref smz, ref smy, ref color);
        }

        public virtual void drawSphere(ref btVector3 p, float radius,ref btVector3 color)
        {
            btTransform tr = btTransform.Identity;
            tr.Origin = p;
            drawSphere(radius, ref tr,ref color);
        }

        public virtual void drawTriangle(ref btVector3 v0,ref btVector3 v1,ref    btVector3 v2,ref btVector3 n0,ref btVector3 n1,ref btVector3 n2,ref btVector3 color, float alpha)
        {
            drawTriangle(ref v0,ref v1,ref v2,ref color,alpha);
        }
        public virtual void drawTriangle(ref btVector3 v0,ref btVector3 v1,ref btVector3 v2,ref btVector3 color, float alpha)
        {
            drawLine(ref v0, ref v1, ref color);
            drawLine(ref v1, ref v2, ref color);
            drawLine(ref v2, ref v0, ref color);
        }

        public abstract void	drawContactPoint(ref btVector3 PointOnB,ref btVector3 normalOnB,float distance,int lifeTime,ref btVector3 color);

	    public abstract void reportErrorWarning(string warningString);
        
	    public abstract void	draw3dText(ref btVector3 location,string textString);

        public abstract DebugDrawModes DebugMode { get; set; }

        public virtual void drawAabb(ref btVector3 from,ref btVector3 to,ref btVector3 color)
        {

            btVector3 halfExtents = (to - from) * 0.5f;
            btVector3 center;// = (to + from) * 0.5f;
            {
                btVector3 temp;
                btVector3.Add(ref to, ref from, out temp);
                btVector3.Multiply(ref temp, 0.5f, out center);
            }
            int i, j;

            btVector3 edgecoord = new btVector3(1f, 1f, 1f), pa, pb;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    pa = new btVector3(edgecoord.X * halfExtents.X, edgecoord.Y * halfExtents.Y,
                        edgecoord.Z * halfExtents.Z);
                    //pa += center;
                    pa.Add(ref center);

                    int othercoord = j % 3;
                    edgecoord[othercoord] *= -1f;
                    pb = new btVector3(edgecoord.X * halfExtents.X, edgecoord.Y * halfExtents.Y,
                        edgecoord.Z * halfExtents.Z);
                    //pb += center;
                    pb.Add(ref center);

                    drawLine(ref pa,ref pb,ref color);
                }
                edgecoord = new btVector3(-1f, -1f, -1f);
                if (i < 3)
                    edgecoord[i] *= -1f;
            }
        }

        public virtual void drawTransform(ref btTransform transform, float orthoLen)
        {
            btVector3 start = transform.Origin;
            //drawLine(start, start + btMatrix3x3.Multiply(transform.Basis, new btVector3(orthoLen, 0, 0)), new btVector3(0.7f, 0, 0));
            //drawLine(start, start + btMatrix3x3.Multiply(transform.Basis, new btVector3(0, orthoLen, 0)), new btVector3(0, 0.7f, 0));
            //drawLine(start, start + btMatrix3x3.Multiply(transform.Basis, new btVector3(0, 0, orthoLen)), new btVector3(0, 0, 0.7f));
            {
                btVector3 temp, temp2, temp3, temp4;
                temp = new btVector3(orthoLen, 0, 0);
                btMatrix3x3.Multiply(ref transform.Basis, ref temp, out temp2);
                btVector3.Add(ref start, ref temp2, out temp3);
                temp4 = new btVector3(0.7f, 0, 0);
                drawLine(ref start, ref temp3, ref temp4);
                temp = new btVector3(0, orthoLen, 0);
                btMatrix3x3.Multiply(ref transform.Basis, ref temp, out temp2);
                btVector3.Add(ref start, ref temp2, out temp3);
                temp4 = new btVector3(0, 0.7f, 0);
                drawLine(ref start, ref temp3, ref temp4);
                temp = new btVector3(0, 0, orthoLen);
                btVector3.Add(ref start, ref temp2, out temp3);
                temp4 = new btVector3(0, 0, 0.7f);
                drawLine(ref start, ref  temp3, ref temp4);
            }
        }
        public virtual void drawArc(ref btVector3 center,ref btVector3 normal,ref btVector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, 
				btVector3 color, bool drawSect)
	    {
            drawArc(ref center,ref normal,ref axis,radiusA,radiusB,minAngle,maxAngle,color,drawSect,10f);
        }
        public virtual void drawArc(ref btVector3 center,ref btVector3 normal,ref btVector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, 
				btVector3 color, bool drawSect, float stepDegrees)
        {
            btVector3 vx = axis;
            btVector3 vy = normal.cross(axis);
            float step = stepDegrees * BulletGlobal.SIMD_RADS_PER_DEG;
            int nSteps = (int)((maxAngle - minAngle) / step);
            if (nSteps == 0) nSteps = 1;
            btVector3 prev;// = center + radiusA * vx * (float)Math.Cos(minAngle) + radiusB * vy * (float)Math.Sin(minAngle);
            {
                btVector3 temp1, temp2, temp3,temp4,temp5;
                btVector3.Multiply(ref vx, radiusA, out temp1);
                btVector3.Multiply(ref temp1, (float)Math.Cos(minAngle), out temp2);
                btVector3.Multiply(ref vy, radiusB, out temp3);
                btVector3.Multiply(ref temp3, (float)Math.Sin(minAngle), out temp4);
                btVector3.Add(ref center, ref temp2, out temp5);
                btVector3.Add(ref temp5, ref temp4, out prev);
            }
            if (drawSect)
            {
                drawLine(ref center, ref prev, ref color);
            }
            for (int i = 1; i <= nSteps; i++)
            {
                float angle = minAngle + (maxAngle - minAngle) * (float)(i) / (float)(nSteps);
                btVector3 next;// = center + radiusA * vx * (float)Math.Cos(angle) + radiusB * vy * (float)Math.Cos(angle);
                {
                    btVector3 temp1, temp2, temp3, temp4, temp5;
                    btVector3.Multiply(ref vx, radiusA, out temp1);
                    btVector3.Multiply(ref temp1, (float)Math.Cos(angle), out temp2);
                    btVector3.Multiply(ref vy, radiusB, out temp3);
                    btVector3.Multiply(ref temp3, (float)Math.Cos(angle), out temp4);
                    btVector3.Add(ref center, ref temp2, out temp5);
                    btVector3.Add(ref temp5, ref temp4, out next);
                }
                drawLine(ref prev, ref next, ref color);
                prev = next;
            }
            if (drawSect)
            {
                drawLine(ref center, ref prev, ref color);
            }
        }
	    public virtual void drawSpherePatch(ref btVector3 center,ref btVector3 up,ref btVector3 axis, float radius, 
		    float minTh, float maxTh, float minPs, float maxPs, btVector3 color)
	    {
            drawSpherePatch(ref center,ref up,ref axis,radius,minTh,maxTh,minPs,maxPs,ref color, 10f);
        }
        public virtual unsafe void drawSpherePatch(ref btVector3 center,ref btVector3 up,ref btVector3 axis, float radius, 
		    float minTh, float maxTh, float minPs, float maxPs,ref btVector3 color, float stepDegrees)
	    {
		    //btVector3* vA=stackalloc btVector3[74];
		    //btVector3* vB=stackalloc btVector3[74];
            StackPtr<btVector3> vA = StackPtr<btVector3>.Allocate(74);
            StackPtr<btVector3> vB = StackPtr<btVector3>.Allocate(74);
            try
            {
                fixed (btVector3* vA_ptr = &vA.Array[0], vB_ptr = &vB.Array[0])
                {
                    btVector3* pvA = vA_ptr, pvB = vB_ptr, pT;
                    btVector3 npole;// = center + up * radius;
                    btVector3 spole;// = center - up * radius;
                    {
                        btVector3 temp1;
                        btVector3.Multiply(ref up, radius, out temp1);
                        btVector3.Add(ref center, ref temp1, out npole);
                        btVector3.Subtract(ref center, ref temp1, out spole);
                    }
                    btVector3 arcStart = btVector3.Zero;
                    float step = stepDegrees * BulletGlobal.SIMD_RADS_PER_DEG;
                    btVector3 kv = up;
                    btVector3 iv = axis;
                    btVector3 jv = kv.cross(iv);
                    bool drawN = false;
                    bool drawS = false;
                    if (minTh <= -BulletGlobal.SIMD_HALF_PI)
                    {
                        minTh = -BulletGlobal.SIMD_HALF_PI + step;
                        drawN = true;
                    }
                    if (maxTh >= BulletGlobal.SIMD_HALF_PI)
                    {
                        maxTh = BulletGlobal.SIMD_HALF_PI - step;
                        drawS = true;
                    }
                    if (minTh > maxTh)
                    {
                        minTh = -BulletGlobal.SIMD_HALF_PI + step;
                        maxTh = BulletGlobal.SIMD_HALF_PI - step;
                        drawN = drawS = true;
                    }
                    int n_hor = (int)((maxTh - minTh) / step) + 1;
                    if (n_hor < 2) n_hor = 2;
                    float step_h = (maxTh - minTh) / (float)(n_hor - 1);
                    bool isClosed = false;
                    if (minPs > maxPs)
                    {
                        minPs = -BulletGlobal.SIMD_PI + step;
                        maxPs = BulletGlobal.SIMD_PI;
                        isClosed = true;
                    }
                    else if ((maxPs - minPs) >= BulletGlobal.SIMD_PI * (2f))
                    {
                        isClosed = true;
                    }
                    else
                    {
                        isClosed = false;
                    }
                    int n_vert = (int)((maxPs - minPs) / step) + 1;
                    if (n_vert < 2) n_vert = 2;
                    float step_v = (maxPs - minPs) / (float)(n_vert - 1);
                    for (int i = 0; i < n_hor; i++)
                    {
                        float th = minTh + (float)(i) * step_h;
                        float sth = radius * (float)Math.Sin(th);
                        float cth = radius * (float)Math.Cos(th);
                        for (int j = 0; j < n_vert; j++)
                        {
                            float psi = minPs + (float)(j) * step_v;
                            float sps = (float)Math.Sin(psi);
                            float cps = (float)Math.Cos(psi);
                            #region pvB[j] = center + cth * cps * iv + cth * sps * jv + sth * kv;
                            {
                                btVector3 temp1, temp2, temp3, temp4;
                                btVector3.Multiply(ref iv, cth * cps, out temp1);
                                btVector3.Multiply(ref jv, cth * sps, out temp2);
                                btVector3.Multiply(ref kv, sth, out temp3);
                                btVector3.Add(ref center, ref temp1, out temp4);
                                btVector3.Add(ref temp4, ref temp3, out pvB[j]);
                            }
                            #endregion
                            if (i != 0)
                            {
                                drawLine(ref pvA[j], ref  pvB[j], ref color);
                            }
                            else if (drawS)
                            {
                                drawLine(ref spole, ref  pvB[j], ref color);
                            }
                            if (j != 0)
                            {
                                drawLine(ref pvB[j - 1], ref  pvB[j], ref color);
                            }
                            else
                            {
                                arcStart = pvB[j];
                            }
                            if ((i == (n_hor - 1)) && drawN)
                            {
                                drawLine(ref npole, ref pvB[j], ref color);
                            }
                            if (isClosed)
                            {
                                if (j == (n_vert - 1))
                                {
                                    drawLine(ref arcStart, ref pvB[j], ref color);
                                }
                            }
                            else
                            {
                                if (((i == 0) || (i == (n_hor - 1))) && ((j == 0) || (j == (n_vert - 1))))
                                {
                                    drawLine(ref center, ref pvB[j], ref color);
                                }
                            }
                        }
                        pT = pvA; pvA = pvB; pvB = pT;
                    }
                }
            }
            finally
            {
                vA.Dispose();
                vB.Dispose();
            }
	    }

        public virtual void drawBox(ref btVector3 bbMin,ref btVector3 bbMax,ref btVector3 color)
        {
            btVector3 temp1, temp2;
            //drawLine(new btVector3(bbMin.X, bbMin.Y, bbMin.Z), new btVector3(bbMax.X, bbMin.Y, bbMin.Z), color);
            temp1 = new btVector3(bbMin.X, bbMin.Y, bbMin.Z);
            temp2 = new btVector3(bbMax.X, bbMin.Y, bbMin.Z);
            drawLine(ref temp1, ref temp2,ref color);

            //drawLine(new btVector3(bbMax.X, bbMin.Y, bbMin.Z), new btVector3(bbMax.X, bbMax.Y, bbMin.Z), color);
            temp1 = new btVector3(bbMax.X, bbMin.Y, bbMin.Z);
            temp2 = new btVector3(bbMax.X, bbMax.Y, bbMin.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMax.X, bbMax.Y, bbMin.Z), new btVector3(bbMin.X, bbMax.Y, bbMin.Z), color);
            temp1 = new btVector3(bbMax.X, bbMax.Y, bbMin.Z);
            temp2 = new btVector3(bbMin.X, bbMax.Y, bbMin.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMin.X, bbMax.Y, bbMin.Z), new btVector3(bbMin.X, bbMin.Y, bbMin.Z), color);
            temp1 = new btVector3(bbMin.X, bbMax.Y, bbMin.Z);
            temp2 = new btVector3(bbMin.X, bbMin.Y, bbMin.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMin.X, bbMin.Y, bbMin.Z), new btVector3(bbMin.X, bbMin.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMin.X, bbMin.Y, bbMin.Z);
            temp2 = new btVector3(bbMin.X, bbMin.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);

            //drawLine(new btVector3(bbMax.X, bbMin.Y, bbMin.Z), new btVector3(bbMax.X, bbMin.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMax.X, bbMin.Y, bbMin.Z);
            temp2 = new btVector3(bbMax.X, bbMin.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMax.X, bbMax.Y, bbMin.Z), new btVector3(bbMax.X, bbMax.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMax.X, bbMax.Y, bbMin.Z);
            temp2 = new btVector3(bbMax.X, bbMax.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);

            //drawLine(new btVector3(bbMin.X, bbMax.Y, bbMin.Z), new btVector3(bbMin.X, bbMax.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMin.X, bbMax.Y, bbMin.Z);
            temp2 = new btVector3(bbMin.X, bbMax.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMin.X, bbMin.Y, bbMax.Z), new btVector3(bbMax.X, bbMin.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMin.X, bbMin.Y, bbMax.Z);
            temp2 = new btVector3(bbMax.X, bbMin.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMax.X, bbMin.Y, bbMax.Z), new btVector3(bbMax.X, bbMax.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMax.X, bbMin.Y, bbMax.Z);
            temp2 = new btVector3(bbMax.X, bbMax.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMax.X, bbMax.Y, bbMax.Z), new btVector3(bbMin.X, bbMax.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMax.X, bbMax.Y, bbMax.Z);
            temp2 = new btVector3(bbMin.X, bbMax.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);
            
            //drawLine(new btVector3(bbMin.X, bbMax.Y, bbMax.Z), new btVector3(bbMin.X, bbMin.Y, bbMax.Z), color);
            temp1 = new btVector3(bbMin.X, bbMax.Y, bbMax.Z);
            temp2 = new btVector3(bbMin.X, bbMin.Y, bbMax.Z);
            drawLine(ref temp1, ref temp2, ref color);
        }
	    public virtual void drawBox(ref btVector3 bbMin,ref btVector3 bbMax,ref btTransform trans,ref btVector3 color)
        {
            btVector3 temp1, temp2, temp3, temp4;
            //drawLine(trans * new btVector3(bbMin.X, bbMin.Y, bbMin.Z), trans * new btVector3(bbMax.X, bbMin.Y, bbMin.Z), color);
            {
                temp1 = new btVector3(bbMin.X, bbMin.Y, bbMin.Z);
                temp2 = new btVector3(bbMax.X, bbMin.Y, bbMin.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMax.X, bbMin.Y, bbMin.Z), trans * new btVector3(bbMax.X, bbMax.Y, bbMin.Z), color);
            {
                temp1 = new btVector3(bbMax.X, bbMin.Y, bbMin.Z);
                temp2 = new btVector3(bbMax.X, bbMax.Y, bbMin.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMax.X, bbMax.Y, bbMin.Z), trans * new btVector3(bbMin.X, bbMax.Y, bbMin.Z), color);
            {
                temp1 = new btVector3(bbMax.X, bbMax.Y, bbMin.Z);
                temp2 = new btVector3(bbMin.X, bbMax.Y, bbMin.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMin.X, bbMax.Y, bbMin.Z), trans * new btVector3(bbMin.X, bbMin.Y, bbMin.Z), color);
            {
                temp1 = new btVector3(bbMin.X, bbMax.Y, bbMin.Z);
                temp2 = new btVector3(bbMin.X, bbMin.Y, bbMin.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMin.X, bbMin.Y, bbMin.Z), trans * new btVector3(bbMin.X, bbMin.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMin.X, bbMin.Y, bbMin.Z);
                temp2 = new btVector3(bbMin.X, bbMin.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMax.X, bbMin.Y, bbMin.Z), trans * new btVector3(bbMax.X, bbMin.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMax.X, bbMin.Y, bbMin.Z);
                temp2 = new btVector3(bbMax.X, bbMin.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMax.X, bbMax.Y, bbMin.Z), trans * new btVector3(bbMax.X, bbMax.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMax.X, bbMax.Y, bbMin.Z);
                temp2 = new btVector3(bbMax.X, bbMax.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMin.X, bbMax.Y, bbMin.Z), trans * new btVector3(bbMin.X, bbMax.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMin.X, bbMax.Y, bbMin.Z);
                temp2 = new btVector3(bbMin.X, bbMax.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMin.X, bbMin.Y, bbMax.Z), trans * new btVector3(bbMax.X, bbMin.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMin.X, bbMin.Y, bbMax.Z);
                temp2 = new btVector3(bbMax.X, bbMin.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMax.X, bbMin.Y, bbMax.Z), trans * new btVector3(bbMax.X, bbMax.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMax.X, bbMin.Y, bbMax.Z);
                temp2 = new btVector3(bbMax.X, bbMax.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMax.X, bbMax.Y, bbMax.Z), trans * new btVector3(bbMin.X, bbMax.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMax.X, bbMax.Y, bbMax.Z);
                temp2 = new btVector3(bbMin.X, bbMax.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
            //drawLine(trans * new btVector3(bbMin.X, bbMax.Y, bbMax.Z), trans * new btVector3(bbMin.X, bbMin.Y, bbMax.Z), color);
            {
                temp1 = new btVector3(bbMin.X, bbMax.Y, bbMax.Z);
                temp2 = new btVector3(bbMin.X, bbMin.Y, bbMax.Z);
                btTransform.Multiply(ref trans, ref temp1, out temp3);
                btTransform.Multiply(ref trans, ref temp2, out temp4);
                drawLine(ref temp3, ref temp4, ref color);
            }
        }
    }
}
