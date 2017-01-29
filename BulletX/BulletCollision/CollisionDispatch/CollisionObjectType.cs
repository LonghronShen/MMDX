
namespace BulletX.BulletCollision.CollisionDispatch
{
    public enum CollisionObjectTypes
    {
        CO_COLLISION_OBJECT = 1,
        CO_RIGID_BODY,
        ///CO_GHOST_OBJECT keeps track of all objects overlapping its AABB and that pass its collision filter
        ///It is useful for collision sensors, explosion objects, character controller etc.
        CO_GHOST_OBJECT,
        CO_SOFT_BODY,
        CO_HF_FLUID
    }
}
