using CommonLibrary.CodeSystem.Presenters.Interfaces;
using Microsoft.CSharp;
using ScintillaNET;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLibrary.CodeSystem.Views
{
    public partial class CodeEditer : Form, ICodeEditor
    {
        private const int BACK_COLOR = 0x2A211C;
        private const int FORE_COLOR = 0xB7B7B7;
        private const int NUMBER_MARGIN = 1;
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;
        private const int FOLDING_MARGIN = 3;
        private const bool CODEFOLDING_CIRCULAR = false;

        private Scintilla m_codeArea = null;

        public Scintilla CodeArea => m_codeArea;
        public ToolStripStatusLabel CodePath => toolStripStatusLabelCode;
        public TextBox MessageText => textBoxError;

        public event Action ClearCode;
        public event Action OpenCode;
        public event Action SaveCode;
        public event Action CompileCode;
        public event Action StopCode;
        public event Action PauseCode;
        public event Action StartCode;

        public CodeEditer()
        {
            InitializeComponent();
        }

        private void CodeEdit_Load(object sender, EventArgs e)
        {
            //添加控件
            m_codeArea = new ScintillaNET.Scintilla();
            m_codeArea.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Controls.Add(m_codeArea);

            //初始视图配置
            m_codeArea.WrapMode = WrapMode.None;
            m_codeArea.IndentationGuides = IndentView.LookBoth;

            //配置样式
            InitColors();
            InitSyntaxColoring();

            // NUMBER MARGIN
            InitNumberMargin();

            // BOOKMARK MARGIN
            InitBookmarkMargin();

            // CODE FOLDING MARGIN
            InitCodeFolding();

            // DEFAULT FILE
            LoadDataFromFile(CodeManager.Instance.CodeFullPath);

            // INIT HOTKEYS
            InitHotkeys();

            CodeManager.Instance.OnStatusChange += OnStatusChange;
            SetToolStrip();
            toolStripStatusLabelCode.Text = CodeManager.Instance.CodeFullPath;
        }

        private Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void InitColors()
        {
            m_codeArea.SetSelectionBackColor(true, IntToColor(0x114D9C));//配置选中内容颜色
            m_codeArea.CaretForeColor = Color.White;//设置光标颜色
        }

        private void InitSyntaxColoring()
        {
            //配置默认样式
            m_codeArea.StyleResetDefault();
            m_codeArea.Styles[Style.Default].Font = "Consolas";//字体格式
            m_codeArea.Styles[Style.Default].Size = 15;//字体大小
            m_codeArea.Styles[Style.Default].BackColor = Color.Black;//IntToColor(0x212121);//背景颜色
            m_codeArea.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            m_codeArea.StyleClearAll();

            //配置CPP（C＃）词法分析器样式
            m_codeArea.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            m_codeArea.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            m_codeArea.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            m_codeArea.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            m_codeArea.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
            m_codeArea.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
            m_codeArea.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            m_codeArea.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            m_codeArea.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            m_codeArea.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            m_codeArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            m_codeArea.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            m_codeArea.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            m_codeArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            m_codeArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            m_codeArea.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            m_codeArea.Lexer = Lexer.Cpp;//设置词法分析器类型

            //设置关键字
            m_codeArea.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            m_codeArea.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");
        }

        private void InitNumberMargin()
        {
            m_codeArea.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            m_codeArea.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            m_codeArea.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            m_codeArea.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = m_codeArea.Margins[NUMBER_MARGIN];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = false;
            nums.Mask = 0;
        }

        private void InitBookmarkMargin()
        {
            //TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

            var margin = m_codeArea.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);
            //margin.Cursor = MarginCursor.Arrow;

            var marker = m_codeArea.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);
        }

        private void InitCodeFolding()
        {
            m_codeArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
            m_codeArea.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

            // Enable code folding
            m_codeArea.SetProperty("fold", "1");
            m_codeArea.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            m_codeArea.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            m_codeArea.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            m_codeArea.Margins[FOLDING_MARGIN].Sensitive = true;
            m_codeArea.Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                m_codeArea.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                m_codeArea.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            m_codeArea.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            m_codeArea.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            m_codeArea.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            m_codeArea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            m_codeArea.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            m_codeArea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            m_codeArea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            m_codeArea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
        }

        private void LoadDataFromFile(string path)
        {
            if (File.Exists(path))
            {
                toolStripStatusLabelCode.Text = path;
                m_codeArea.Text = File.ReadAllText(path);
            }
        }

        private void InitHotkeys()
        {
            // remove conflicting hotkeys from scintilla
            m_codeArea.ClearCmdKey(Keys.Control | Keys.F);
            m_codeArea.ClearCmdKey(Keys.Control | Keys.R);
            m_codeArea.ClearCmdKey(Keys.Control | Keys.H);
            m_codeArea.ClearCmdKey(Keys.Control | Keys.L);
            m_codeArea.ClearCmdKey(Keys.Control | Keys.U);
        }

        private void InvokeIfNeeded(Action action)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        private void toolStripButtonCompile_Click(object sender, EventArgs e)
        {
            CompileCode?.Invoke();
        }

        private void toolStripButtonRun_Click(object sender, EventArgs e)
        {
            StartCode?.Invoke();
        }

        private void toolStripButtonPause_Click(object sender, EventArgs e)
        {
            PauseCode?.Invoke();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            StopCode?.Invoke();
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            ClearCode?.Invoke();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            OpenCode?.Invoke();
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            SaveCode?.Invoke();
        }

        private void OnStatusChange()
        {
            Action action = new Action(() =>
            {
                SetToolStrip();
            });
            InvokeIfNeeded(action);
        }

        private void SetToolStrip()
        {
            switch (CodeManager.Instance.Status)
            {
                case CodeStatus.Idle:
                case CodeStatus.AbnormalStop:
                    toolStripButtonRun.Enabled = true;
                    toolStripButtonPause.Enabled = false;
                    toolStripButtonStop.Enabled = false;
                    GC.Collect();
                    break;
                case CodeStatus.Run:
                    toolStripButtonRun.Enabled = false;
                    toolStripButtonPause.Enabled = true;
                    toolStripButtonStop.Enabled = true;
                    break;
                case CodeStatus.Pause:
                    toolStripButtonRun.Enabled = true;
                    toolStripButtonPause.Enabled = false;
                    toolStripButtonStop.Enabled = true;
                    break;
                default:
                    break;
            }
        }

        private void CodeEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            CodeManager.Instance.OnStatusChange -= OnStatusChange;
        }
    }
}
