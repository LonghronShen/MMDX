using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MikuMikuDance.XNA.Model;
using System.Reflection;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// アクセサリのパーツのTypeWriter
    /// </summary>
    [ContentTypeWriter]
    public class MMDAccessoryPartWriter : ContentTypeWriter<MMDAccessoryPartContent>
    {
        /// <summary>
        /// TypeWriterによる出力
        /// </summary>
        protected override void Write(ContentWriter output, MMDAccessoryPartContent value)
        {
            output.Write(value.VertexCount);
            output.WriteObject(value.IndexBuffer);
            output.Write(value.BaseVertex);
            output.Write(value.TriangleCount);
            output.Write(value.Screen);
            output.Write(value.Edge);
            output.WriteSharedResource(value.Material);
            output.WriteSharedResource(MMDModelContent.EdgeEffect);
        }
        /// <summary>
        /// MMDX上での型
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            //return "MikuMikuDance.XNA.Accessory.MMDAccessoryPart, MikuMikuDanceXNA";
            var type = typeof(MMDAccessoryPart).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
        /// <summary>
        /// MMDX上でのTypeReader
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            //return "MikuMikuDance.XNA.Accessory.MMDAccessoryPartReader, MikuMikuDanceXNA";
            var type = typeof(MMDAccessoryPartReader).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
