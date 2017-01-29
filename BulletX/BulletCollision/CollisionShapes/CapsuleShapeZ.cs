
namespace BulletX.BulletCollision.CollisionShapes
{
    public class CapsuleShapeZ : CapsuleShape
    {

        public CapsuleShapeZ(float radius, float height)
            :base()
        {
            m_upAxis = 2;
            m_implicitShapeDimensions.setValue(radius, radius, 0.5f * height);
        }
        public override string Name { get { return "CapsuleZ"; } }
    }
}
