using Unity;
using MikuMikuDance.Framework.Abstractions;
using MikuMikuDance.XNA.Misc;  // XNAAdapterWrappers

namespace MikuMikuDance.Framework.Renderer.Xna
{
    public static class XnaRendererFactory
    {
        public static UnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<IMMDGraphicsDevice, XNAGraphicsDeviceWrapper>(
                new ContainerControlledLifetimeManager());
            container.RegisterType<IMMDVertexBuffer, XNAVertexBufferWrapper>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDIndexBuffer, XNAIndexBufferWrapper>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDRenderTarget, XNARenderTargetWrapper>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDTexture, XNATextureWrapper>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDEffect, XNAEffectWrapper>(
                new TransientLifetimeManager());

            return container;
        }
    }
}
