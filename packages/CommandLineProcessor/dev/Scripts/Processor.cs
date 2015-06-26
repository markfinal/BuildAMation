#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace CommandLineProcessor
{
namespace V2
{
    public static class Processor
    {
        public static int
        Execute(
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.Tool tool,
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
            foreach (var envVar in tool.InheritedEnvironmentVariables)
            {
                if (!processStartInfo.EnvironmentVariables.ContainsKey(envVar))
                {
                    Bam.Core.Log.Info("Environment variable '{0}' does not exist", envVar);
                    continue;
                }
                cachedEnvVars.Add(envVar, processStartInfo.EnvironmentVariables[envVar]);
            }

            processStartInfo.EnvironmentVariables.Clear();

            foreach (var envVar in cachedEnvVars)
            {
                processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }
            foreach (var envVar in tool.EnvironmentVariables)
            {
                processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value.ToString(System.IO.Path.PathSeparator);
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
                var exitCode = process.ExitCode;
                //Bam.Core.Log.DebugMessage("Tool exit code: {0}", exitCode);

                return exitCode;
            }

            return -1;
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
