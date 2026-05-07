namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for a 2D texture.
    /// </summary>
    public interface IMMDTexture
    {
        /// <summary>
        /// Binds the texture to the specified texture slot.
        /// </summary>
        /// <param name="slot">The texture sampler slot index.</param>
        void Bind(int slot);
    }
}
