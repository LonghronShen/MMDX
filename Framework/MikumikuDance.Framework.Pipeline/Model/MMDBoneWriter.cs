using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MikuMikuDance.Core.Model;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDBoneのタイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDBoneWriter : ContentTypeWriter<MMDBoneContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDBoneContent value)
        {
            output.WriteObject(value.BindPose);
            output.WriteObject(value.InverseBindPose);
            //output.Write(value.IKParentBoneIndex);
            output.Write(value.Name);
            output.Write(value.SkeletonHierarchy);
        }

        /// <summary>
        /// MMDX上での型を指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDBone);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDBoneReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
