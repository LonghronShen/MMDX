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
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Motion;
using MikuMikuDance.Core.Motion;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDModelContentのTypeWriter
    /// </summary>
    [ContentTypeWriter]
    public class MMDMotionContentWriter : ContentTypeWriter<MMDMotionContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDMotionContent value)
        {
            output.WriteDictionary(value.BoneFrames);
            output.WriteDictionary(value.FaceFrames);
            output.WriteObject(value.CameraFrames);
            output.WriteObject(value.LightFrames);
        }

        /// <summary>
        /// MMDX上での型の指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(MMDMotion).GetTypeInfo();
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
            //return "MikuMikuDance.XNA.Model.MMDModelReader, MikuMikuDanceXNA";

            var type = typeof(MMDMotionReader).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }
}
