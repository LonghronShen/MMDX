using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SlimMMDXDemo4
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
            form.Show();
            using (Demo4 demo = new Demo4(form.pnlMain))
            {
                demo.Run();
            }
        }
    }
}
