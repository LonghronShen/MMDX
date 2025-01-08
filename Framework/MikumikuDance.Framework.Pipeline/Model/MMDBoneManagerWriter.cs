using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MikuMikuDance.Core.Model;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// BoneManager用タイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDBoneManagerWriter : ContentTypeWriter<MMDBoneManagerContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDBoneManagerContent value)
        {
            output.WriteObject(value.bones);
            output.WriteObject(value.iks);
        }

        /// <summary>
        /// MMDX上での型を指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDBoneManager);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDBoneManagerReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
