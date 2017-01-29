
namespace BulletX.BulletCollision.CollisionShapes
{
    public abstract class ConcaveShape : CollisionShape
    {
        float m_collisionMargin;
        public ConcaveShape()
        {
            m_collisionMargin = 0f;
        }
#if false
        virtual void	processAllTriangles(btTriangleCallback* callback,const btVector3& aabbMin,const btVector3& aabbMax) const = 0;
#endif
        public override float Margin { get { return m_collisionMargin; } set { m_collisionMargin = value; } }
        
        
    }
}
