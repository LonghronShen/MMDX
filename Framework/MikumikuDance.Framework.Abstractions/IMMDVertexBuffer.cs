namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for a vertex buffer.
    /// </summary>
    public interface IMMDVertexBuffer
    {
        /// <summary>
        /// Sets vertex data.
        /// </summary>
        /// <typeparam name="T">Struct type of the vertex data.</typeparam>
        /// <param name="data">Array of vertex data.</param>
        void SetData<T>(T[] data) where T : struct;

        /// <summary>
        /// Binds the vertex buffer for rendering.
        /// </summary>
        void Bind();
    }
}
