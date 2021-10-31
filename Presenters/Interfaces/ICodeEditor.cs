using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLibrary.CodeSystem.Presenters.Interfaces
{
    public interface ICodeEditor
    {
        Scintilla CodeArea { get; }
        ToolStripStatusLabel CodePath { get; }
        TextBox MessageText { get; }

        event Action ClearCode;
        event Action OpenCode;
        event Action SaveCode;
        event Action CompileCode;
        event Action StopCode;
        event Action PauseCode;
        event Action StartCode;
    }
}
