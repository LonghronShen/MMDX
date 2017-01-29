
namespace BulletX.LinerMath
{
    static class AabbUtil2
    {
        public static bool TestAabbAgainstAabb2(btVector3 aabbMin1, btVector3 aabbMax1,
								btVector3 aabbMin2, btVector3 aabbMax2)
        {
	        bool overlap = true;
	        overlap = (aabbMin1.X > aabbMax2.X || aabbMax1.X < aabbMin2.X) ? false : overlap;
	        overlap = (aabbMin1.Z > aabbMax2.Z || aabbMax1.Z < aabbMin2.Z) ? false : overlap;
	        overlap = (aabbMin1.Y > aabbMax2.Y || aabbMax1.Y < aabbMin2.Y) ? false : overlap;
	        return overlap;
        }
    }
}
