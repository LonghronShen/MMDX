using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDRenderTarget"/>.
    /// Wraps <see cref="RenderTarget2D"/>.
    /// </summary>
    public class MonoGameRenderTarget : IMMDRenderTarget
    {
        private readonly RenderTarget2D _target;

        internal MonoGameRenderTarget(RenderTarget2D target)
        {
            _target = target ?? throw new System.ArgumentNullException(nameof(target));
        }

        /// <summary>
        /// Gets the underlying MonoGame <see cref="RenderTarget2D"/>.
        /// </summary>
        public RenderTarget2D NativeTarget => _target;

        public int Width => _target.Width;

        public int Height => _target.Height;

        public void SaveAsPng(string path, int width, int height)
        {
            using (var stream = File.OpenWrite(path))
            {
                _target.SaveAsPng(stream, width, height);
            }
        }
    }
}
