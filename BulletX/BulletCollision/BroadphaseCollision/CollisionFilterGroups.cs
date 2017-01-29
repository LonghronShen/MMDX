using System;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    [Flags]
    public enum CollisionFilterGroups
    {
        DefaultFilter = 1,
        StaticFilter = 2,
        KinematicFilter = 4,
        DebrisFilter = 8,
        SensorTrigger = 16,
        CharacterFilter = 32,
        AllFilter = -1 //all bits sets: DefaultFilter | StaticFilter | KinematicFilter | DebrisFilter | SensorTrigger
    };
}
