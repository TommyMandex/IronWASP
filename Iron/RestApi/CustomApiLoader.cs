//
// Copyright 2011-2013 Lavakumar Kuppan
//
// This file is part of IronWASP
//
// IronWASP is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// IronWASP is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with IronWASP.  If not, see http://www.gnu.org/licenses/.
//

using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting;
using IronPython;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronRuby;
using IronRuby.Hosting;
using IronRuby.Runtime;
using IronRuby.StandardLibrary;

namespace IronWASP.RestApi
{
    internal class CustomApiLoader
    {
        internal static void LoadCustomApiRegistrationFromPythonScript()
        {
            RunScript( string.Format("{0}\\ApiScript.py", Config.RootDir), GetScriptEngine());
        }

        internal static void LoadCustomApiRegistrationFromRubyScript()
        {
            RunScript(string.Format("{0}\\ApiScript.rb", Config.RootDir), GetScriptEngine());
        }

        static ScriptEngine GetScriptEngine()
        {
            ScriptRuntimeSetup Setup = new ScriptRuntimeSetup();
            Setup.LanguageSetups.Add(IronRuby.Ruby.CreateRubySetup());
            Setup.LanguageSetups.Add(IronPython.Hosting.Python.CreateLanguageSetup(null));
            ScriptRuntime RunTime = new ScriptRuntime(Setup);
            ScriptEngine Engine = RunTime.GetEngine("py");
            ScriptScope Scope = RunTime.CreateScope();

            Assembly MainAssembly = Assembly.GetExecutingAssembly();
            string RootDir = Directory.GetParent(MainAssembly.Location).FullName;

            RunTime.LoadAssembly(MainAssembly);
            RunTime.LoadAssembly(typeof(String).Assembly);
            RunTime.LoadAssembly(typeof(Uri).Assembly);
            RunTime.LoadAssembly(typeof(XmlDocument).Assembly);

            Engine.Runtime.TryGetEngine("py", out Engine);
            return Engine;
        }

        static void RunScript(string ScriptFile, ScriptEngine Engine)
        {
            try
            {
                ScriptSource PluginSource;
                CompiledCode CompiledPlugin;
                ScriptErrorReporter CompileErrors = new ScriptErrorReporter();
                string ErrorMessage = "";

                if (ScriptFile.EndsWith(".py", StringComparison.CurrentCultureIgnoreCase))
                {
                    Engine.Runtime.TryGetEngine("py", out Engine);
                    PluginSource = Engine.CreateScriptSourceFromFile(ScriptFile);
                    string IndentError = PluginEditor.CheckPythonIndentation(PluginSource.GetCode())[1];
                    if (IndentError.Length > 0)
                    {
                        string UpdatedCode = PluginEditor.FixPythonIndentation(PluginSource.GetCode());
                        PluginSource = Engine.CreateScriptSourceFromString(UpdatedCode);
                        //ErrorMessage = string.Format("{0}\r\n{1}", IndentError, ErrorMessage);
                    }
                    CompiledPlugin = PluginSource.Compile(CompileErrors);
                    ErrorMessage = CompileErrors.GetErrors();
                    if (ErrorMessage.Length > 0 && IndentError.Length > 0)
                    {
                        ErrorMessage = string.Format("{0}\r\n{1}", IndentError, ErrorMessage);
                    }
                    if (ErrorMessage.Length == 0)
                    {
                        PluginSource.ExecuteProgram();
                    }
                }
                else if (ScriptFile.EndsWith(".rb", StringComparison.CurrentCultureIgnoreCase))
                {
                    Engine.Runtime.TryGetEngine("rb", out Engine);
                    PluginSource = Engine.CreateScriptSourceFromFile(ScriptFile);
                    CompiledPlugin = PluginSource.Compile(CompileErrors);
                    ErrorMessage = CompileErrors.GetErrors();
                    if (ErrorMessage.Length == 0)
                    {
                        PluginSource.ExecuteProgram();
                    }
                }
                if (ErrorMessage.Length > 0)
                {
                    IronException.Report("Syntax error in API Script - " + ScriptFile, ErrorMessage);
                }
            }
            catch (Exception Exp)
            {
                IronException.Report("Error loading script - " + ScriptFile, Exp.Message, Exp.StackTrace);
            }
        }
    }
}
