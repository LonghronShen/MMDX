using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MikuMikuDance.XNA.Model;

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
            var type = typeof(MMDAccessoryPart);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// MMDX上でのTypeReader
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDAccessoryPartReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
