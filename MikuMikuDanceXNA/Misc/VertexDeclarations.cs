using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace MikuMikuDance.XNA.Misc
{
    [StructLayout(LayoutKind.Sequential)]
    struct VertexPositionNormal : IVertexType
    {

        public Vector3 Position;
        public Vector3 Normal;
        public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
        public VertexDeclaration VertexDeclaration { get { return vertexDeclaration; } }
    }
    [StructLayout(LayoutKind.Sequential)]
    struct VertexPositionNormalColor : IVertexType
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
        public VertexDeclaration VertexDeclaration { get { return vertexDeclaration; } }
    }
    [StructLayout(LayoutKind.Sequential)]
    struct VertexPositionNormalTextureColor : IVertexType
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Color Color;
        public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
        public VertexDeclaration VertexDeclaration { get { return vertexDeclaration; } }
    }
    [StructLayout(LayoutKind.Sequential)]
    struct VertexXBoxExtend : IVertexType
    {
        public Vector2 FacePtr;
        public Vector2 BlendIndices;
        public Vector2 BlendWeight;
        public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.BlendIndices, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector2, VertexElementUsage.BlendWeight, 0)
        );
        public VertexDeclaration VertexDeclaration { get { return vertexDeclaration; } }
    }
    [StructLayout(LayoutKind.Sequential)]
    struct VertexSkinning : IVertexType
    {
        public Quaternion Rotation;
        public Vector3 Translation;
        public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 3)
        );
        public VertexDeclaration VertexDeclaration { get { return vertexDeclaration; } }
    }
    [StructLayout(LayoutKind.Sequential)]
    struct VertexFaceVert : IVertexType
    {
        public Vector4 FaceData;
        public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4)
        );
        public VertexDeclaration VertexDeclaration { get { return vertexDeclaration; } }
    }

}
