#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace CommandLineProcessor
{
namespace V2
{
    public static class Processor
    {
        public static void
        Execute(
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool tool,
            Bam.Core.StringArray commandLine,
            string hostApplication = null,
            string workingDirectory = null)
        {
            var executablePath = tool.Executable.ToString();
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            if (null != hostApplication)
            {
                processStartInfo.Arguments = executablePath + " ";
                executablePath = hostApplication;
            }
            else
            {
                processStartInfo.Arguments = string.Empty;
            }
            processStartInfo.FileName = executablePath;
            processStartInfo.ErrorDialog = true;
            if (null != workingDirectory)
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            var cachedEnvVars = new System.Collections.Generic.Dictionary<string, string>();
            // first get the inherited environment variables from the system environment
            if (null != tool.InheritedEnvironmentVariables)
            {
                foreach (var envVar in tool.InheritedEnvironmentVariables)
                {
                    if (!processStartInfo.EnvironmentVariables.ContainsKey(envVar))
                    {
                        Bam.Core.Log.Info("Environment variable '{0}' does not exist", envVar);
                        continue;
                    }
                    cachedEnvVars.Add(envVar, processStartInfo.EnvironmentVariables[envVar]);
                }
            }

            processStartInfo.EnvironmentVariables.Clear();

            foreach (var envVar in cachedEnvVars)
            {
                processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }
            if (null != tool.EnvironmentVariables)
            {
                foreach (var envVar in tool.EnvironmentVariables)
                {
                    processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value.ToString(System.IO.Path.PathSeparator);
                }
            }

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            processStartInfo.Arguments += commandLine.ToString(' ');

            Bam.Core.Log.Detail("{0} {1}", executablePath, processStartInfo.Arguments);

            // useful debugging of the command line processor
            Bam.Core.Log.DebugMessage("Working directory: '{0}'", processStartInfo.WorkingDirectory);
            if (processStartInfo.EnvironmentVariables.Count > 0)
            {
                Bam.Core.Log.DebugMessage("Environment variables:");
                foreach (string envVar in processStartInfo.EnvironmentVariables.Keys)
                {
                    Bam.Core.Log.DebugMessage("\t{0} = {1}", envVar, processStartInfo.EnvironmentVariables[envVar]);
                }
            }

            System.Diagnostics.Process process = null;
            try
            {
                process = System.Diagnostics.Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new Bam.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
            }
            if (null != process)
            {
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(context.OutputDataReceived);
                process.BeginOutputReadLine();

                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(context.ErrorDataReceived);
                process.BeginErrorReadLine();

                // TODO: need to poll for an external cancel op? this currently waits forever
                process.WaitForExit();
            }

            var exitCode = process.ExitCode;
            //Bam.Core.Log.DebugMessage("Tool exit code: {0}", exitCode);
            process.Close();

            if (exitCode != 0)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Command failed: {0} {1}", executablePath, processStartInfo.Arguments);
                message.AppendLine();
                message.AppendFormat("Command exit code: {0}", exitCode);
                message.AppendLine();
                throw new Bam.Core.Exception(message.ToString());
            }
        }
    }
}
    public static class Processor
    {
        static bool disableResponseFiles = Bam.Core.State.Get<bool>("Build", "DisableResponseFiles", false);

        public static int
        Execute(
            Bam.Core.DependencyNode node,
            Bam.Core.ITool tool,
            Bam.Core.StringArray commandLineBuilder)
        {
            return Execute(node, tool, commandLineBuilder, null, null);
        }

        public static int
        Execute(
            Bam.Core.DependencyNode node,
            Bam.Core.ITool tool,
            Bam.Core.StringArray commandLineBuilder,
            string hostApplication)
        {
            return Execute(node, tool, commandLineBuilder, hostApplication, null);
        }

        public static int
        Execute(
            Bam.Core.DependencyNode node,
            Bam.Core.ITool tool,
            Bam.Core.StringArray commandLineBuilder,
            string hostApplication,
            string workingDirectory)
        {
            var target = node.Target;
            var executablePath = tool.Executable((Bam.Core.BaseTarget)target);

            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            if (null != hostApplication)
            {
                processStartInfo.Arguments = executablePath + " ";
                executablePath = hostApplication;
            }
            else
            {
                processStartInfo.Arguments = string.Empty;
            }
            processStartInfo.FileName = executablePath;
            processStartInfo.ErrorDialog = true;
            if (null != workingDirectory)
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            var requiredEnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>();
            if (tool is Bam.Core.IToolForwardedEnvironmentVariables)
            {
                foreach (var requiredEnvVar in (tool as Bam.Core.IToolForwardedEnvironmentVariables).VariableNames)
                {
                    requiredEnvironmentVariables[requiredEnvVar] = processStartInfo.EnvironmentVariables[requiredEnvVar];
                    //Bam.Core.Log.DebugMessage("Saved envvar '{0}'", requiredEnvVar);
                }
            }

            processStartInfo.EnvironmentVariables.Clear();

            // TODO: change to var? on mono?
            foreach (System.Collections.Generic.KeyValuePair<string, string> requiredEnvVar in requiredEnvironmentVariables)
            {
                processStartInfo.EnvironmentVariables[requiredEnvVar.Key] = requiredEnvVar.Value;
                //Bam.Core.Log.DebugMessage("Restored envvar '{0}' as '{1}'", requiredEnvVar.Key, requiredEnvVar.Value);
            }

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            if (tool is Bam.Core.IToolEnvironmentVariables)
            {
                var variables = (tool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
                foreach (var key in variables.Keys)
                {
                    // values - assume when there are multiple values that they are paths
                    processStartInfo.EnvironmentVariables[key] = variables[key].ToString(System.IO.Path.PathSeparator);
                }
            }

            if ((tool is Bam.Core.IToolSupportsResponseFile) && !disableResponseFiles)
            {
                var module = node.Module;
                var moduleBuildDir = module.Locations[Bam.Core.State.ModuleBuildDirLocationKey];
                var responseFileLoc = new Bam.Core.ScaffoldLocation(
                    moduleBuildDir,
                    node.UniqueModuleName + ".rsp",
                    Bam.Core.ScaffoldLocation.ETypeHint.File,
                    Bam.Core.Location.EExists.WillExist);
                var responseFile = responseFileLoc.GetSinglePath();

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(responseFile))
                {
                    writer.WriteLine(commandLineBuilder.ToString('\n'));
                }

                var responseFileOption = (tool as Bam.Core.IToolSupportsResponseFile).Option;
                processStartInfo.Arguments += System.String.Format("{0}{1}", responseFileOption, responseFile);
            }
            else
            {
                processStartInfo.Arguments += commandLineBuilder.ToString(' ');
            }

            Bam.Core.Log.Detail("{0} {1}", executablePath, processStartInfo.Arguments);

            // useful debugging of the command line processor
            Bam.Core.Log.DebugMessage("Working directory: '{0}'", processStartInfo.WorkingDirectory);
            if (processStartInfo.EnvironmentVariables.Count > 0)
            {
                Bam.Core.Log.DebugMessage("Environment variables:");
                foreach (string envVar in processStartInfo.EnvironmentVariables.Keys)
                {
                    Bam.Core.Log.DebugMessage("\t{0} = {1}", envVar, processStartInfo.EnvironmentVariables[envVar]);
                }
            }

            System.Diagnostics.Process process = null;
            try
            {
                process = System.Diagnostics.Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new Bam.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
            }
            if (null != process)
            {
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(node.OutputDataReceived);
                process.BeginOutputReadLine();

                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(node.ErrorDataReceived);
                process.BeginErrorReadLine();

                // TODO: need to poll for an external cancel op? this currently waits forever
                process.WaitForExit();
                var exitCode = process.ExitCode;
                //Bam.Core.Log.DebugMessage("Tool exit code: {0}", exitCode);

                return exitCode;
            }

            return -1;
        }
    }
}
