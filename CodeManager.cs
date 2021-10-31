using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLibrary.CodeSystem
{
    public class CodeManager
    {
        private static readonly Lazy<CodeManager> m_instance = new Lazy<CodeManager>(() => new CodeManager());

        private static readonly string m_strCodeDirectory = Path.Combine(Application.StartupPath, "CodeSystem");
        private static readonly string m_strHistoryCodeDirectory = Path.Combine(m_strCodeDirectory, "History");
        private static readonly string m_strHistoryCodePath = Path.Combine(m_strHistoryCodeDirectory, "CodePath.txt");
        private static readonly string m_strCodeHeaderPath = Path.Combine(m_strCodeDirectory, "Header.txt");
        private static readonly string m_strCompiledCodePath = Path.Combine(m_strCodeDirectory, "Code.txt");
        private static readonly string m_strCodeReferencePath = Path.Combine(m_strCodeDirectory, "Reference.txt");

        private CompilerResults m_compilerResults;
        private List<string> m_listReference = new List<string>();
        private Thread m_threadCode;
        private string m_strContent = string.Empty;
        private string m_strErrors = string.Empty;
        private CodeStatus m_codeStatus = CodeStatus.Idle;

        public static CodeManager Instance { get => m_instance.Value; }

        public delegate void StatusChangeEventHandler();
        public StatusChangeEventHandler OnStatusChange { get; set; }
        public string CodeFullPath { get; set; }

        public CodeStatus Status
        {
            get { return m_codeStatus; }
            set
            {
                m_codeStatus = value;
                OnStatusChange?.Invoke();
            }
        }

        private CodeManager()
        {
            if (!Directory.Exists(m_strCodeDirectory))
            {
                Directory.CreateDirectory(m_strCodeDirectory);
            }
            if (!Directory.Exists(m_strHistoryCodeDirectory))
            {
                Directory.CreateDirectory(m_strHistoryCodeDirectory);
            }
            if (File.Exists(m_strHistoryCodePath))
            {
                using (StreamReader sr = new StreamReader(m_strHistoryCodePath))
                {
                    CodeFullPath = sr.ReadLine();
                    if (!string.IsNullOrEmpty(CodeFullPath) && File.Exists(CodeFullPath))
                    {
                        m_strContent = File.ReadAllText(CodeFullPath);
                    }
                }
            }
        }

        private string GenerateCode(string content)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetCodeHeader());
            sb.Append(Environment.NewLine);
            sb.Append("namespace DynamicCodeGenerate");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("    public class CodeSystem");
            sb.Append(Environment.NewLine);
            sb.Append("    {");
            sb.Append(Environment.NewLine);
            sb.Append("         public void Main()");
            sb.Append(Environment.NewLine);
            sb.Append("         {");
            sb.Append(Environment.NewLine);
            sb.Append(content);
            sb.Append(Environment.NewLine);
            sb.Append("         }");
            sb.Append(Environment.NewLine);
            sb.Append("     }");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            string code = sb.ToString();
            return code;
        }

        private void GetCodeReference()
        {
            if (File.Exists(m_strCodeReferencePath))
            {
                using (StreamReader sr = new StreamReader(m_strCodeReferencePath))
                {
                    string dlls = sr.ReadToEnd();
                    m_listReference = dlls.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
        }

        private string GetCodeHeader()
        {
            string header = string.Empty;
            if (File.Exists(m_strCodeHeaderPath))
            {
                using (StreamReader sr = new StreamReader(m_strCodeHeaderPath))
                {
                    header = sr.ReadToEnd();
                }
            }
            else
            {
                header = "using System;\r\nusing System.Collections.Generic;\r\nusing System.ComponentModel;\r\nusing System.Data;\r\nusing System.Diagnostics;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Reflection;\r\nusing System.Text;\r\nusing System.Threading;\r\nusing System.Threading.Tasks;\r\nusing System.Windows.Forms;";
            }
            return header;
        }

        private void Run()
        {
            if ((m_compilerResults != null) && (!m_compilerResults.Errors.HasErrors))
            {
                try
                {
                    Assembly assembly = m_compilerResults.CompiledAssembly;
                    object objectInstance = assembly.CreateInstance("DynamicCodeGenerate.CodeSystem");
                    MethodInfo methodInfo = objectInstance.GetType().GetMethod("Main");
                    methodInfo.Invoke(objectInstance, null);
                    Status = CodeStatus.Idle;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\r\n");
                    Status = CodeStatus.AbnormalStop;
                }
            }
            else
            {
                MessageBox.Show("代码编译失败，请检查代码");
            }
        }

        public void LoadData(string content)
        {
            m_strContent = content;
        }

        public bool Compile()
        {
            bool bIsCompileSuccess;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
            ICodeCompiler codeCompiler = cSharpCodeProvider.CreateCompiler();

            CompilerParameters compilerParameters = new CompilerParameters();
            GetCodeReference();
            if (m_listReference.Count != 0)
            {
                for (int i = 0; i < m_listReference.Count; i++)
                {
                    if (m_listReference[i].Contains("System"))
                    {
                        compilerParameters.ReferencedAssemblies.Add(m_listReference[i]);
                    }
                    else
                    {
                        compilerParameters.ReferencedAssemblies.Add(Path.Combine(Application.StartupPath, m_listReference[i]));
                    }
                }
            }
            else
            {
                //标准引用
                compilerParameters.ReferencedAssemblies.Add("System.dll");
                compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
                compilerParameters.ReferencedAssemblies.Add("System.Data.dll");
                compilerParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            }
            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            string code = GenerateCode(m_strContent);
            m_compilerResults = codeCompiler.CompileAssemblyFromSource(compilerParameters, code);
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (m_compilerResults.Errors.HasErrors)
            {
                string error = string.Empty;
                error += "编译错误：\r\n";
                foreach (CompilerError err in m_compilerResults.Errors)
                {
                    error += $"Line{err.Line}:{err.ErrorText }\r\n";
                }
                MessageBox.Show(error);
                m_strErrors = error;
                m_compilerResults = null;
                bIsCompileSuccess = false;
            }
            else
            {
                MessageBox.Show("编译成功");
                m_strErrors = "编译成功";
                bIsCompileSuccess = true;
            }
            using (StreamWriter sw = new StreamWriter(m_strCompiledCodePath))
            {
                sw.WriteLine(code);
            }
            return bIsCompileSuccess;
        }

        public void RunCode()
        {
            if (Status == CodeStatus.Pause)
            {
                Status = CodeStatus.Run;
                m_threadCode.Resume();
            }
            else if (Status == CodeStatus.Idle || Status == CodeStatus.AbnormalStop)
            {
                if ((m_compilerResults == null))
                {
                    if (!Compile())
                        return;
                }

                Status = CodeStatus.Run;
                m_threadCode = new Thread(Run);
                m_threadCode.IsBackground = true;
                m_threadCode.Start();
            }
        }

        public void PauseCode()
        {
            m_threadCode.Suspend();
            Status = CodeStatus.Pause;
        }

        public void StopCode()
        {
            try
            {
                if ((m_threadCode.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
                {
                    m_threadCode.Resume();
                }
                m_threadCode.Abort();
            }
            catch
            {
            }
            Status = CodeStatus.Idle;
        }

        public string GetErrors()
        {
            return m_strErrors;
        }

        public void SaveHistoryCodePath()
        {
            if (!Directory.Exists(m_strHistoryCodeDirectory))
            {
                Directory.CreateDirectory(m_strHistoryCodeDirectory);
            }
            using (StreamWriter sw = new StreamWriter(m_strHistoryCodePath))
            {
                sw.Write(CodeFullPath);
            }
        }
    }
}
