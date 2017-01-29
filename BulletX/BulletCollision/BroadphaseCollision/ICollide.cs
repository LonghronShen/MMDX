
namespace BulletX.BulletCollision.BroadphaseCollision
{
    public class ICollide
    {
        public virtual void Process(DbvtNode na, DbvtNode nb) { }
        public virtual void Process(DbvtNode n) { }
        public virtual void Process(DbvtNode n, float f) { Process(n); }
        public virtual bool Descent(DbvtNode n) { return (true); }
        public virtual bool AllLeaves(DbvtNode n) { return (true); }
    }
}
