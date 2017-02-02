using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MikuMikuDance.Core.Model;
using System.Reflection;

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
            //return "MikuMikuDance.Core.Model.MMDIK, MikuMikuDanceCore";
            var type = typeof(MMDIK).GetTypeInfo();
            return $"{type.FullName}, {type.Assembly.FullName}";
        }
        
        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            //return "MikuMikuDance.XNA.Model.MMDIKReader, MikuMikuDanceXNA";
            var type = typeof(MMDIKReader).GetTypeInfo();
            return $"{type.FullName}, {type.Assembly.FullName}";
        }
    }
}
