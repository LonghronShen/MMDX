using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDance VACライタ
    /// </summary>
    [ContentTypeWriter]
    public class VACWriter : ContentTypeWriter<VACContent>
    {
        /// <summary>
        /// パイプライン書き出し
        /// </summary>
        protected override void Write(ContentWriter output, VACContent value)
        {
            output.Write(value.BoneName);
            output.Write(value.Transform);
        }

        /// <summary>
        /// 読み込み先の型
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDAccessoryReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// 読み出す用のタイプリーダ
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(VACReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
