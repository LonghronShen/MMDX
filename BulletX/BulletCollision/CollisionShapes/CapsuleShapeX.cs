
namespace BulletX.BulletCollision.CollisionShapes
{
    public class CapsuleShapeX : CapsuleShape
    {
        public CapsuleShapeX(float radius, float height)
        {
            m_upAxis = 0;
            m_implicitShapeDimensions.setValue(0.5f * height, radius, radius);
        }
		
	    //debugging
        public override string Name { get { return "CapsuleX"; } }
	
    }
}