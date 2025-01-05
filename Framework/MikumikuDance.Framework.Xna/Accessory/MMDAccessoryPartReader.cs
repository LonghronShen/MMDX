using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// 失弁本扔伉由奈汁及戊件氾件玄伉奈母
    /// </summary>
    public class MMDAccessoryPartReader : ContentTypeReader<MMDAccessoryPart>
    {
        /// <summary>
        /// 失弁本扔伉由奈汁及掂心煋心
        /// </summary>
        /// <param name="input">戊件氾件汁伉奈母</param>
        /// <param name="existingInstance">暫湔由奈汁</param>
        /// <returns>失弁本扔伉由奈汁</returns>
        protected override MMDAccessoryPart Read(ContentReader input, MMDAccessoryPart existingInstance)
        {
            int vertexCount = input.ReadInt32();
            IndexBuffer indices = input.ReadObject<IndexBuffer>();
            int baseVertex = input.ReadInt32();
            int triangleCount = input.ReadInt32();
            bool screen = input.ReadBoolean();
            bool edge = input.ReadBoolean();
            MMDAccessoryPart result = new MMDAccessoryPart(vertexCount, indices, baseVertex, triangleCount, screen, edge);
            input.ReadSharedResource<Effect>((effect) => result.Effect = effect);
            input.ReadSharedResource<Effect>((effect) => MMDXCore.Instance.EdgeEffect = effect);
            return result;
        }
    }
}
