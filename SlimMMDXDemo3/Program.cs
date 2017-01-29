using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SlimMMDXDemo3
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
            FrmMain frmMain = new FrmMain();
            frmMain.Show();
            using (Demo3 demo3 = new Demo3(frmMain.pnlMain))
            {
                demo3.Run();
            }
        }
    }
}
