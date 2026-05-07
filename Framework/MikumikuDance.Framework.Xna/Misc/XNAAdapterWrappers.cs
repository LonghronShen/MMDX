using System;
using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// IMMDIndexBuffer wrapper around XNA IndexBuffer.
    /// </summary>
    public class XNAIndexBufferWrapper : IMMDIndexBuffer
    {
        public IndexBuffer InnerBuffer { get; private set; }

        public XNAIndexBufferWrapper(IndexBuffer buffer)
        {
            InnerBuffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void SetData<T>(T[] data) where T : struct
        {
            InnerBuffer.SetData(data);
        }

        public void Bind()
        {
            // Binding is done via GraphicsDevice.Indices = InnerBuffer in the device wrapper
        }
    }

    /// <summary>
    /// IMMDVertexBuffer wrapper around DynamicVertexBuffer (used for writable/skinned vertex data).
    /// </summary>
    public class XNAVertexBufferWrapper : IMMDVertexBuffer
    {
        public DynamicVertexBuffer InnerBuffer { get; private set; }

        public XNAVertexBufferWrapper(DynamicVertexBuffer buffer)
        {
            InnerBuffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void SetData<T>(T[] data) where T : struct
        {
            InnerBuffer.SetData(data);
        }

        public void Bind()
        {
            // Binding is done via GraphicsDevice.SetVertexBuffer in the device wrapper
        }
    }

    /// <summary>
    /// IMMDEffect wrapper around XNA Effect.
    /// </summary>
    public class XNAEffectWrapper : IMMDEffect
    {
        public Effect InnerEffect { get; private set; }
        private IMMDEffectTechnique currentTechnique;

        public XNAEffectWrapper(Effect effect)
        {
            InnerEffect = effect ?? throw new ArgumentNullException(nameof(effect));
        }

        public IMMDEffectTechnique CurrentTechnique
        {
            get { return currentTechnique; }
            set { currentTechnique = value; }
        }

        public void SetMatrix(string name, float[] matrix)
        {
            var param = InnerEffect.Parameters[name];
            if (param != null)
            {
                // Convert float[16] to XNA Matrix
                var m = new Microsoft.Xna.Framework.Matrix(
                    matrix[0], matrix[1], matrix[2], matrix[3],
                    matrix[4], matrix[5], matrix[6], matrix[7],
                    matrix[8], matrix[9], matrix[10], matrix[11],
                    matrix[12], matrix[13], matrix[14], matrix[15]);
                param.SetValue(m);
            }
        }

        public void SetVector(string name, float[] vec)
        {
            var param = InnerEffect.Parameters[name];
            if (param != null)
            {
                var v = new Microsoft.Xna.Framework.Vector4(vec[0], vec[1], vec[2], vec[3]);
                param.SetValue(v);
            }
        }

        public void SetFloat(string name, float value)
        {
            var param = InnerEffect.Parameters[name];
            if (param != null)
            {
                param.SetValue(value);
            }
        }

        public void SetTexture(string name, IMMDTexture texture)
        {
            var param = InnerEffect.Parameters[name];
            if (param != null)
            {
                // Try to extract the underlying XNA Texture
                if (texture is XNATextureWrapper texWrapper)
                {
                    param.SetValue(texWrapper.InnerTexture);
                }
            }
        }

        public void Apply()
        {
            foreach (EffectPass pass in InnerEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
        }
    }

    /// <summary>
    /// IMMDTexture wrapper around XNA Texture2D.
    /// </summary>
    public class XNATextureWrapper : IMMDTexture
    {
        public Texture2D InnerTexture { get; private set; }

        public XNATextureWrapper(Texture2D texture)
        {
            InnerTexture = texture ?? throw new ArgumentNullException(nameof(texture));
        }

        public void Bind(int slot)
        {
            // Binding is handled via effect parameters in XNA
        }
    }

    /// <summary>
    /// IMMDRenderTarget wrapper around XNA RenderTarget2D.
    /// </summary>
    public class XNARenderTargetWrapper : IMMDRenderTarget
    {
        public RenderTarget2D InnerTarget { get; private set; }

        public XNARenderTargetWrapper(RenderTarget2D target)
        {
            InnerTarget = target ?? throw new ArgumentNullException(nameof(target));
        }

        public int Width
        {
            get { return InnerTarget.Width; }
        }

        public int Height
        {
            get { return InnerTarget.Height; }
        }

        public void SaveAsPng(string path, int width, int height)
        {
            using (var stream = System.IO.File.Create(path))
                InnerTarget.SaveAsPng(stream, width, height);
        }
    }

    /// <summary>
    /// IMMDGraphicsDevice wrapper around XNA GraphicsDevice.
    /// </summary>
    public class XNAGraphicsDeviceWrapper : IMMDGraphicsDevice
    {
        public GraphicsDevice InnerDevice { get; private set; }

        public XNAGraphicsDeviceWrapper(GraphicsDevice device)
        {
            InnerDevice = device ?? throw new ArgumentNullException(nameof(device));
        }

        public IMMDVertexBuffer CreateVertexBuffer(int size, bool dynamic)
        {
            // Since we need DynamicVertexBuffer for writable vertex buffers, we create one
            // The caller must ensure the vertex type is set correctly externally.
            // We use a generic vertex declaration placeholder - actual type is set in the subclasses.
            var vb = new DynamicVertexBuffer(InnerDevice, typeof(Microsoft.Xna.Framework.Graphics.VertexPositionNormal), size / 12, BufferUsage.None);
            return new XNAVertexBufferWrapper(vb);
        }

        public IMMDIndexBuffer CreateIndexBuffer(int size, bool is32Bit)
        {
            IndexElementSize elementSize = is32Bit ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits;
            var ib = new IndexBuffer(InnerDevice, elementSize, size / (is32Bit ? 4 : 2), BufferUsage.None);
            return new XNAIndexBufferWrapper(ib);
        }

        public IMMDRenderTarget CreateRenderTarget(int width, int height)
        {
            var rt = new RenderTarget2D(InnerDevice, width, height);
            return new XNARenderTargetWrapper(rt);
        }

        public void Clear(float r, float g, float b, float a)
        {
            InnerDevice.Clear(new Microsoft.Xna.Framework.Color(r, g, b, a));
        }

        public void SetRenderTarget(IMMDRenderTarget renderTarget)
        {
            if (renderTarget is XNARenderTargetWrapper rtWrapper)
                InnerDevice.SetRenderTarget(rtWrapper.InnerTarget);
            else
                InnerDevice.SetRenderTarget(null);
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            InnerDevice.Viewport = new Viewport(x, y, width, height);
        }

        public void DrawIndexedPrimitives(int baseVertex, int minIndex, int numVertices, int startIndex, int indexCount)
        {
            // IMMDGraphicsDevice.DrawIndexedPrimitives の indexCount は実際には三角形数 (primitiveCount)
            InnerDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVertex, minIndex, numVertices, startIndex, indexCount);
        }

        public void SetVertexBuffer(IMMDVertexBuffer vertexBuffer)
        {
            if (vertexBuffer is XNAVertexBufferWrapper vbWrapper)
                InnerDevice.SetVertexBuffer(vbWrapper.InnerBuffer);
        }

        public void SetIndexBuffer(IMMDIndexBuffer indexBuffer)
        {
            if (indexBuffer is XNAIndexBufferWrapper ibWrapper)
                InnerDevice.Indices = ibWrapper.InnerBuffer;
        }
    }
}
