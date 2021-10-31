using CommonLibrary.CodeSystem.Presenters.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLibrary.CodeSystem.Presenters
{
    public class CodeEditorPresenter
    {
        private ICodeEditor m_view;

        public CodeEditorPresenter(ICodeEditor view)
        {
            m_view = view;

            m_view.ClearCode += ClearCode;
            m_view.OpenCode += OpenCode;
            m_view.SaveCode += SaveCode;
            m_view.CompileCode += CompileCode;
            m_view.StartCode += StartCode;
            m_view.PauseCode += PauseCode;
            m_view.StopCode += StopCode;
        }

        private void StopCode()
        {
            CodeManager.Instance.StopCode();
        }

        private void PauseCode()
        {
            CodeManager.Instance.PauseCode();
        }

        private void StartCode()
        {
            CodeManager.Instance.RunCode();
        }

        private void CompileCode()
        {
            CodeManager.Instance.LoadData(m_view.CodeArea.Text);
            CodeManager.Instance.Compile();
            m_view.MessageText.Text = CodeManager.Instance.GetErrors();
        }

        private void SaveCode()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "脚本|*.cd";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(m_view.CodeArea.Text);
                        CodeManager.Instance.CodeFullPath = sfd.FileName;
                        CodeManager.Instance.SaveHistoryCodePath();
                    }
                    MessageBox.Show("保存成功");
                }
            }
        }

        private void OpenCode()
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "脚本文件|*.cd";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        LoadDataFromFile(ofd.FileName);
                        CodeManager.Instance.CodeFullPath = ofd.FileName;
                        CodeManager.Instance.SaveHistoryCodePath();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadDataFromFile(string path)
        {
            if (File.Exists(path))
            {
                m_view.CodePath.Text = path;
                m_view.CodeArea.Text = File.ReadAllText(path);
            }
        }

        private void ClearCode()
        {
            m_view.CodeArea.Text = string.Empty;
        }
    }
}
