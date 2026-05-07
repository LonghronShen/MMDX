using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDTexture"/>.
    /// Wraps <see cref="Texture2D"/>.
    /// </summary>
    public class MonoGameTexture : IMMDTexture
    {
        private readonly GraphicsDevice _device;
        private readonly Texture2D _texture;

        internal MonoGameTexture(GraphicsDevice device, Texture2D texture)
        {
            _device = device ?? throw new System.ArgumentNullException(nameof(device));
            _texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
        }

        /// <summary>
        /// Gets the underlying MonoGame <see cref="Texture2D"/>.
        /// </summary>
        public Texture2D NativeTexture => _texture;

        public void Bind(int slot)
        {
            _device.Textures[slot] = _texture;
        }
    }
}
