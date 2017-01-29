using System;
using System.Collections.Generic;
using BulletX.LinerMath;
using sSV = BulletX.BulletCollision.NarrowPhaseCollision.GJK.sSV;
using U = System.UInt32;
using U1 = System.Char;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    class EPA : IDisposable
    {

        const int EPA_MAX_VERTICES = 64;
        const int EPA_MAX_FACES = (EPA_MAX_VERTICES * 2);
        const int EPA_MAX_ITERATIONS = 255;
        const float EPA_ACCURACY = 0.0001f;
        const float EPA_FALLBACK = 10 * EPA_ACCURACY;
        const float EPA_PLANE_EPS = 0.00001f;
        const float EPA_INSIDE_EPS = 0.01f;
        /* Types		*/
        //typedef	GJK::sSV	sSV;
        public class sFace : IDisposable
        {
            public btVector3 n;
            public float d;
            public float p;
            public sSV[] c = new sSV[3];
            public sFace[] f = new sFace[3];
            public sFace[] l = new sFace[2];
            public U1[] e = new U1[3];
            public U1 pass;

            #region ICloneable メンバ
            static Queue<sFace> ObjPool = new Queue<sFace>();
            public sFace Clone()
            {
                sFace result;
                if (ObjPool.Count > 0)
                    result = ObjPool.Dequeue();
                else
                    result = new sFace();
                result.n = n;
                result.d = d;
                result.p = p;
                Array.Copy(c, result.c, c.Length);//多分用途的に浅いコピー
                Array.Copy(f, result.f, f.Length);
                Array.Copy(e, result.e, e.Length);
                result.pass = pass;
                return result;
            }

            #endregion

            #region IDisposable メンバ

            public void Dispose()
            {
                ObjPool.Enqueue(this);
            }

            #endregion
        };
        public struct sList
        {
            public sFace root;
            public U count;
            internal void Constructor() { root = null; count = 0; }
        };
        public struct sHorizon
        {
            public sFace cf;
            public sFace ff;
            public U nf;

            internal void Init()
            {
                cf = null; ff = null; nf = 0;
            }
        };
        public enum eStatus
        {
            Valid,
            Touching,
            Degenerated,
            NonConvex,
            InvalidHull,
            OutOfFaces,
            OutOfVertices,
            AccuraryReached,
            FallBack,
            Failed
        };
        /* Fields		*/
        public eStatus m_status;
        public GJK.sSimplex m_result = new GJK.sSimplex();
        public btVector3 m_normal;
        public float m_depth;
        public sSV[] m_sv_store = new sSV[EPA_MAX_VERTICES];
        public sFace[] m_fc_store = new sFace[EPA_MAX_FACES];
        public U m_nextsv;
        public sList m_hull;
        public sList m_stock;

        static Queue<EPA> ObjPool = new Queue<EPA>();

        public static EPA CreateFromPool()
        {
            EPA result;
            if (ObjPool.Count > 0)
                result = ObjPool.Dequeue();
            else
                result = new EPA();
            result.Constructor();
            return result;
        }

        private EPA()
        {
            for (int i = 0; i < EPA_MAX_VERTICES; i++)
                m_sv_store[i] = new sSV();
            for (int i = 0; i < EPA_MAX_FACES; i++)
                m_fc_store[i] = new sFace();
        }
        private void Constructor()
        {
            m_status = eStatus.Failed;
            m_normal = new btVector3(0, 0, 0);
            m_depth = 0;
            m_nextsv = 0;
            m_hull.Constructor();
            m_stock.Constructor();
            for (U i = 0; i < EPA_MAX_FACES; ++i)
            {
                append(ref m_stock, m_fc_store[EPA_MAX_FACES - i - 1]);
            }
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            ObjPool.Enqueue(this);
        }

        #endregion

        static void bind(sFace fa, U ea, sFace fb, U eb)
        {
            fa.e[ea] = (U1)eb; fa.f[ea] = fb;
            fb.e[eb] = (U1)ea; fb.f[eb] = fa;
        }
        static void append(ref sList list, sFace face)
        {
            face.l[0] = null;
            face.l[1] = list.root;
            if (list.root != null) list.root.l[0] = face;
            list.root = face;
            ++list.count;
        }
        static void remove(ref sList list, sFace face)
        {
            if (face.l[1] != null) face.l[1].l[0] = face.l[0];
            if (face.l[0] != null) face.l[0].l[1] = face.l[1];
            if (face == list.root) list.root = face.l[1];
            --list.count;
        }
        sFace[] tetra = new sFace[4];//ローカルから昇格
        public eStatus Evaluate(GJK gjk, btVector3 guess)
        {
            GJK.sSimplex simplex = gjk.m_simplex;
            if ((simplex.rank > 1) && gjk.EncloseOrigin())
            {

                /* Clean up				*/
                while (m_hull.root != null)
                {
                    sFace f = m_hull.root;
                    remove(ref m_hull, f);
                    append(ref m_stock, f);
                }
                m_status = eStatus.Valid;
                m_nextsv = 0;
                /* Orient simplex		*/
                if (GJK.det(simplex.c[0].w - simplex.c[3].w,
                    simplex.c[1].w - simplex.c[3].w,
                    simplex.c[2].w - simplex.c[3].w) < 0)
                {
                    BulletGlobal.Swap<sSV>(ref simplex.c[0], ref simplex.c[1]);
                    BulletGlobal.Swap<float>(ref simplex.p[0], ref simplex.p[1]);
                }
                /* Build initial hull	*/
                tetra[0] = newface(simplex.c[0], simplex.c[1], simplex.c[2], true);
                tetra[1] = newface(simplex.c[1], simplex.c[0], simplex.c[3], true);
                tetra[2] = newface(simplex.c[2], simplex.c[1], simplex.c[3], true);
                tetra[3] = newface(simplex.c[0], simplex.c[2], simplex.c[3], true);
                if (m_hull.count == 4)
                {
                    sFace best = findbest();
                    sFace outer = best.Clone();
                    try
                    {
                        U pass = 0;
                        U iterations = 0;
                        bind(tetra[0], 0, tetra[1], 0);
                        bind(tetra[0], 1, tetra[2], 0);
                        bind(tetra[0], 2, tetra[3], 0);
                        bind(tetra[1], 1, tetra[3], 2);
                        bind(tetra[1], 2, tetra[2], 1);
                        bind(tetra[2], 2, tetra[3], 1);
                        m_status = eStatus.Valid;
                        for (; iterations < EPA_MAX_ITERATIONS; ++iterations)
                        {
                            if (m_nextsv < EPA_MAX_VERTICES)
                            {
                                sHorizon horizon = new sHorizon();
                                horizon.Init();
                                sSV w = m_sv_store[m_nextsv++];
                                bool valid = true;
                                best.pass = (U1)(++pass);
                                gjk.getsupport(best.n, w);
                                float wdist = btVector3.dot(best.n, w.w) - best.d;
                                if (wdist > EPA_ACCURACY)
                                {
                                    for (U j = 0; (j < 3) && valid; ++j)
                                    {
                                        valid &= expand(pass, w,
                                            best.f[j], best.e[j],
                                            ref horizon);
                                    }
                                    if (valid && (horizon.nf >= 3))
                                    {
                                        bind(horizon.cf, 1, horizon.ff, 2);
                                        remove(ref m_hull, best);
                                        append(ref m_stock, best);
                                        best = findbest();
                                        if (best.p >= outer.p)
                                        {
                                            outer.Dispose();
                                            outer = best.Clone();
                                        }
                                    }
                                    else { m_status = eStatus.InvalidHull; break; }
                                }
                                else { m_status = eStatus.AccuraryReached; break; }
                            }
                            else { m_status = eStatus.OutOfVertices; break; }
                        }
                        btVector3 projection = outer.n * outer.d;
                        m_normal = outer.n;
                        m_depth = outer.d;
                        m_result.rank = 3;
                        m_result.c[0] = outer.c[0];
                        m_result.c[1] = outer.c[1];
                        m_result.c[2] = outer.c[2];
                        m_result.p[0] = btVector3.cross(outer.c[1].w - projection,
                            outer.c[2].w - projection).Length;
                        m_result.p[1] = btVector3.cross(outer.c[2].w - projection,
                            outer.c[0].w - projection).Length;
                        m_result.p[2] = btVector3.cross(outer.c[0].w - projection,
                            outer.c[1].w - projection).Length;
                        float sum = m_result.p[0] + m_result.p[1] + m_result.p[2];
                        m_result.p[0] /= sum;
                        m_result.p[1] /= sum;
                        m_result.p[2] /= sum;
                        return (m_status);
                    }
                    finally
                    {
                        outer.Dispose();
                    }
                }
            }
            /* Fallback		*/
            m_status = eStatus.FallBack;
            m_normal = -guess;
            float nl = m_normal.Length;
            if (nl > 0)
                m_normal = m_normal / nl;
            else
                m_normal = new btVector3(1, 0, 0);
            m_depth = 0;
            m_result.rank = 1;
            m_result.c[0] = simplex.c[0];
            m_result.p[0] = 1;
            return (m_status);
        }
        sFace newface(sSV a, sSV b, sSV c, bool forced)
        {
            if (m_stock.root != null)
            {
                sFace face = m_stock.root;
                remove(ref m_stock, face);
                append(ref m_hull, face);
                face.pass = (char)0;
                face.c[0] = a;
                face.c[1] = b;
                face.c[2] = c;
                face.n = btVector3.cross(b.w - a.w, c.w - a.w);
                float l = face.n.Length;
                bool v = l > EPA_ACCURACY;
                face.p = Math.Min(Math.Min(
                    btVector3.dot(a.w, btVector3.cross(face.n, a.w - b.w)),
                    btVector3.dot(b.w, btVector3.cross(face.n, b.w - c.w))),
                    btVector3.dot(c.w, btVector3.cross(face.n, c.w - a.w))) /
                    (v ? l : 1);
                face.p = face.p >= -EPA_INSIDE_EPS ? 0 : face.p;
                if (v)
                {
                    face.d = btVector3.dot(a.w, face.n) / l;
                    face.n /= l;
                    if (forced || (face.d >= -EPA_PLANE_EPS))
                    {
                        return (face);
                    }
                    else m_status = eStatus.NonConvex;
                }
                else m_status = eStatus.Degenerated;
                remove(ref m_hull, face);
                append(ref m_stock, face);
                return null;
            }
            m_status = m_stock.root != null ? eStatus.OutOfVertices : eStatus.OutOfFaces;
            return null;
        }
        sFace findbest()
        {
            sFace minf = m_hull.root;
            float mind = minf.d * minf.d;
            float maxp = minf.p;
            for (sFace f = minf.l[1]; f != null; f = f.l[1])
            {
                float sqd = f.d * f.d;
                if ((f.p >= maxp) && (sqd < mind))
                {
                    minf = f;
                    mind = sqd;
                    maxp = f.p;
                }
            }
            return (minf);
        }
        static U[] i1m3 = { 1, 2, 0 };
        static U[] i2m3 = { 2, 0, 1 };
        bool expand(U pass, sSV w, sFace f, U e, ref sHorizon horizon)
        {
            if (f.pass != pass)
            {
                U e1 = i1m3[e];
                if ((btVector3.dot(f.n, w.w) - f.d) < -EPA_PLANE_EPS)
                {
                    sFace nf = newface(f.c[e1], f.c[e], w, false);
                    if (nf != null)
                    {
                        bind(nf, 0, f, e);
                        if (horizon.cf != null) bind(horizon.cf, 1, nf, 2); else horizon.ff = nf;
                        horizon.cf = nf;
                        ++horizon.nf;
                        return (true);
                    }
                }
                else
                {
                    U e2 = i2m3[e];
                    f.pass = (U1)pass;
                    if (expand(pass, w, f.f[e1], f.e[e1], ref horizon) &&
                        expand(pass, w, f.f[e2], f.e[e2], ref horizon))
                    {
                        remove(ref m_hull, f);
                        append(ref m_stock, f);
                        return (true);
                    }
                }
            }
            return (false);
        }
    }
}
