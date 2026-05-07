namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for the graphics device, wrapping GraphicsDevice operations.
    /// </summary>
    public interface IMMDGraphicsDevice
    {
        /// <summary>
        /// Creates a vertex buffer.
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="dynamic">Whether the buffer is dynamic.</param>
        /// <returns>The vertex buffer abstraction.</returns>
        IMMDVertexBuffer CreateVertexBuffer(int size, bool dynamic);

        /// <summary>
        /// Creates an index buffer.
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="is32Bit">Whether indices are 32-bit.</param>
        /// <returns>The index buffer abstraction.</returns>
        IMMDIndexBuffer CreateIndexBuffer(int size, bool is32Bit);

        /// <summary>
        /// Creates a render target.
        /// </summary>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <returns>The render target abstraction.</returns>
        IMMDRenderTarget CreateRenderTarget(int width, int height);

        /// <summary>
        /// Clears the current render target with the specified color.
        /// </summary>
        void Clear(float r, float g, float b, float a);

        /// <summary>
        /// Sets the active render target.
        /// </summary>
        void SetRenderTarget(IMMDRenderTarget renderTarget);

        /// <summary>
        /// Sets the viewport dimensions.
        /// </summary>
        void SetViewport(int x, int y, int width, int height);

        /// <summary>
        /// Draws indexed primitives.
        /// </summary>
        void DrawIndexedPrimitives(int baseVertex, int minIndex, int numVertices, int startIndex, int indexCount);

        /// <summary>
        /// Binds a vertex buffer.
        /// </summary>
        void SetVertexBuffer(IMMDVertexBuffer vertexBuffer);

        /// <summary>
        /// Binds an index buffer.
        /// </summary>
        void SetIndexBuffer(IMMDIndexBuffer indexBuffer);
    }
}
