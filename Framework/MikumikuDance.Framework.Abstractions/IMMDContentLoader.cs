namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for a content manager, providing type-safe asset loading.
    /// </summary>
    public interface IMMDContentLoader
    {
        /// <summary>
        /// Loads a model asset.
        /// </summary>
        /// <typeparam name="T">Model type (must be a class).</typeparam>
        /// <param name="assetName">Asset name/path.</param>
        /// <returns>The loaded model.</returns>
        T LoadModel<T>(string assetName) ;

        /// <summary>
        /// Loads a texture asset.
        /// </summary>
        /// <typeparam name="T">Texture type (must be a class).</typeparam>
        /// <param name="assetName">Asset name/path.</param>
        /// <returns>The loaded texture.</returns>
        T LoadTexture<T>(string assetName) ;

        /// <summary>
        /// Loads an effect (shader) asset.
        /// </summary>
        /// <typeparam name="T">Effect type (must be a class).</typeparam>
        /// <param name="assetName">Asset name/path.</param>
        /// <returns>The loaded effect.</returns>
        T LoadEffect<T>(string assetName) ;
    }
}
