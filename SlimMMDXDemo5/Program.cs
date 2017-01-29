using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SlimMMDXDemo5
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
            using (Demo5 demo = new Demo5(form.pnlMain))
            {
                demo.Run();
            }
        }
    }
}
