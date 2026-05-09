using System;
using System.Runtime.InteropServices;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic color struct, replacing Microsoft.Xna.Framework.Color.
    /// Packed as ARGB (BGRA in memory for XNA compat, but accessed via properties).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MMDColor : IEquatable<MMDColor>
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public MMDColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r; G = g; B = b; A = a;
        }

        // ---- Presets ----
        public static readonly MMDColor Transparent = new MMDColor(0, 0, 0, 0);
        public static readonly MMDColor White = new MMDColor(255, 255, 255);
        public static readonly MMDColor Black = new MMDColor(0, 0, 0);

        public static MMDColor FromArgb(int r, int g, int b) =>
            new MMDColor((byte)r, (byte)g, (byte)b);

        // ---- Equality ----
        public bool Equals(MMDColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object obj) => obj is MMDColor other && Equals(other);
        public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;
        public static bool operator ==(MMDColor a, MMDColor b) => a.Equals(b);
        public static bool operator !=(MMDColor a, MMDColor b) => !a.Equals(b);

        public override string ToString() => $"{{R:{R} G:{G} B:{B} A:{A}}}";
    }
}
