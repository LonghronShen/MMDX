using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDContentLoader"/>.
    /// Wraps <see cref="ContentManager"/> to provide type-safe asset loading.
    /// </summary>
    public class MonoGameContentLoader : IMMDContentLoader
    {
        private readonly ContentManager _content;

        public MonoGameContentLoader(ContentManager content)
        {
            _content = content ?? throw new System.ArgumentNullException(nameof(content));
        }

        public T LoadEffect<T>(string assetName) where T : class
        {
            return _content.Load<T>(assetName);
        }

        public T LoadModel<T>(string assetName) where T : class
        {
            return _content.Load<T>(assetName);
        }

        public T LoadTexture<T>(string assetName) where T : class
        {
            return _content.Load<T>(assetName);
        }
    }
}
