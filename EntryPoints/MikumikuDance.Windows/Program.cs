using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
            // net40 target: manual DI without Unity
            // (Unity 5.x does not support net40)
            var contentManager = new ContentManager(new GameServiceContainer(), "Content");
            var contentLoader = new XnaContentLoader(contentManager);
            var mmdxCore = MMDXCore.Instance;

            using (var game = new Game1(contentLoader, mmdxCore))
            {
                game.Run();
            }
        }
    }
#endif
}
