using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SlimDX;

namespace SlimMMDXDemo2
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
            using (Demo2 demo = new Demo2(form.pnlDraw))
            {
                demo.Run();
            }
            
            //SlimMMDXの解放処理
            foreach (var item in ObjectTable.Objects)
                item.Dispose();
        }
    }
}
