using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Model
{
#if XBOX
    class MMDXBoxModelPartFactory : IMMDModelPartFactory
    {
        #region IMMDModelPartFactory メンバー

        public IMMDModelPart Create(int triangleCount, MMDVertexNm[] Vertices, Dictionary<string, object> OpaqueData)
        {
            IndexBuffer indexBuffer = null;
            if (OpaqueData.ContainsKey("IndexBuffer"))
                indexBuffer = OpaqueData["IndexBuffer"] as IndexBuffer;
            Vector2[] extVert = null;
            if (OpaqueData.ContainsKey("VerticesExtention"))
                extVert = OpaqueData["VerticesExtention"] as Vector2[];
            if (indexBuffer == null)
                throw new ArgumentException("MMDModelPartXBoxFactoryのOpaqueDataには\"IndexBuffer\"キーとIndexBufferオブジェクトが必要です。", "OpaqueData");
            if (extVert == null)
                throw new ArgumentException("MMDModelPartXboxFactoryのOpaqueDataには\"VerticesExtention\"キーが必要です。", "OpaqueData");
            if (Vertices is MMDVertexNmTx[])
            {
                if (Vertices is MMDVertexNmTxVc[])
                    return new MMDXBoxModelPartPNmTxVc(triangleCount, (MMDVertexNmTxVc[])Vertices, extVert, indexBuffer);
                else
                    return new MMDXBoxModelPartPNmTx(triangleCount, (MMDVertexNmTx[])Vertices, extVert, indexBuffer);
            }
            else
            {
                if (Vertices is MMDVertexNmVc[])
                    return new MMDXBoxModelPartPNmVc(triangleCount, (MMDVertexNmVc[])Vertices, extVert, indexBuffer);
                else
                    return new MMDXBoxModelPartPNm(triangleCount, (MMDVertexNm[])Vertices, extVert, indexBuffer);
            }
        }

        #endregion
    }
#endif
}
