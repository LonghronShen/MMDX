using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MikuMikuDance.Core.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MikuMikuDance.XNA.Model
{
    [ContentTypeWriter]
    public class SkinVertSetContentWriter
        : ContentTypeWriter<SkinVertSetContent>
    {
        public SkinVertSetContentWriter()
        {
        }

        protected override void Write(ContentWriter output, SkinVertSetContent value)
        {
            output.Write(value.index);
            output.Write(value.vector);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(SkinVertSet).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(SkinVertSetReader).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }
    }

    [ContentTypeWriter]
    public class SkinVertSetArrayContentWriter
        : ArrayContentWriter<SkinVertSetContent, SkinVertSetContentWriter>
    {
        public SkinVertSetArrayContentWriter()
        {
        }
    }

    public class SkinVertSetDictionaryContentWriter
        : DictionaryContentWriter<string, SkinVertSetContent[], ContentTypeWriter<string>, SkinVertSetArrayContentWriter>
    {
        public SkinVertSetDictionaryContentWriter()
        {
        }
    }
}
