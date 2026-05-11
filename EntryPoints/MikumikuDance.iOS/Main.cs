using Foundation;
using UIKit;

namespace MikumikuDance.iOS
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static Game1 game;

        internal static void RunGame()
        {
            game = new Game1();
            game.Run();
        }

        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            RunGame();
            return true;
        }
    }
}
