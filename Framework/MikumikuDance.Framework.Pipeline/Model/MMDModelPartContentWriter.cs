using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Reflection;

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
            var type = typeof(MMDModelPart).GetTypeInfo();
            return $"{type.FullName}, {type.Assembly.FullName}";
        }
        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            TypeInfo type = null;
            // MMDX側でのタイプライターを指定
            switch (targetPlatform)
            {
                case TargetPlatform.Xbox360:
                    //return "MikuMikuDance.XNA.Model.MMDXBoxModelPartReader, MikuMikuDanceXNA";
                    type = typeof(MMDXBoxModelPartReader).GetTypeInfo();
                    return $"{type.FullName}, {type.Assembly.FullName}";
                //default:
                //    throw new NotImplementedException();
                case TargetPlatform.Windows:
                default:
                    //return "MikuMikuDance.XNA.Model.MMDGPUModelPartReader, MikuMikuDanceXNA";
                    type = typeof(MMDGPUModelPartReader).GetTypeInfo();
                    return $"{type.FullName}, {type.Assembly.FullName}";
            }
        }
    }
}
