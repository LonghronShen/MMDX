﻿
namespace BulletX.BulletCollision.BroadphaseCollision
{
    public enum BroadphaseNativeTypes
    {
        //順番変更禁止
        // polyhedral convex shapes
        BOX_SHAPE_PROXYTYPE,
        TRIANGLE_SHAPE_PROXYTYPE,
        TETRAHEDRAL_SHAPE_PROXYTYPE,
        CONVEX_TRIANGLEMESH_SHAPE_PROXYTYPE,
        CONVEX_HULL_SHAPE_PROXYTYPE,
        CONVEX_POINT_CLOUD_SHAPE_PROXYTYPE,
        CUSTOM_POLYHEDRAL_SHAPE_TYPE,
        //implicit convex shapes
        IMPLICIT_CONVEX_SHAPES_START_HERE,
        SPHERE_SHAPE_PROXYTYPE,
        MULTI_SPHERE_SHAPE_PROXYTYPE,
        CAPSULE_SHAPE_PROXYTYPE,
        CONE_SHAPE_PROXYTYPE,
        CONVEX_SHAPE_PROXYTYPE,
        CYLINDER_SHAPE_PROXYTYPE,
        UNIFORM_SCALING_SHAPE_PROXYTYPE,
        MINKOWSKI_SUM_SHAPE_PROXYTYPE,
        MINKOWSKI_DIFFERENCE_SHAPE_PROXYTYPE,
        BOX_2D_SHAPE_PROXYTYPE,
        CONVEX_2D_SHAPE_PROXYTYPE,
        CUSTOM_CONVEX_SHAPE_TYPE,
        //concave shapes
        CONCAVE_SHAPES_START_HERE,
        //keep all the convex shapetype below here, for the check IsConvexShape in broadphase proxy!
        TRIANGLE_MESH_SHAPE_PROXYTYPE,
        SCALED_TRIANGLE_MESH_SHAPE_PROXYTYPE,
        ///used for demo integration FAST/Swift collision library and Bullet
        FAST_CONCAVE_MESH_PROXYTYPE,
        //terrain
        TERRAIN_SHAPE_PROXYTYPE,
        ///Used for GIMPACT Trimesh integration
        GIMPACT_SHAPE_PROXYTYPE,
        ///Multimaterial mesh
        MULTIMATERIAL_TRIANGLE_MESH_PROXYTYPE,

        EMPTY_SHAPE_PROXYTYPE,
        STATIC_PLANE_PROXYTYPE,
        CUSTOM_CONCAVE_SHAPE_TYPE,
        CONCAVE_SHAPES_END_HERE,

        COMPOUND_SHAPE_PROXYTYPE,

        SOFTBODY_SHAPE_PROXYTYPE,
        HFFLUID_SHAPE_PROXYTYPE,
        HFFLUID_BUOYANT_CONVEX_SHAPE_PROXYTYPE,
        INVALID_SHAPE_PROXYTYPE,

        MAX_BROADPHASE_COLLISION_TYPES

    }
}
