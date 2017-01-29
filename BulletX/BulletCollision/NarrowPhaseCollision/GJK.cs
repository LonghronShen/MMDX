using System;
using System.Collections.Generic;
using BulletX.LinerMath;
using tShape = BulletX.BulletCollision.NarrowPhaseCollision.MinkowskiDiff;
using U = System.UInt32;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    unsafe class GJK : IDisposable
    {
        const int GJK_MAX_ITERATIONS = 128;
        const float GJK_ACCURARY = 0.0001f;
        const float GJK_MIN_DISTANCE = 0.0001f;
        const float GJK_DUPLICATED_EPS = 0.0001f;
        const float GJK_SIMPLEX2_EPS = 0.0f;
        const float GJK_SIMPLEX3_EPS = 0.0f;
        const float GJK_SIMPLEX4_EPS = 0.0f;
        static U[] imd3 = { 1, 2, 0 };

        /* Types		*/
        public class sSV
        {
            public btVector3 d, w;

            internal void Constructor()
            {
                d = btVector3.Zero;
                w = btVector3.Zero;
            }
        };
        public class sSimplex
        {
            public sSV[] c = new sSV[4];
            public float[] p = new float[4];
            public U rank;

            internal void Constructor()
            {
                Array.Clear(c, 0, c.Length);
                Array.Clear(p, 0, p.Length);
                rank = 0;
            }
        };
        public enum eStatus
        {
            Valid,
            Inside,
            Failed
        };
        /* Fields		*/
        public tShape m_shape;
        public btVector3 m_ray;
        public float m_distance;
        public sSimplex[] m_simplices = new sSimplex[2] { new sSimplex(), new sSimplex() };
        public sSV[] m_store = new sSV[4] { new sSV(), new sSV(), new sSV(), new sSV() };
        public sSV[] m_free = new sSV[4];
        public U m_nfree;
        public U m_current;
        public sSimplex m_simplex;
        public eStatus m_status;



        static Queue<GJK> ObjPool = new Queue<GJK>();
        public static GJK CreateFromPool()
        {
            GJK result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new GJK();
            result.Constructor();
            return result;
        }
        private GJK() { }
        private void Constructor()
        {
            m_ray = new btVector3(0, 0, 0);
            m_nfree = 0;
            m_status = eStatus.Failed;
            m_current = 0;
            m_distance = 0;
            for (int i = 0; i < m_simplices.Length; i++)
                m_simplices[i].Constructor();
            for (int i = 0; i < m_store.Length; i++)
                m_store[i].Constructor();
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            ObjPool.Enqueue(this);
        }

        #endregion

        public eStatus Evaluate(tShape shapearg, btVector3 guess)
        {
            U iterations = 0;
            float sqdist = 0;
            float alpha = 0;
            //btVector3	*lastw=stackalloc btVector3[4];
            //btVector3[] lastw = new btVector3[4];
            StackPtr<btVector3> lastw = StackPtr<btVector3>.Allocate(4);
            try
            {
                U clastw = 0;
                /* Initialize solver		*/
                m_free[0] = m_store[0];
                m_free[1] = m_store[1];
                m_free[2] = m_store[2];
                m_free[3] = m_store[3];
                m_nfree = 4;
                m_current = 0;
                m_status = eStatus.Valid;
                m_shape = shapearg;
                m_distance = 0;
                /* Initialize simplex		*/
                m_simplices[0].rank = 0;
                m_ray = guess;
                float sqrl = m_ray.Length2;
                appendvertice(m_simplices[0], sqrl > 0 ? -m_ray : new btVector3(1, 0, 0));
                m_simplices[0].p[0] = 1;
                m_ray = m_simplices[0].c[0].w;
                sqdist = sqrl;
                lastw[0] =
                    lastw[1] =
                    lastw[2] =
                    lastw[3] = m_ray;
                /* Loop						*/
                do
                {
                    U next = 1 - m_current;
                    sSimplex cs = m_simplices[m_current];
                    sSimplex ns = m_simplices[next];
                    /* Check zero							*/
                    float rl = m_ray.Length;
                    if (rl < GJK_MIN_DISTANCE)
                    {/* Touching or inside				*/
                        m_status = eStatus.Inside;
                        break;
                    }
                    /* Append new vertice in -'v' direction	*/
                    appendvertice(cs, -m_ray);
                    btVector3 w = cs.c[cs.rank - 1].w;
                    bool found = false;
                    for (U i = 0; i < 4; ++i)
                    {
                        if ((w - lastw[i]).Length2 < GJK_DUPLICATED_EPS)
                        { found = true; break; }
                    }
                    if (found)
                    {/* Return old simplex				*/
                        removevertice(m_simplices[m_current]);
                        break;
                    }
                    else
                    {/* Update lastw					*/
                        lastw[clastw = (clastw + 1) & 3] = w;
                    }
                    /* Check for termination				*/
                    float omega = btVector3.dot(m_ray, w) / rl;
                    alpha = (float)Math.Max(omega, alpha);
                    if (((rl - alpha) - (GJK_ACCURARY * rl)) <= 0)
                    {/* Return old simplex				*/
                        removevertice(m_simplices[m_current]);
                        break;
                    }
                    /* Reduce simplex						*/
                    //float* weights = stackalloc float[4];
                    StackPtr<float> weights = StackPtr<float>.Allocate(4);
                    try
                    {
                        U mask = 0;
                        switch (cs.rank)
                        {
                            case 2: sqdist = projectorigin(cs.c[0].w,
                                        cs.c[1].w,
                                        weights, ref mask); break;
                            case 3: sqdist = projectorigin(cs.c[0].w,
                                        cs.c[1].w,
                                        cs.c[2].w,
                                        weights, ref mask); break;
                            case 4: sqdist = projectorigin(cs.c[0].w,
                                        cs.c[1].w,
                                        cs.c[2].w,
                                        cs.c[3].w,
                                        weights, ref mask); break;
                        }
                        if (sqdist >= 0)
                        {/* Valid	*/
                            ns.rank = 0;
                            m_ray = new btVector3(0, 0, 0);
                            m_current = next;
                            for (U i = 0, ni = cs.rank; i < ni; ++i)
                            {
                                if ((mask & (1 << (int)i)) != 0)
                                {
                                    ns.c[ns.rank] = cs.c[i];
                                    ns.p[ns.rank++] = weights[i];
                                    #region m_ray += cs.c[i].w * weights[i];
                                    {
                                        btVector3 temp;
                                        btVector3.Multiply(ref cs.c[i].w, weights[i], out temp);
                                        m_ray.Add(ref temp);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    m_free[m_nfree++] = cs.c[i];
                                }
                            }
                            if (mask == 15) m_status = eStatus.Inside;
                        }
                        else
                        {/* Return old simplex				*/
                            removevertice(m_simplices[m_current]);
                            break;
                        }
                        m_status = ((++iterations) < GJK_MAX_ITERATIONS) ? m_status : eStatus.Failed;
                    }
                    finally
                    {
                        weights.Dispose();
                    }
                } while (m_status == eStatus.Valid);
                m_simplex = m_simplices[m_current];
                switch (m_status)
                {
                    case eStatus.Valid: m_distance = m_ray.Length; break;
                    case eStatus.Inside: m_distance = 0; break;
                    default:
                        {
                            break;
                        }
                }
                return (m_status);
            }
            finally
            {
                lastw.Dispose();
            }
        }
        public bool EncloseOrigin()
        {
            switch (m_simplex.rank)
            {
                case 1:
                    {
                        for (U i = 0; i < 3; ++i)
                        {
                            btVector3 axis = new btVector3(0, 0, 0);
                            axis[(int)i] = 1;
                            appendvertice(m_simplex, axis);
                            if (EncloseOrigin()) return (true);
                            removevertice(m_simplex);
                            appendvertice(m_simplex, -axis);
                            if (EncloseOrigin()) return (true);
                            removevertice(m_simplex);
                        }
                    }
                    break;
                case 2:
                    {
                        btVector3 d = m_simplex.c[1].w - m_simplex.c[0].w;
                        for (U i = 0; i < 3; ++i)
                        {
                            btVector3 axis = new btVector3(0, 0, 0);
                            axis[(int)i] = 1;
                            btVector3 p = btVector3.cross(d, axis);
                            if (p.Length2 > 0)
                            {
                                appendvertice(m_simplex, p);
                                if (EncloseOrigin()) return (true);
                                removevertice(m_simplex);
                                appendvertice(m_simplex, -p);
                                if (EncloseOrigin()) return (true);
                                removevertice(m_simplex);
                            }
                        }
                    }
                    break;
                case 3:
                    {
                        btVector3 n = btVector3.cross(m_simplex.c[1].w - m_simplex.c[0].w,
                            m_simplex.c[2].w - m_simplex.c[0].w);
                        if (n.Length2 > 0)
                        {
                            appendvertice(m_simplex, n);
                            if (EncloseOrigin()) return (true);
                            removevertice(m_simplex);
                            appendvertice(m_simplex, -n);
                            if (EncloseOrigin()) return (true);
                            removevertice(m_simplex);
                        }
                    }
                    break;
                case 4:
                    {
                        if ((float)Math.Abs(det(m_simplex.c[0].w - m_simplex.c[3].w,
                            m_simplex.c[1].w - m_simplex.c[3].w,
                            m_simplex.c[2].w - m_simplex.c[3].w)) > 0)
                            return (true);
                    }
                    break;
            }
            return (false);
        }
        /* Internals	*/
        public void getsupport(btVector3 d, sSV sv)
        {
            sv.d = d / d.Length;
            //sv.w = m_shape.Support(sv.d);
            m_shape.Support(ref sv.d, out sv.w);
        }
        void removevertice(sSimplex simplex)
        {
            m_free[m_nfree++] = simplex.c[--simplex.rank];
        }
        void appendvertice(sSimplex simplex, btVector3 v)
        {
            simplex.p[simplex.rank] = 0;
            simplex.c[simplex.rank] = m_free[--m_nfree];
            getsupport(v, simplex.c[simplex.rank++]);
        }
        public static float det(btVector3 a, btVector3 b, btVector3 c)
        {
            return (a.Y * b.Z * c.X + a.Z * b.X * c.Y -
                a.X * b.Z * c.Y - a.Y * b.X * c.Z +
                a.X * b.Y * c.Z - a.Z * b.Y * c.X);
        }
        static float projectorigin(btVector3 a, btVector3 b, float[] w, ref U m)
        {
            btVector3 d = b - a;
            float l = d.Length2;
            if (l > GJK_SIMPLEX2_EPS)
            {
                float t = (l > 0 ? -btVector3.dot(a, d) / l : 0);
                if (t >= 1) 
                { 
                    w[0] = 0;
                    w[1] = 1;
                    m = 2;
                    return (b.Length2);
                }
                else if (t <= 0) 
                { 
                    w[0] = 1;
                    w[1] = 0;
                    m = 1;
                    return (a.Length2); }
                else
                {
                    w[0] = 1 - (w[1] = t);
                    m = 3;
                    #region return ((a + d * t).Length2);
                    {
                        btVector3 temp1, temp2;
                        btVector3.Multiply(ref d, t, out temp1);
                        btVector3.Add(ref a, ref temp1, out temp2);
                        return temp2.Length2;
                    }
                    #endregion
                }
            }
            return (-1);
        }
        static btVector3*[] vt3 = new btVector3*[3];
        static btVector3[] dl = new btVector3[3];
        unsafe static float projectorigin(btVector3 a, btVector3 b, btVector3 c, float[] w, ref U m)
        {
            vt3[0] = &a;
            vt3[1] = &b;
            vt3[2] = &c;
            dl[0] = a - b;
            dl[1] = b - c;
            dl[2] = c - a;
            btVector3 n = btVector3.cross(dl[0], dl[1]);
            float l = n.Length2;
            if (l > GJK_SIMPLEX3_EPS)
            {
                float mindist = -1;
                //float* subw = stackalloc float[2];
                StackPtr<float> subw = StackPtr<float>.Allocate(2);
                try
                {
                    subw[0] = 0f;
                    subw[1] = 0f;
                    U subm = (0);
                    for (U i = 0; i < 3; ++i)
                    {
                        if (btVector3.dot(*vt3[i], btVector3.cross(dl[i], n)) > 0)
                        {
                            U j = imd3[i];
                            float subd = (projectorigin(*vt3[i], *vt3[j], subw, ref subm));
                            if ((mindist < 0) || (subd < mindist))
                            {
                                mindist = subd;
                                m = (U)((((subm & 1) != 0) ? 1 << (int)i : 0) + (((subm & 2) != 0) ? 1 << (int)j : 0));
                                w[i] = subw[0];
                                w[j] = subw[1];
                                w[imd3[j]] = 0;
                            }
                        }
                    }
                    if (mindist < 0)
                    {
                        float d = btVector3.dot(a, n);
                        float s = (float)Math.Sqrt(l);
                        btVector3 p = n * (d / l);
                        mindist = p.Length2;
                        m = 7;
                        w[0] = (btVector3.cross(dl[1], b - p)).Length / s;
                        w[1] = (btVector3.cross(dl[2], c - p)).Length / s;
                        w[2] = 1 - (w[0] + w[1]);
                    }
                    return (mindist);
                }
                finally
                {
                    subw.Dispose();
                }
            }
            return (-1);
        }
        static btVector3*[] vt4 = new btVector3*[4];
        unsafe static float projectorigin(btVector3 a, btVector3 b, btVector3 c, btVector3 d, float[] w, ref U m)
        {
            vt4[0] = &a;
            vt4[1] = &b;
            vt4[2] = &c;
            vt4[3] = &d;
            dl[0] = a - d;
            dl[1] = b - d;
            dl[2] = c - d;
            float vl = det(dl[0], dl[1], dl[2]);
            bool ng = (vl * btVector3.dot(a, btVector3.cross(b - c, a - b))) <= 0;
            if (ng && ((float)Math.Abs(vl) > GJK_SIMPLEX4_EPS))
            {
                float mindist = -1;
                //float* subw = stackalloc float[3];
                StackPtr<float> subw = StackPtr<float>.Allocate(3);
                try
                {
                    subw[0] = 0f;
                    subw[1] = 0f;
                    subw[2] = 0f;
                    U subm = (0);
                    for (U i = 0; i < 3; ++i)
                    {
                        U j = imd3[i];
                        float s = vl * btVector3.dot(d, btVector3.cross(dl[i], dl[j]));
                        if (s > 0)
                        {
                            float subd = projectorigin(*vt4[i], *vt4[j], d, subw, ref subm);
                            if ((mindist < 0) || (subd < mindist))
                            {
                                mindist = subd;
                                m = (U)(((subm & 1) != 0 ? 1 << (int)i : 0) +
                                    ((subm & 2) != 0 ? 1 << (int)j : 0) +
                                    ((subm & 4) != 0 ? 8 : 0));
                                w[i] = subw[0];
                                w[j] = subw[1];
                                w[imd3[j]] = 0;
                                w[3] = subw[2];
                            }
                        }
                    }
                    if (mindist < 0)
                    {
                        mindist = 0;
                        m = 15;
                        w[0] = det(c, b, d) / vl;
                        w[1] = det(a, c, d) / vl;
                        w[2] = det(b, a, d) / vl;
                        w[3] = 1 - (w[0] + w[1] + w[2]);
                    }
                    return (mindist);
                }
                finally
                {
                    subw.Dispose();
                }
            }
            return (-1);
        }
    }
}
