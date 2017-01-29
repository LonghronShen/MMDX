using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

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
            return "MikuMikuDance.XNA.Model.MMDModelPart, MikuMikuDanceXNA";
        }
        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // MMDX側でのタイプライターを指定
            switch (targetPlatform)
            {
                case TargetPlatform.Windows:
                    return "MikuMikuDance.XNA.Model.MMDGPUModelPartReader, MikuMikuDanceXNA";
                case TargetPlatform.Xbox360:
                    return "MikuMikuDance.XNA.Model.MMDXBoxModelPartReader, MikuMikuDanceXNA";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
