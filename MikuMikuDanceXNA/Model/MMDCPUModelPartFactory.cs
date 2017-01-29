using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.XNA.Model
{
#if false//WP7対応時に復活
    class MMDModelPartCPUFactory : IMMDModelPartFactory
    {
        //ファクトリー関数
        public IMMDModelPart Create(int triangleCount, MMDVertex[] Vertices, Dictionary<string, object> OpaqueData)
        {
            IndexBuffer indexBuffer = null;
            if (OpaqueData.ContainsKey("IndexBuffer"))
                indexBuffer = OpaqueData["IndexBuffer"] as IndexBuffer;
            int[] VertMap = OpaqueData["VertMap"] as int[];
            if (indexBuffer == null)
                throw new ArgumentException("MMDModelPartFactoryのOpaqueDataには\"IndexBuffer\"キーとIndexBufferオブジェクトが必要です。", "OpaqueData");
            if (Vertices is MMDVertexNm[])
            {
                if (Vertices is MMDVertexNmTx[])
                {
                    if (Vertices is MMDVertexNmTxVc[])
                        return new MMDCPUModelPartPNmTxVc(triangleCount, (MMDVertexNmTxVc[])Vertices, VertMap, indexBuffer);
                    else
                        return new MMDCPUModelPartPNmTx(triangleCount, (MMDVertexNmTx[])Vertices, VertMap, indexBuffer);
                }
                else
                {
                    if (Vertices is MMDVertexNmVc[])
                        return new MMDCPUModelPartPNmVc(triangleCount, (MMDVertexNmVc[])Vertices, VertMap, indexBuffer);
                    else
                        return new MMDCPUModelPartPNm(triangleCount, (MMDVertexNm[])Vertices, VertMap, indexBuffer);
                }
            }
            else
            {
                if (Vertices is MMDVertexTx[])
                {
                    if (Vertices is MMDVertexTxVc[])
                        return new MMDCPUModelPartPTxVc(triangleCount, (MMDVertexTxVc[])Vertices, VertMap, indexBuffer);
                    else
                        return new MMDCPUModelPartPTx(triangleCount, (MMDVertexTx[])Vertices, VertMap, indexBuffer);
                }
                else
                {
                    if (Vertices is MMDVertexVc[])
                        return new MMDCPUModelPartPVc(triangleCount, (MMDVertexVc[])Vertices, VertMap, indexBuffer);
                    else
                        return new MMDCPUModelPartP(triangleCount, Vertices, VertMap, indexBuffer);
                }
            }
        }
    }
#endif
}
