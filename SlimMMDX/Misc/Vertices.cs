using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using SlimDX;

namespace MikuMikuDance.SlimDX.Misc
{
    
    [StructLayout(LayoutKind.Sequential)]
    struct VertexPNmTx
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;

        public static VertexElement[] VertexElements = new VertexElement[4]
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal,0),
            new VertexElement(0,24,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,0),
            VertexElement.VertexDeclarationEnd
        };
    }
}
