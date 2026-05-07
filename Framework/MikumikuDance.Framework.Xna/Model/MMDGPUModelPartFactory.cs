using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Misc;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.XNA.Model
{
#if WINDOWS
    class MMDGPUModelPartFactory : IMMDModelPartFactory
    {
        //ファクトリー関数
        public IMMDModelPart Create(int triangleCount, MMDVertexNm[] Vertices, Dictionary<string, object> OpaqueData)
        {
            IndexBuffer xnaIndexBuffer = null;
            if (OpaqueData.ContainsKey("IndexBuffer"))
                xnaIndexBuffer = OpaqueData["IndexBuffer"] as IndexBuffer;
            Dictionary<long, int[]> VertMap = OpaqueData["VertMap"] as Dictionary<long, int[]>;
            if (xnaIndexBuffer == null)
                throw new ArgumentException("MMDModelPartGPUFactoryのOpaqueDataには\"IndexBuffer\"キーとIndexBufferオブジェクトが必要です。", "OpaqueData");

            // Wrap the XNA IndexBuffer in the abstraction
            IMMDIndexBuffer indexBuffer = new XNAIndexBufferWrapper(xnaIndexBuffer);
            // XNA GraphicsDevice from the underlying index buffer
            GraphicsDevice xnaDevice = xnaIndexBuffer.GraphicsDevice;
            // Get the abstraction device (may be null if not using DI)
            IMMDGraphicsDevice device = MMDXCore.Instance.GraphicsDevice;
            if (device == null)
            {
                // Fallback: create a wrapper from the XNA device
                device = new XNAGraphicsDeviceWrapper(xnaDevice);
            }

            if (Vertices is MMDVertexNmTx[])
            {
                if (Vertices is MMDVertexNmTxVc[])
                    return new MMDGPUModelPartPNmTxVc(triangleCount, (MMDVertexNmTxVc[])Vertices, VertMap, indexBuffer, device, xnaDevice);
                else
                    return new MMDGPUModelPartPNmTx(triangleCount, (MMDVertexNmTx[])Vertices, VertMap, indexBuffer, device, xnaDevice);
            }
            else
            {
                if (Vertices is MMDVertexNmVc[])
                    return new MMDGPUModelPartPNmVc(triangleCount, (MMDVertexNmVc[])Vertices, VertMap, indexBuffer, device, xnaDevice);
                else
                    return new MMDGPUModelPartPNm(triangleCount, (MMDVertexNm[])Vertices, VertMap, indexBuffer, device, xnaDevice);
            }
        }
    }
#endif
}
