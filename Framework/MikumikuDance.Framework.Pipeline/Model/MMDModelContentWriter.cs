using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDModelContentのTypeWriter
    /// </summary>
    [ContentTypeWriter]
    public class MMDModelContentWriter : ContentTypeWriter<MMDModelContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDModelContent value)
        {
            output.WriteObject(value.ModelParts);
            output.WriteObject(value.BoneManager);
            output.WriteObject(value.FaceManager);
            output.WriteObject(value.AttachedMotionData);
            output.WriteObject(value.Rigids);
            output.WriteObject(value.Joints);
            output.WriteSharedResource(MMDModelContent.EdgeEffect);
        }

        /// <summary>
        /// MMDX上での型の指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDXModel);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// MMDX上でのリーダの指定
        /// </summary>
        /// <param name="targetPlatform"></param>
        /// <returns></returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // MMDX側で読み込む型を指定
            var type = typeof(MMDModelReader);
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
