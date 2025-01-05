using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Reflection;

namespace MikuMikuDance.XNA.Model
{
    public class ArrayContentWriter<T, TElementWriter>
        : ContentTypeWriter<T[]> where TElementWriter : ContentTypeWriter<T>
    {
        protected ContentTypeWriter<T> _elementWriter;

        protected ArrayContentWriter()
        {
            this._elementWriter = Activator.CreateInstance<TElementWriter>();
        }

        protected override void Write(ContentWriter output, T[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            output.Write(value.Length);

            foreach (var element in value)
            {
                output.WriteObject(element);
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(T[]).GetTypeInfo();
            return $"{type.Namespace}.{type.Name}, {type.Assembly.GetName().Name}";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return $"{typeof(ContentTypeReader).Namespace}.ArrayReader`1[[{_elementWriter.GetRuntimeType(targetPlatform)}]]";
        }
    }
}
