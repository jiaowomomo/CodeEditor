using CommonLibrary.CodeSystem.Presenters;
using CommonLibrary.CodeSystem.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLibrary.CodeSystem
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CodeEditer codeEditer = new CodeEditer();
            CodeEditorPresenter codeEditorPresenter = new CodeEditorPresenter(codeEditer);
            Application.Run(codeEditer);
        }
    }
}
