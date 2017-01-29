using System;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class DbvtNode
    {

        public DbvtAabbMm volume;
        public DbvtNode parent;
        public bool isleaf() { return (childs[1] == null); }
        public bool isinternal() { return (!isleaf()); }

        //これは元ではunionになっているので、このクラスを作って回避
        public struct DbvtNodeUnion
        {
            object data0;
            DbvtNode data1;
            public DbvtNode this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return (DbvtNode)data0;
                        case 1:
                            return data1;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            data0 = value;
                            return;
                        case 1:
                            data1 = value;
                            return;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
            }
            public object Data
            {
                get { return data0; }
                set { data0 = value; }
            }
            /*public int DataAsInt
            {
                get { return (int)data0; }
                set { data0 = value; }
            }*/
        }
        public DbvtNodeUnion childs;
        public object data { get { return childs.Data; } set { childs.Data = value; } }
        //public int dataAsInt { get { return childs.DataAsInt; } set { childs.DataAsInt = value; } }



    }
}
