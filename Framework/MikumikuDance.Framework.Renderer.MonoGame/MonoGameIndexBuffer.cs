using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDIndexBuffer"/>.
    /// Wraps <see cref="IndexBuffer"/>.
    /// </summary>
    public class MonoGameIndexBuffer : IMMDIndexBuffer
    {
        private readonly GraphicsDevice _device;
        private readonly IndexBuffer _buffer;

        internal MonoGameIndexBuffer(GraphicsDevice device, IndexBuffer buffer)
        {
            _device = device ?? throw new System.ArgumentNullException(nameof(device));
            _buffer = buffer ?? throw new System.ArgumentNullException(nameof(buffer));
        }

        /// <summary>
        /// Gets the underlying MonoGame <see cref="IndexBuffer"/>.
        /// </summary>
        public IndexBuffer NativeBuffer => _buffer;

        public void Bind()
        {
            _device.Indices = _buffer;
        }

        public void SetData<T>(T[] data) where T : struct
        {
            _buffer.SetData(data);
        }
    }
}
