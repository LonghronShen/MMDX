using System;

namespace BulletX.BulletCollision.CollisionDispatch
{
    [Flags]
    public enum CollisionFlags
    {
        CF_STATIC_OBJECT = 1,
        CF_KINEMATIC_OBJECT = 2,
        CF_NO_CONTACT_RESPONSE = 4,
        CF_CUSTOM_MATERIAL_CALLBACK = 8,//this allows per-triangle material (friction/restitution)
        CF_CHARACTER_OBJECT = 16,
        CF_DISABLE_VISUALIZE_OBJECT = 32, //disable debug drawing
        CF_DISABLE_SPU_COLLISION_PROCESSING = 64//disable parallel/SPU processing
    }
    
}
