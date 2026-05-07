using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDVertexBuffer"/>.
    /// Wraps <see cref="VertexBuffer"/> or <see cref="DynamicVertexBuffer"/>.
    /// </summary>
    public class MonoGameVertexBuffer : IMMDVertexBuffer
    {
        private readonly GraphicsDevice _device;
        private readonly VertexBuffer _buffer;
        private readonly bool _isDynamic;

        internal MonoGameVertexBuffer(GraphicsDevice device, VertexBuffer buffer)
        {
            _device = device ?? throw new System.ArgumentNullException(nameof(device));
            _buffer = buffer ?? throw new System.ArgumentNullException(nameof(buffer));
            _isDynamic = buffer is DynamicVertexBuffer;
        }

        /// <summary>
        /// Gets the underlying MonoGame <see cref="VertexBuffer"/>.
        /// </summary>
        public VertexBuffer NativeBuffer => _buffer;

        public void Bind()
        {
            _device.SetVertexBuffer(_buffer);
        }

        public void SetData<T>(T[] data) where T : struct
        {
            _buffer.SetData(data);
        }
    }
}
