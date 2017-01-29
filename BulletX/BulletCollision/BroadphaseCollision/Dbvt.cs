using System;
using System.Collections.Generic;
using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    struct sStkNN
    {
        public DbvtNode a;
        public DbvtNode b;
        public sStkNN(DbvtNode na, DbvtNode nb) { a = na; b = nb; }
    }
    public class Dbvt
    {
        const int DOUBLE_STACKSIZE = 128;

        // Fields
        public DbvtNode m_root;
        public DbvtNode m_free;
        public int m_lkhd;
        public int m_leaves;
        public uint m_opath;

        List<sStkNN> m_stkStack = new List<sStkNN>();
        public Dbvt()
        {
            m_root = null;
            m_free = null;
            m_lkhd = -1;
            m_leaves = 0;
            m_opath = 0;
        }

        public DbvtNode insert(ref DbvtAabbMm volume, object data)
        {
            DbvtNode leaf = createnode(this, null,ref volume, data);
            insertleaf(this, m_root, leaf);
            ++m_leaves;
            return (leaf);
        }
        public void collideTV(DbvtNode root, ref DbvtAabbMm vol, ICollide policy)
        {
            if (root != null)
            {
                DbvtAabbMm volume = vol;
                List<DbvtNode> stack = new List<DbvtNode>();
                stack.Add(root);
                do
                {
                    DbvtNode n = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    if (Intersect(ref n.volume,ref volume))
                    {
                        if (n.isinternal())
                        {
                            stack.Add(n.childs[0]);
                            stack.Add(n.childs[1]);
                        }
                        else
                        {
                            policy.Process(n);
                        }
                    }
                } while (stack.Count > 0);
            }
        }
        static DbvtNode createnode(Dbvt pdbvt,
                                           DbvtNode parent,
                                           object data)
        {
            DbvtNode node;
            if (pdbvt.m_free != null)
            {
                node = pdbvt.m_free;
                pdbvt.m_free = null;
            }
            else
            {
                node = new DbvtNode();
            }
            node.parent = parent;
            node.data = data;
            node.childs[1] = null;
            return (node);
        }
        private static DbvtNode createnode(Dbvt pdbvt,
                                           DbvtNode parent,
                                           ref DbvtAabbMm volume,
                                           object data)
        {
            DbvtNode node = createnode(pdbvt, parent, data);
            node.volume = volume;
            return (node);
        }
        private static DbvtNode createnode(Dbvt pdbvt,
                                           DbvtNode parent,
                                           ref DbvtAabbMm volume0,
                                           ref DbvtAabbMm volume1,
                                           object data)
        {
            DbvtNode node = createnode(pdbvt, parent, data);
            Merge(ref volume0,ref volume1,ref node.volume);
            return (node);
        }
        private static void insertleaf(Dbvt pdbvt,
                                           DbvtNode root,
                                           DbvtNode leaf)
        {
            if (pdbvt.m_root == null)
            {
                pdbvt.m_root = leaf;
                leaf.parent = null;
            }
            else
            {
                if (!root.isleaf())
                {
                    do
                    {
                        root = root.childs[Select(ref leaf.volume,
                            ref root.childs[0].volume,
                            ref root.childs[1].volume)];
                    } while (!root.isleaf());
                }
                DbvtNode prev = root.parent;
                DbvtNode node = createnode(pdbvt, prev,ref leaf.volume,ref root.volume, null);
                if (prev != null)
                {
                    prev.childs[indexof(root)] = node;
                    node.childs[0] = root; root.parent = node;
                    node.childs[1] = leaf; leaf.parent = node;
                    do
                    {
                        if (!prev.volume.Contain(ref node.volume))
                            Merge(ref prev.childs[0].volume,ref prev.childs[1].volume,ref prev.volume);
                        else
                            break;
                        node = prev;
                        prev = node.parent;
                    } while (null != prev);
                }
                else
                {
                    node.childs[0] = root; root.parent = node;
                    node.childs[1] = leaf; leaf.parent = node;
                    pdbvt.m_root = node;
                }
            }
        }
        static void Merge(ref DbvtAabbMm a,
                             ref DbvtAabbMm b,
                             ref DbvtAabbMm r)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (a.mi[i] < b.mi[i]) r.mi[i] = a.mi[i]; else r.mi[i] = b.mi[i];
                if (a.mx[i] > b.mx[i]) r.mx[i] = a.mx[i]; else r.mx[i] = b.mx[i];
            }
        }
        static int indexof(DbvtNode node)
        {
            return (node.parent.childs[1] == node) ? 1 : 0;
        }
        static int Select(ref DbvtAabbMm o,
                               ref DbvtAabbMm a,
                               ref DbvtAabbMm b)
        {
            return (Proximity(ref o,ref a) < Proximity(ref o,ref  b) ? 0 : 1);
        }
        static float Proximity(ref DbvtAabbMm a,
                                  ref DbvtAabbMm b)
        {
            btVector3 d;// = (a.mi + a.mx) - (b.mi + b.mx);
            {
                btVector3 temp1, temp2;
                btVector3.Add(ref a.mi, ref a.mx, out temp1);
                btVector3.Add(ref b.mi, ref b.mx, out temp2);
                btVector3.Subtract(ref temp1, ref temp2, out d);
            }
            return (Math.Abs(d.X) + Math.Abs(d.Y) + Math.Abs(d.Z));
        }
        public static bool Intersect(ref DbvtAabbMm a,
                                  ref DbvtAabbMm b)
        {
            return ((a.mi.X <= b.mx.X) &&
                (a.mx.X >= b.mi.X) &&
                (a.mi.Y <= b.mx.Y) &&
                (a.mx.Y >= b.mi.Y) &&
                (a.mi.Z <= b.mx.Z) &&
                (a.mx.Z >= b.mi.Z));
        }
        public void remove(DbvtNode leaf)
        {
            removeleaf(this, leaf);
            deletenode(this, leaf);
            --m_leaves;
        }
        //
        static DbvtNode removeleaf(Dbvt pdbvt, DbvtNode leaf)
        {
            if (leaf == pdbvt.m_root)
            {
                pdbvt.m_root = null;
                return null;
            }
            else
            {
                DbvtNode parent = leaf.parent;
                DbvtNode prev = parent.parent;
                DbvtNode sibling = parent.childs[1 - indexof(leaf)];
                if (prev != null)
                {
                    prev.childs[indexof(parent)] = sibling;
                    sibling.parent = prev;
                    deletenode(pdbvt, parent);
                    while (prev != null)
                    {
                        DbvtAabbMm pb = prev.volume;
                        Merge(ref prev.childs[0].volume,ref prev.childs[1].volume,ref prev.volume);
                        if (NotEqual(ref pb, ref prev.volume))
                        {
                            prev = prev.parent;
                        }
                        else break;
                    }
                    return (prev != null ? prev : pdbvt.m_root);
                }
                else
                {
                    pdbvt.m_root = sibling;
                    sibling.parent = null;
                    deletenode(pdbvt, parent);
                    return (pdbvt.m_root);
                }
            }
        }
        static void deletenode(Dbvt pdbvt, DbvtNode node)
        {
            //btAlignedFree(pdbvt->m_free);
            pdbvt.m_free = node;
        }
        static bool NotEqual(ref DbvtAabbMm a, ref DbvtAabbMm b)
        {
            return ((a.mi.X != b.mi.X) ||
                (a.mi.Y != b.mi.Y) ||
                (a.mi.Z != b.mi.Z) ||
                (a.mx.X != b.mx.X) ||
                (a.mx.Y != b.mx.Y) ||
                (a.mx.Z != b.mx.Z));
        }
        public void update(DbvtNode leaf, ref DbvtAabbMm volume)
        {
            DbvtNode root = removeleaf(this, leaf);
            if (root != null)
            {
                if (m_lkhd >= 0)
                {
                    for (int i = 0; (i < m_lkhd) && root.parent != null; ++i)
                    {
                        root = root.parent;
                    }
                }
                else root = m_root;
            }
            leaf.volume = volume;
            insertleaf(this, root, leaf);
        }
        public bool	update(DbvtNode leaf,ref DbvtAabbMm volume,ref btVector3 velocity,float margin)
        {
	        if(leaf.volume.Contain(ref volume)) return(false);
            btVector3 temp = new btVector3(margin, margin, margin);
	        volume.Expand(ref temp);
	        volume.SignedExpand(ref velocity);
	        update(leaf,ref volume);
	        return(true);
        }
        public void collideTTpersistentStack(DbvtNode root0, DbvtNode root1, DbvtBroadphase pbp)
        {
            if (root0 != null && root1 != null)
            {
                //int								depth=1;
                //int								treshold=DOUBLE_STACKSIZE-4;
                //中身はただのPush-Pop動作なので、それに置き換え
                m_stkStack.Clear();
                //m_stkStack.AddRange(DOUBLE_STACKSIZE);
                m_stkStack.Add(new sStkNN(root0, root1));
                do
                {
                    //sStkNN	p=m_stkStack[--depth];
                    sStkNN p = m_stkStack[m_stkStack.Count - 1];
                    /*
                    if(depth>treshold)
				    {
					    m_stkStack.resize(m_stkStack.size()*2);
					    treshold=m_stkStack.size()-4;
				    }*/

                    if (p.a == p.b)
                    {
                        if (p.a.isinternal())
                        {
                            /*m_stkStack[depth++]=sStkNN(p.a->childs[0],p.a->childs[0]);
                            m_stkStack[depth++]=sStkNN(p.a->childs[1],p.a->childs[1]);
                            m_stkStack[depth++]=sStkNN(p.a->childs[0],p.a->childs[1]);
                            */
                            m_stkStack.Add(new sStkNN(p.a.childs[0], p.a.childs[0]));
                            m_stkStack.Add(new sStkNN(p.a.childs[1], p.a.childs[1]));
                            m_stkStack.Add(new sStkNN(p.a.childs[0], p.a.childs[1]));
                        }
                    }
                    else if (Intersect(ref p.a.volume,ref p.b.volume))
                    {
                        if (p.a.isinternal())
                        {
                            if (p.b.isinternal())
                            {
                                /*m_stkStack[depth++]=sStkNN(p.a->childs[0],p.b->childs[0]);
                                m_stkStack[depth++]=sStkNN(p.a->childs[1],p.b->childs[0]);
                                m_stkStack[depth++]=sStkNN(p.a->childs[0],p.b->childs[1]);
                                m_stkStack[depth++]=sStkNN(p.a->childs[1],p.b->childs[1]);
                                */
                                m_stkStack.Add(new sStkNN(p.a.childs[0], p.b.childs[0]));
                                m_stkStack.Add(new sStkNN(p.a.childs[1], p.b.childs[0]));
                                m_stkStack.Add(new sStkNN(p.a.childs[0], p.b.childs[1]));
                                m_stkStack.Add(new sStkNN(p.a.childs[1], p.b.childs[1]));
                            }
                            else
                            {
                                /*m_stkStack[depth++]=sStkNN(p.a->childs[0],p.b);
                                m_stkStack[depth++]=sStkNN(p.a->childs[1],p.b);
                                */
                                m_stkStack.Add(new sStkNN(p.a.childs[0], p.b));
                                m_stkStack.Add(new sStkNN(p.a.childs[1], p.b));
                            }
                        }
                        else
                        {
                            if (p.b.isinternal())
                            {
                                /*m_stkStack[depth++] = sStkNN(p.a, p.b->childs[0]);
                                m_stkStack[depth++] = sStkNN(p.a, p.b->childs[1]);
                                m_stkStack[depth++] = sStkNN(p.a, p.b->childs[0]);
                                m_stkStack[depth++] = sStkNN(p.a, p.b->childs[1]);
                                */
                                m_stkStack.Add(new sStkNN(p.a, p.b.childs[0]));
                                m_stkStack.Add(new sStkNN(p.a, p.b.childs[1]));
                                m_stkStack.Add(new sStkNN(p.a, p.b.childs[0]));
                                m_stkStack.Add(new sStkNN(p.a, p.b.childs[1]));
                            }
                            else
                            {
                                //policy.Process(p.a,p.b);
                                DbvtTreeCollider.Process(pbp, p.a, p.b);//stackにclassをnewできないので回避処理……
                            }
                        }
                    }
                } while (m_stkStack.Count > 0);
            }
        }

    }
}
