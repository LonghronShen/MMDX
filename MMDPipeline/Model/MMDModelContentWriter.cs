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
            return "MikuMikuDance.XNA.Model.MMDXModel, MikuMikuDanceCore";
        }
        
        /// <summary>
        /// MMDX上でのリーダの指定
        /// </summary>
        /// <param name="targetPlatform"></param>
        /// <returns></returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // MMDX側で読み込む型を指定
            return "MikuMikuDance.XNA.Model.MMDModelReader, MikuMikuDanceXNA";
        }
    }
}
