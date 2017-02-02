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
    /// FaceManager用タイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDFaceManagerWriter : ContentTypeWriter<MMDFaceManagerContent>
    {
        /// <summary>
        /// FaceManagerの書き出し
        /// </summary>
        protected override void Write(ContentWriter output, MMDFaceManagerContent value)
        {
            if (output.TargetPlatform == TargetPlatform.Xbox360)
            {
                output.WriteObject(value.vertDataXBox);
                output.WriteObject(value.faceDict);
            }
            else
            {
                output.WriteObject(value.vertData);
            }
        }
        /// <summary>
        /// 読み込み時の型を指定
        /// </summary>
        /// <param name="targetPlatform"></param>
        /// <returns></returns>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            //return "MikuMikuDance.Core.Model.MMDFaceManager, MikuMikuDanceCore";
            var type = typeof(MMDFaceManager).GetTypeInfo();
            return $"{type.FullName}, {type.Assembly.FullName}";
        }
        /// <summary>
        /// 読み込み時のタイプライターを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            TypeInfo type = null;
            if (targetPlatform == TargetPlatform.Xbox360)
            {
                //return "MikuMikuDance.XNA.Model.MMDXBoxFaceManagerReader, MikuMikuDanceXNA";
                type = typeof(MMDXBoxFaceManagerReader).GetTypeInfo();
                return $"{type.FullName}, {type.Assembly.FullName}";
            }
            //return "MikuMikuDance.XNA.Model.MMDFaceManagerReader, MikuMikuDanceXNA";
            type = typeof(MMDFaceManagerReader).GetTypeInfo();
            return $"{type.FullName}, {type.Assembly.FullName}";
        }
    }
}
