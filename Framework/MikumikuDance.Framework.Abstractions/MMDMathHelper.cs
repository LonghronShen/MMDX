using System;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic math helper, replacing core functions from Microsoft.Xna.Framework.MathHelper.
    /// </summary>
    public static class MMDMathHelper
    {
        public const float Pi = (float)Math.PI;
        public const float PiOver2 = (float)(Math.PI / 2.0);
        public const float PiOver4 = (float)(Math.PI / 4.0);
        public const float TwoPi = (float)(Math.PI * 2.0);
        public const float E = (float)Math.E;

        /// <summary>Clamp value to [min, max] range.</summary>
        public static float Clamp(float value, float min, float max)
        {
            if (min > value) return min;
            if (max < value) return max;
            return value;
        }

        /// <summary>Linear interpolation.</summary>
        public static float Lerp(float start, float end, float factor)
        {
            return start + (end - start) * factor;
        }

        /// <summary>Degrees to radians.</summary>
        public static float ToRadians(float degrees)
        {
            return degrees * Pi / 180.0f;
        }

        /// <summary>Radians to degrees.</summary>
        public static float ToDegrees(float radians)
        {
            return radians * 180.0f / Pi;
        }

        /// <summary>Distance between two values.</summary>
        public static float Distance(float a, float b)
        {
            return Math.Abs(a - b);
        }

        /// <summary>Smooth interpolation (Hermite).</summary>
        public static float SmoothStep(float a, float b, float t)
        {
            t = Clamp(t, 0, 1);
            t = t * t * (3 - 2 * t);
            return a + (b - a) * t;
        }
    }
}
