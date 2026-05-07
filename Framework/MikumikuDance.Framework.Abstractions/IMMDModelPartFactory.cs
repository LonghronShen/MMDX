using System.Collections.Generic;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstract factory for creating model parts.
    /// Provides an abstraction over the concrete model part creation logic.
    /// </summary>
    public interface IMMDModelPartFactory
    {
        /// <summary>
        /// Creates a model part.
        /// </summary>
        /// <param name="triangleCount">Number of triangles/polygons.</param>
        /// <param name="vertices">Array of vertex data.</param>
        /// <param name="opaqueData">Opaque data dictionary containing index buffers and other metadata.</param>
        /// <returns>The created model part abstraction.</returns>
        IMMDModelPart CreatePart(int triangleCount, object vertices, Dictionary<string, object> opaqueData);
    }

    /// <summary>
    /// Abstraction for a model part.
    /// </summary>
    public interface IMMDModelPart
    {
        /// <summary>
        /// Sets rendering parameters and draws the model part.
        /// </summary>
        /// <param name="mode">Drawing mode.</param>
        void Draw(int mode);
    }
}
