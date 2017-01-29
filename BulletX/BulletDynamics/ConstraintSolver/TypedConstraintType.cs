using BulletX.BulletCollision.NarrowPhaseCollision;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public enum TypedConstraintType
    {
        POINT2POINT_CONSTRAINT_TYPE = ContactManifoldTypes.MAX_CONTACT_MANIFOLD_TYPE + 1,
        HINGE_CONSTRAINT_TYPE,
        CONETWIST_CONSTRAINT_TYPE,
        D6_CONSTRAINT_TYPE,
        SLIDER_CONSTRAINT_TYPE,
        CONTACT_CONSTRAINT_TYPE
    }
}