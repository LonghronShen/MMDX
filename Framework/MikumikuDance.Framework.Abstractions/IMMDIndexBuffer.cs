namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for an index buffer.
    /// </summary>
    public interface IMMDIndexBuffer
    {
        /// <summary>
        /// Sets index data.
        /// </summary>
        /// <typeparam name="T">Struct type of the index data (typically int or short).</typeparam>
        /// <param name="data">Array of index data.</param>
        void SetData<T>(T[] data) where T : struct;

        /// <summary>
        /// Binds the index buffer for rendering.
        /// </summary>
        void Bind();
    }
}
