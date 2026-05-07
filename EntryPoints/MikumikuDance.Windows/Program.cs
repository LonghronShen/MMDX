using System;
using Microsoft.Xna.Framework.Content;
using Unity;
using MikumikuDance.Framework.Abstractions;
using MikuMikuDance.Framework.Renderer.Xna;
using MikuMikuDance.XNA;

namespace MikumikuDance.Windows
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Create DI container with XNA backend
            var container = XnaRendererFactory.CreateContainer();

            // Register XnaContentLoader for content loading
            var contentManager = new ContentManager(
                new GameServiceContainer(),
                "Content");
            container.RegisterInstance<IMMDContentLoader>(
                new XnaContentLoader(contentManager));

            // Register MMDXCore as singleton with DI
            container.RegisterType<MMDXCore>(
                new ContainerControlledLifetimeManager());

            // Register Game1
            container.RegisterType<Game1>();

            // Create and run the game
            using (var game = container.Resolve<Game1>())
            {
                // MMDXCore singleton from DI (replaces MMDXCore.Instance)
                var core = container.Resolve<MMDXCore>();

                game.Run();
            }
        }
    }
#endif
}
