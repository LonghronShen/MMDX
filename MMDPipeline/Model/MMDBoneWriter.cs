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
            return "MikuMikuDance.Core.Model.MMDBone, MikuMikuDanceCore";
        }

        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Model.MMDBoneReader, MikuMikuDanceXNA";
        }
    }
}
