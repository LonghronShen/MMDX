using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDGraphicsDevice"/>.
    /// Wraps <see cref="GraphicsDevice"/> and maps all rendering operations directly.
    /// </summary>
    public class MonoGameGraphicsDevice : IMMDGraphicsDevice
    {
        private readonly GraphicsDevice _device;

        public MonoGameGraphicsDevice(GraphicsDevice device)
        {
            _device = device ?? throw new System.ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Gets the underlying MonoGame <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsDevice NativeDevice => _device;

        public void Clear(float r, float g, float b, float a)
        {
            _device.Clear(new Color(r, g, b, a));
        }

        public IMMDIndexBuffer CreateIndexBuffer(int size, bool is32Bit)
        {
            var indexFormat = is32Bit ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits;
            var buffer = new IndexBuffer(_device, indexFormat, size / (is32Bit ? 4 : 2), BufferUsage.None);
            return new MonoGameIndexBuffer(_device, buffer);
        }

        public IMMDRenderTarget CreateRenderTarget(int width, int height)
        {
            var rt = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            return new MonoGameRenderTarget(rt);
        }

        public IMMDVertexBuffer CreateVertexBuffer(int size, bool dynamic)
        {
            if (dynamic)
            {
                var buffer = new DynamicVertexBuffer(_device, size, BufferUsage.WriteOnly);
                return new MonoGameVertexBuffer(_device, buffer);
            }
            else
            {
                var buffer = new VertexBuffer(_device, size, BufferUsage.None);
                return new MonoGameVertexBuffer(_device, buffer);
            }
        }

        public void DrawIndexedPrimitives(int baseVertex, int minIndex, int numVertices, int startIndex, int indexCount)
        {
            int primitiveCount = indexCount / 3;
            _device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                baseVertex,
                startIndex,
                primitiveCount);
        }

        public void SetIndexBuffer(IMMDIndexBuffer indexBuffer)
        {
            if (indexBuffer is MonoGameIndexBuffer mgIndexBuffer)
            {
                _device.Indices = mgIndexBuffer.NativeBuffer;
            }
        }

        public void SetRenderTarget(IMMDRenderTarget renderTarget)
        {
            if (renderTarget == null)
            {
                _device.SetRenderTarget(null);
                return;
            }

            if (renderTarget is MonoGameRenderTarget mgRt)
            {
                _device.SetRenderTarget(mgRt.NativeTarget);
            }
        }

        public void SetVertexBuffer(IMMDVertexBuffer vertexBuffer)
        {
            if (vertexBuffer is MonoGameVertexBuffer mgVb)
            {
                _device.SetVertexBuffer(mgVb.NativeBuffer);
            }
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            _device.Viewport = new Viewport(x, y, width, height);
        }
    }
}
