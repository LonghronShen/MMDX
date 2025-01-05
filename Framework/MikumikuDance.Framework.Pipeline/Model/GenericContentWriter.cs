using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Reflection;

namespace MikuMikuDance.XNA.Model
{
    public class GenericContentWriter<TContentType, TRuntimeType> : ContentTypeWriter<TContentType>
    {
        public Action<ContentWriter, TContentType> Writer { get; set; }

        public GenericContentWriter(Action<ContentWriter, TContentType> writer = null)
        {
            this.Writer = writer;
        }

        protected override void Write(ContentWriter output, TContentType value)
        {
            if (this.Writer != null)
            {
                this.Writer(output, value);
            }
            else
            {
                output.WriteObject(value);
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(TRuntimeType).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return $"Microsoft.Xna.Framework.Content.ReflectiveReader`1[[{GetRuntimeType(targetPlatform)}]]";
        }
    }
}
