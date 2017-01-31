using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{

    public static class VertexContentExtensions
    {

        public static int AddEx(this VertexContent self, int positionIndex)
        {
            var oldVertexCount = self.VertexCount;
            self.Add(positionIndex);
            var newVertexIndex = self.VertexCount;

            // MonoGame bug: issue #5314 not update all channels
            int count = newVertexIndex - oldVertexCount;

            foreach (var channel in self.Channels)
            {
                var type = channel.GetType().GetTypeInfo();
                var method = type.GetMethod("InsertRange", BindingFlags.Instance | BindingFlags.NonPublic);
                var elementArrayType = channel.ElementType.MakeArrayType();
                var elementArray = elementArrayType.InvokeMember("Set", BindingFlags.CreateInstance, null, new object(), new object[] { count });
                method.Invoke(channel, new object[] { oldVertexCount, (IEnumerable)elementArray });
            }

            return oldVertexCount;
        }

    }

}