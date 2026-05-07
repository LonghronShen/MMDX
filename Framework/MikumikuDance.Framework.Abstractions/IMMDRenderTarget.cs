namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for a render target 2D.
    /// </summary>
    public interface IMMDRenderTarget
    {
        /// <summary>
        /// Gets the width of the render target.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the render target.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Saves the render target contents as a PNG image.
        /// </summary>
        /// <param name="path">Output file path.</param>
        /// <param name="width">Desired image width.</param>
        /// <param name="height">Desired image height.</param>
        void SaveAsPng(string path, int width, int height);
    }
}
