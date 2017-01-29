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
    /// MMDIKContentのタイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDIKWriter : ContentTypeWriter<MMDIKContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDIKContent value)
        {
            output.Write(value.IKBoneIndex);
            output.Write(value.IKTargetBoneIndex);
            output.Write(value.Iteration);
            output.Write(value.ControlWeight);
            output.WriteObject(value.IKChildBones);
            
        }
        
        /// <summary>
        /// MMDX上での型を指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.Core.Model.MMDIK, MikuMikuDanceCore";
        }
        
        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Model.MMDIKReader, MikuMikuDanceXNA";
        }
    }
}
