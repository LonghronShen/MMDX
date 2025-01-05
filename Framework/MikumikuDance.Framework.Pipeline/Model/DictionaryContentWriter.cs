using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Collections.Generic;

namespace MikuMikuDance.XNA.Model
{
    public class DictionaryContentWriter<TKey, TValue, TKeyWriter, TValueWriter>
        : ContentTypeWriter<Dictionary<TKey, TValue>>
        where TKeyWriter : ContentTypeWriter<TKey>
        where TValueWriter : ContentTypeWriter<TValue>
    {
        protected ContentTypeWriter<TKey> _keyWriter;
        protected ContentTypeWriter<TValue> _valueWriter;

        public DictionaryContentWriter()
        {
            if (typeof(TKeyWriter).IsAbstract)
            {
                this._keyWriter = null;
            }
            else
            {
                this._keyWriter = Activator.CreateInstance<TKeyWriter>();
            }

            if (typeof(TValueWriter).IsAbstract)
            {
                this._valueWriter = null;
            }
            else
            {
                this._valueWriter = Activator.CreateInstance<TValueWriter>();
            }
        }

        protected override void Write(ContentWriter output, Dictionary<TKey, TValue> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            output.Write(value.Count);
            foreach (var element in value)
            {
                if (this._keyWriter != null)
                {
                    output.WriteRawObject(element.Key, this._keyWriter);
                }
                else
                {
                    output.WriteObject(element.Key);
                }

                if (this._valueWriter != null)
                {
                    output.WriteRawObject(element.Value, this._valueWriter);
                }
                else
                {
                    output.WriteObject(element.Value);
                }
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var keyType = typeof(TKey).AssemblyQualifiedName;
            if (this._keyWriter != null)
            {
                keyType = this._keyWriter.GetRuntimeType(targetPlatform);
            }

            var valueType = typeof(TValue).AssemblyQualifiedName;
            if (this._valueWriter != null)
            {
                valueType = this._valueWriter.GetRuntimeType(targetPlatform);
            }

            return $"{typeof(Dictionary<,>).Namespace}.Dictionary`2[[{keyType}],[{valueType}]]";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var keyType = typeof(TKey).AssemblyQualifiedName;
            if (this._keyWriter != null)
            {
                keyType = this._keyWriter.GetRuntimeType(targetPlatform);
            }

            var valueType = typeof(TValue).AssemblyQualifiedName;
            if (this._valueWriter != null)
            {
                valueType = this._valueWriter.GetRuntimeType(targetPlatform);
            }

            return $"{typeof(ContentTypeReader).Namespace}.DictionaryReader`2[[{keyType}],[{valueType}]]";
        }
    }
}
