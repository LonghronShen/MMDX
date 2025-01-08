using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDance の　アクセサリタイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDAccessoryWriter : ContentTypeWriter<MMDAccessoryContent>
    {
        /// <summary>
        /// アクセサリの書き出し
        /// </summary>
        protected override void Write(ContentWriter output, MMDAccessoryContent value)
        {
            output.WriteObject(value.Vertex);
            output.WriteObject(value.Parts);
        }

        /// <summary>
        /// MMDX上での型
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDAccessory);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// MMDX上でのリーダ
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDAccessoryReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
