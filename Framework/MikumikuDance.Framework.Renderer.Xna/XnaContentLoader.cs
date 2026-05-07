using System;
using Microsoft.Xna.Framework.Content;
using MikuMikuDance.Framework.Abstractions;

namespace MikuMikuDance.Framework.Renderer.Xna
{
    /// <summary>
    /// IMMDContentLoader implementation wrapping XNA's ContentManager.
    /// Provides type-safe asset loading for models, textures, and effects.
    /// </summary>
    public class XnaContentLoader : IMMDContentLoader
    {
        private readonly ContentManager _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="XnaContentLoader"/> class.
        /// </summary>
        /// <param name="content">The XNA ContentManager to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
        public XnaContentLoader(ContentManager content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// Loads a model asset through the underlying ContentManager.
        /// </summary>
        /// <typeparam name="T">Model type (must be a class).</typeparam>
        /// <param name="assetName">Asset name/path relative to the ContentManager's root directory.</param>
        /// <returns>The loaded model.</returns>
        public T LoadModel<T>(string assetName) where T : class
        {
            return _content.Load<T>(assetName);
        }

        /// <summary>
        /// Loads a texture asset through the underlying ContentManager.
        /// </summary>
        /// <typeparam name="T">Texture type (must be a class, e.g., <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/>).</typeparam>
        /// <param name="assetName">Asset name/path relative to the ContentManager's root directory.</param>
        /// <returns>The loaded texture.</returns>
        public T LoadTexture<T>(string assetName) where T : class
        {
            return _content.Load<T>(assetName);
        }

        /// <summary>
        /// Loads an effect (shader) asset through the underlying ContentManager.
        /// </summary>
        /// <typeparam name="T">Effect type (must be a class, e.g., <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>).</typeparam>
        /// <param name="assetName">Asset name/path relative to the ContentManager's root directory.</param>
        /// <returns>The loaded effect.</returns>
        public T LoadEffect<T>(string assetName) where T : class
        {
            return _content.Load<T>(assetName);
        }
    }
}
