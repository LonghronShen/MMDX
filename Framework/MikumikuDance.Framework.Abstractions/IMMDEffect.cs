namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Abstraction for an effect (shader).
    /// Provides methods for setting shader parameters and applying passes.
    /// </summary>
    public interface IMMDEffect
    {
        /// <summary>
        /// Gets or sets the current technique.
        /// </summary>
        IMMDEffectTechnique CurrentTechnique { get; set; }

        /// <summary>
        /// Sets a matrix parameter on the effect.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="matrix">16-element array representing a 4x4 matrix in row-major order.</param>
        void SetMatrix(string name, float[] matrix);

        /// <summary>
        /// Sets a vector parameter on the effect.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="vec">4-element array representing the vector.</param>
        void SetVector(string name, float[] vec);

        /// <summary>
        /// Sets a float parameter on the effect.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Float value.</param>
        void SetFloat(string name, float value);

        /// <summary>
        /// Sets a texture parameter on the effect.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="texture">Texture to bind.</param>
        void SetTexture(string name, IMMDTexture texture);

        /// <summary>
        /// Applies the current technique pass.
        /// </summary>
        void Apply();
    }
}
