using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.XNA.Model
{
#if WINDOWS
    class MMDGPUModelPartFactory : IMMDModelPartFactory
    {
        //ファクトリー関数
        public IMMDModelPart Create(int triangleCount, MMDVertexNm[] Vertices, Dictionary<string, object> OpaqueData)
        {
            IndexBuffer indexBuffer = null;
            if (OpaqueData.ContainsKey("IndexBuffer"))
                indexBuffer = OpaqueData["IndexBuffer"] as IndexBuffer;
            Dictionary<long, int[]> VertMap = OpaqueData["VertMap"] as Dictionary<long, int[]>;
            if (indexBuffer == null)
                throw new ArgumentException("MMDModelPartGPUFactoryのOpaqueDataには\"IndexBuffer\"キーとIndexBufferオブジェクトが必要です。", "OpaqueData");
            if (Vertices is MMDVertexNmTx[])
            {
                if (Vertices is MMDVertexNmTxVc[])
                    return new MMDGPUModelPartPNmTxVc(triangleCount, (MMDVertexNmTxVc[])Vertices, VertMap, indexBuffer);
                else
                    return new MMDGPUModelPartPNmTx(triangleCount, (MMDVertexNmTx[])Vertices, VertMap, indexBuffer);
            }
            else
            {
                if (Vertices is MMDVertexNmVc[])
                    return new MMDGPUModelPartPNmVc(triangleCount, (MMDVertexNmVc[])Vertices, VertMap, indexBuffer);
                else
                    return new MMDGPUModelPartPNm(triangleCount, (MMDVertexNm[])Vertices, VertMap, indexBuffer);
            }
        }
    }
#endif
}
