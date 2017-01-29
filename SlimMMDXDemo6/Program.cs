using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SlimMMDXDemo6
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FrmMain form = new FrmMain();
            using (Demo6 demo = new Demo6(form.pnlMain))
            {
                demo.Run();
            }
        }
    }
}
