#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using System.Linq;
namespace CommandLineProcessor
{
    public static class Processor
    {
        public static string
        StringifyTool(
            Bam.Core.ICommandLineTool tool)
        {
            var linearized = new System.Text.StringBuilder();
            linearized.AppendFormat("{0}", tool.Executable.ToStringQuoteIfNecessary());
            if (tool.InitialArguments != null)
            {
                foreach (var arg in tool.InitialArguments)
                {
                    linearized.AppendFormat(" {0}", arg.ToString());
                }
            }
            return linearized.ToString();
        }

        public static string
        TerminatingArgs(
            Bam.Core.ICommandLineTool tool)
        {
            var linearized = new System.Text.StringBuilder();
            if (null != tool.TerminatingArguments)
            {
                foreach (var arg in tool.TerminatingArguments)
                {
                    linearized.AppendFormat(" {0}", arg.ToString());
                }
            }
            return linearized.ToString();
        }

        public static void
        Execute(
            Bam.Core.ExecutionContext context,
            string executablePath,
            Bam.Core.Array<int> successfulExitCodes,
            Bam.Core.TokenizedStringArray commandLineArguments)
        {
            var arguments = new Bam.Core.StringArray();
            foreach (var arg in commandLineArguments)
            {
                arguments.Add(arg.ToString());
            }
            Execute(context, executablePath, successfulExitCodes, commandLineArguments: arguments);
        }

        public static void
        Execute(
            Bam.Core.ExecutionContext context,
            string executablePath,
            Bam.Core.Array<int> successfulExitCodes,
            Bam.Core.StringArray commandLineArguments = null,
            string workingDirectory = null,
            Bam.Core.StringArray inheritedEnvironmentVariables = null,
            System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> addedEnvironmentVariables = null,
            string useResponseFileOption = null)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = executablePath;
            processStartInfo.ErrorDialog = true;
            if (null != workingDirectory)
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            var cachedEnvVars = new System.Collections.Generic.Dictionary<string, string>();
            // first get the inherited environment variables from the system environment
            if (null != inheritedEnvironmentVariables)
            {
                int envVarCount;
                if (inheritedEnvironmentVariables.Count == 1 &&
                    inheritedEnvironmentVariables[0] == "*")
                {
                    foreach (System.Collections.DictionaryEntry envVar in processStartInfo.EnvironmentVariables)
                    {
                        cachedEnvVars.Add(envVar.Key as string, envVar.Value as string);
                    }
                }
                else if (inheritedEnvironmentVariables.Count == 1 &&
                         System.Int32.TryParse(inheritedEnvironmentVariables[0], out envVarCount) &&
                         envVarCount < 0)
                {
                    envVarCount += processStartInfo.EnvironmentVariables.Count;
                    foreach (var envVar in processStartInfo.EnvironmentVariables.Cast<System.Collections.DictionaryEntry>().OrderBy(item => item.Key))
                    {
                        cachedEnvVars.Add(envVar.Key as string, envVar.Value as string);
                        --envVarCount;
                        if (0 == envVarCount)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var envVar in inheritedEnvironmentVariables)
                    {
                        if (!processStartInfo.EnvironmentVariables.ContainsKey(envVar))
                        {
                            Bam.Core.Log.Info("Environment variable '{0}' does not exist", envVar);
                            continue;
                        }
                        cachedEnvVars.Add(envVar, processStartInfo.EnvironmentVariables[envVar]);
                    }
                }
            }
            processStartInfo.EnvironmentVariables.Clear();
            if (null != inheritedEnvironmentVariables)
            {
                foreach (var envVar in cachedEnvVars)
                {
                    processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
                }
            }
            if (null != addedEnvironmentVariables)
            {
                foreach (var envVar in addedEnvironmentVariables)
                {
                    processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value.ToString(System.IO.Path.PathSeparator);
                }
            }

            processStartInfo.UseShellExecute = false; // to redirect IO streams
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;

            var arguments = commandLineArguments != null ? commandLineArguments.ToString(' ') : string.Empty;
            string responseFilePath = null;
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                //TODO: should this include the length of the executable path too?
                if (arguments.Length >= 32767)
                {
                    if (null == useResponseFileOption)
                    {
                        throw new Bam.Core.Exception("Command line is {0} characters long, but response files are not supported by the tool {1}", arguments.Length, executablePath);
                    }

                    responseFilePath = Bam.Core.IOWrapper.CreateTemporaryFile();
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(responseFilePath))
                    {
                        Bam.Core.Log.DebugMessage("Written response file {0} containing:\n{1}", responseFilePath, arguments);
                        // escape any back slashes
                        writer.WriteLine(arguments.Replace(@"\", @"\\"));
                    }

                    arguments = System.String.Format("{0}{1}", useResponseFileOption, responseFilePath);
                }
            }
            processStartInfo.Arguments = arguments;

            Bam.Core.Log.Detail("{0} {1}", processStartInfo.FileName, processStartInfo.Arguments);

            // useful debugging of the command line processor
            Bam.Core.Log.DebugMessage("Working directory: '{0}'", processStartInfo.WorkingDirectory);
            if (processStartInfo.EnvironmentVariables.Count > 0)
            {
                Bam.Core.Log.DebugMessage("Environment variables:");
                foreach (var envVar in processStartInfo.EnvironmentVariables.Cast<System.Collections.DictionaryEntry>().OrderBy(item => item.Key))
                {
                    Bam.Core.Log.DebugMessage("\t{0} = {1}", envVar.Key, envVar.Value);
                }
            }

            System.Diagnostics.Process process = null;
            int exitCode = -1;
            try
            {
                process = new System.Diagnostics.Process();
                process.StartInfo = processStartInfo;
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(context.OutputDataReceived);
                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(context.ErrorDataReceived);
                process.Start();
                process.StandardInput.Close();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new Bam.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
            }

            if (null != process)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                // TODO: need to poll for an external cancel op?
                // poll for the process to exit, as some processes seem to get stuck (hdiutil attach, for example)
                while (!process.HasExited)
                {
                    process.WaitForExit(2000);
                }
                // this additional WaitForExit appears to be needed in order to finish reading the output and error streams asynchronously
                // without it, output is missing from a Native build when executed in many threads
                process.WaitForExit();

                exitCode = process.ExitCode;
                //Bam.Core.Log.DebugMessage("Tool exit code: {0}", exitCode);
                process.Close();
            }

            // delete once the process has finished, or never started
            if (null != responseFilePath)
            {
                System.IO.File.Delete(responseFilePath);
            }

            if (!successfulExitCodes.Contains(exitCode))
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Command failed: {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
                message.AppendLine();
                if (null != responseFilePath)
                {
                    message.AppendLine("Response file contained:");
                    message.AppendLine(arguments);
                }
                message.AppendFormat("Command exit code: {0}", exitCode);
                message.AppendLine();
                throw new Bam.Core.Exception(message.ToString());
            }
        }

#if false
        // TODO: deprecate this one, as you can't get the module override WorkingDirectory
        public static void
        Execute(
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool tool,
            Bam.Core.StringArray commandLine,
            string workingDirectory = null)
        {
            var commandLineArgs = new Bam.Core.StringArray();
            if (tool.InitialArguments != null)
            {
                foreach (var arg in tool.InitialArguments)
                {
                    commandLineArgs.Add(arg.ToString());
                }
            }
            commandLineArgs.AddRange(commandLine);
            if (null != tool.TerminatingArguments)
            {
                foreach (var arg in tool.TerminatingArguments)
                {
                    commandLineArgs.Add(arg.ToString());
                }
            }

            Execute(
                context,
                tool.Executable.ToString(),
                tool.SuccessfulExitCodes,
                commandLineArgs,
                workingDirectory: workingDirectory,
                inheritedEnvironmentVariables: tool.InheritedEnvironmentVariables,
                addedEnvironmentVariables: tool.EnvironmentVariables,
                useResponseFileOption: tool.UseResponseFileOption);
        }
#endif

        public static void
        Execute(
            Bam.Core.Module module,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool tool,
            Bam.Core.StringArray commandLine,
            string workingDirectory = null)
        {
            if (null == tool)
            {
                throw new Bam.Core.Exception(
                    "Command line tool passed with module '{0}' is invalid",
                    module.ToString()
                );
            }
            var commandLineArgs = new Bam.Core.StringArray();
            if (tool.InitialArguments != null)
            {
                foreach (var arg in tool.InitialArguments)
                {
                    commandLineArgs.Add(arg.ToString());
                }
            }
            commandLineArgs.AddRange(commandLine);
            if (null != tool.TerminatingArguments)
            {
                foreach (var arg in tool.TerminatingArguments)
                {
                    commandLineArgs.Add(arg.ToString());
                }
            }

            Execute(
                context,
                tool.Executable.ToString(),
                tool.SuccessfulExitCodes,
                commandLineArgs,
                workingDirectory: (module.WorkingDirectory != null) ? module.WorkingDirectory.ToString() : null,
                inheritedEnvironmentVariables: tool.InheritedEnvironmentVariables,
                addedEnvironmentVariables: tool.EnvironmentVariables,
                useResponseFileOption: tool.UseResponseFileOption);
        }

    }
}
