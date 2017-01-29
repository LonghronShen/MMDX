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


namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// �A�N�Z�T���̃p�[�c��TypeWriter
    /// </summary>
    [ContentTypeWriter]
    public class MMDAccessoryPartWriter : ContentTypeWriter<MMDAccessoryPartContent>
    {
        /// <summary>
        /// TypeWriter�ɂ��o��
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
        /// MMDX��ł̌^
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Accessory.MMDAccessoryPart, MikuMikuDanceXNA";
        }
        /// <summary>
        /// MMDX��ł�TypeReader
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Accessory.MMDAccessoryPartReader, MikuMikuDanceXNA";
        }
    }
}
