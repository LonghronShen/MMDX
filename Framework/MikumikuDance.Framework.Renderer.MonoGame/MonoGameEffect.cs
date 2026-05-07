using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IMMDEffect"/>.
    /// Wraps <see cref="Effect"/> and maps shader parameter setting methods.
    /// </summary>
    public class MonoGameEffect : IMMDEffect
    {
        private readonly Effect _effect;
        private IMMDEffectTechnique _currentTechnique;

        public MonoGameEffect(Effect effect)
        {
            _effect = effect ?? throw new System.ArgumentNullException(nameof(effect));
        }

        /// <summary>
        /// Gets the underlying MonoGame <see cref="Effect"/>.
        /// </summary>
        public Effect NativeEffect => _effect;

        public IMMDEffectTechnique CurrentTechnique
        {
            get => _currentTechnique;
            set
            {
                _currentTechnique = value;
                if (value != null)
                {
                    // Try to select the technique by name
                    foreach (var technique in _effect.Techniques)
                    {
                        if (technique.Name == value.Name)
                        {
                            _effect.CurrentTechnique = technique;
                            return;
                        }
                    }
                }
            }
        }

        public void Apply()
        {
            if (_effect.CurrentTechnique != null)
            {
                _effect.CurrentTechnique.Passes[0].Apply();
            }
        }

        public void SetFloat(string name, float value)
        {
            var param = _effect.Parameters[name];
            if (param != null)
            {
                param.SetValue(value);
            }
        }

        public void SetMatrix(string name, float[] matrix)
        {
            if (matrix == null || matrix.Length < 16)
                throw new System.ArgumentException("Matrix must be a 16-element array.", nameof(matrix));

            var mgMatrix = new Matrix(
                matrix[0], matrix[1], matrix[2], matrix[3],
                matrix[4], matrix[5], matrix[6], matrix[7],
                matrix[8], matrix[9], matrix[10], matrix[11],
                matrix[12], matrix[13], matrix[14], matrix[15]);

            var param = _effect.Parameters[name];
            if (param != null)
            {
                param.SetValue(mgMatrix);
            }
        }

        public void SetTexture(string name, IMMDTexture texture)
        {
            if (texture is MonoGameTexture mgTexture)
            {
                var param = _effect.Parameters[name];
                if (param != null)
                {
                    param.SetValue(mgTexture.NativeTexture);
                }
            }
        }

        public void SetVector(string name, float[] vec)
        {
            if (vec == null || vec.Length < 4)
                throw new System.ArgumentException("Vector must be a 4-element array.", nameof(vec));

            var mgVec = new Vector4(vec[0], vec[1], vec[2], vec[3]);
            var param = _effect.Parameters[name];
            if (param != null)
            {
                param.SetValue(mgVec);
            }
        }
    }
}
