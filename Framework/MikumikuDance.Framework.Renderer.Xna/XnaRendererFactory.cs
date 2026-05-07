using MikuMikuDance.Framework.Abstractions;
using MikuMikuDance.XNA.Misc;  // XNAAdapterWrappers
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.Framework.Renderer.Xna
{
    /// <summary>
    /// Simple DI container registration for XNA (net40) backend.
    /// Since Unity 5.x does not support net40, this provides manual factory methods.
    /// </summary>
    public static class XnaRendererFactory
    {
        /// <summary>
        /// Creates a GraphicsDevice wrapper. Must be called with a valid GraphicsDevice.
        /// </summary>
        public static XNAGraphicsDeviceWrapper CreateGraphicsDevice(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
            => new XNAGraphicsDeviceWrapper(device);

        /// <summary>
        /// Creates a ContentLoader wrapping the given ContentManager.
        /// </summary>
        public static XnaContentLoader CreateContentLoader(ContentManager content)
            => new XnaContentLoader(content);

        /// <summary>
        /// Registers all XNA-backed abstractions into a simple dictionary-based service registry.
        /// This replaces Unity DI for the net40 target where Unity packages are unavailable.
        /// </summary>
        public static void RegisterServices(ServiceRegistry registry, Microsoft.Xna.Framework.Graphics.GraphicsDevice device, ContentManager content)
        {
            registry.Register<IMMDGraphicsDevice>(new XNAGraphicsDeviceWrapper(device));
            registry.Register<IMMDContentLoader>(new XnaContentLoader(content));
            // Other types (vertex buffers, index buffers, etc.) are created on-demand
        }
    }

    /// <summary>
    /// Minimal service registry for net40 target (Unity-free DI).
    /// </summary>
    public class ServiceRegistry
    {
        private readonly System.Collections.Generic.Dictionary<System.Type, object> _services = new System.Collections.Generic.Dictionary<System.Type, object>();

        public void Register<T>(object instance)
        {
            _services[typeof(T)] = instance;
        }

        public T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var instance))
                return (T)instance;
            return null;
        }
    }
}
