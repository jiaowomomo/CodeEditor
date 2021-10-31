# CodeEditor
C#代码编辑器，可用于动态编译生成C#执行程序，使软件功能灵活性更高。周末空闲重写了一个编辑器，因此开源最初的视觉软件删减的脚本编辑模块，欢迎交流学习。
# 开发环境
● Visual Studio 2019

● .NET Framework 4.7.2

● ScintillaNET，第三方文本控件
# 文件介绍
\History\CodePath.txt

记录主函数执行内容的保存地址

\CodeSystem\Header.txt

编写引用空间，例如：
using Microsoft.CSharp;
using System;

\CodeSystem\Code.txt

保存编译后生成的整体代码

\CodeSystem\Reference.txt

编写调用的动态库相对路径
