using Unity;
using MikumikuDance.Framework.Abstractions;

namespace MikumikuDance.Framework.Renderer.MonoGame
{
    /// <summary>
    /// Factory class for creating a Unity DI container pre-configured
    /// with MonoGame implementations of all abstraction interfaces.
    /// </summary>
    public static class MonoGameRendererFactory
    {
        public static UnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<IMMDGraphicsDevice, MonoGameGraphicsDevice>(
                new ContainerControlledLifetimeManager());
            container.RegisterType<IMMDVertexBuffer, MonoGameVertexBuffer>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDIndexBuffer, MonoGameIndexBuffer>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDRenderTarget, MonoGameRenderTarget>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDTexture, MonoGameTexture>(
                new TransientLifetimeManager());
            container.RegisterType<IMMDEffect, MonoGameEffect>(
                new TransientLifetimeManager());

            return container;
        }
    }
}
