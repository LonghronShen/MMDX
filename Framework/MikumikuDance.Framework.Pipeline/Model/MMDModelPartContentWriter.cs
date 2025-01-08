using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDModelPartContentのタイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDModelPartContentWriter : ContentTypeWriter<MMDModelPartContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDModelPartContent value)
        {

            output.Write(value.TriangleCount);
            output.WriteObject(value.Vertices);
            if (output.TargetPlatform == TargetPlatform.Xbox360)
                output.WriteObject(value.extVertices);//XBox360では違う物返す
            else
                output.WriteObject(value.VertMap);
            output.WriteObject(value.IndexCollection);
            output.WriteSharedResource(value.Material);

        }

        /// <summary>
        /// MMDX上での型を指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            //return "MikuMikuDance.XNA.Model.MMDModelPart, MikuMikuDanceXNA";
            var type = typeof(MMDModelPart);
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            Type type;
            // MMDX側でのタイプライターを指定
            switch (targetPlatform)
            {
                case TargetPlatform.Xbox360:
                    type = typeof(MMDXBoxModelPartReader);
                    break;
                case TargetPlatform.Windows:
                default:
                    type = typeof(MMDGPUModelPartReader);
                    break;
            }

            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
