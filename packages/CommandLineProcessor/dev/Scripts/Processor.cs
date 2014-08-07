// <copyright file="Processor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public static class Processor
    {
        static bool disableResponseFiles = Opus.Core.State.Get<bool>("Build", "DisableResponseFiles", false);

        public static int
        Execute(
            Opus.Core.DependencyNode node,
            Opus.Core.ITool tool,
            Opus.Core.StringArray commandLineBuilder)
        {
            return Execute(node, tool, commandLineBuilder, null, null);
        }

        public static int
        Execute(
            Opus.Core.DependencyNode node,
            Opus.Core.ITool tool,
            Opus.Core.StringArray commandLineBuilder,
            string hostApplication)
        {
            return Execute(node, tool, commandLineBuilder, hostApplication, null);
        }

        public static int
        Execute(
            Opus.Core.DependencyNode node,
            Opus.Core.ITool tool,
            Opus.Core.StringArray commandLineBuilder,
            string hostApplication,
            string workingDirectory)
        {
            var target = node.Target;
            var executablePath = tool.Executable((Opus.Core.BaseTarget)target);

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
            if (tool is Opus.Core.IToolForwardedEnvironmentVariables)
            {
                foreach (var requiredEnvVar in (tool as Opus.Core.IToolForwardedEnvironmentVariables).VariableNames)
                {
                    requiredEnvironmentVariables[requiredEnvVar] = processStartInfo.EnvironmentVariables[requiredEnvVar];
                    //Opus.Core.Log.DebugMessage("Saved envvar '{0}'", requiredEnvVar);
                }
            }

            processStartInfo.EnvironmentVariables.Clear();

            // TODO: change to var? on mono?
            foreach (System.Collections.Generic.KeyValuePair<string, string> requiredEnvVar in requiredEnvironmentVariables)
            {
                processStartInfo.EnvironmentVariables[requiredEnvVar.Key] = requiredEnvVar.Value;
                //Opus.Core.Log.DebugMessage("Restored envvar '{0}' as '{1}'", requiredEnvVar.Key, requiredEnvVar.Value);
            }

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            if (tool is Opus.Core.IToolEnvironmentVariables)
            {
                var variables = (tool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
                foreach (var key in variables.Keys)
                {
                    // values - assume when there are multiple values that they are paths
                    processStartInfo.EnvironmentVariables[key] = variables[key].ToString(System.IO.Path.PathSeparator);
                }
            }

            if ((tool is Opus.Core.IToolSupportsResponseFile) && !disableResponseFiles)
            {
                var module = node.Module;
                var moduleBuildDir = module.Locations[Opus.Core.State.ModuleBuildDirLocationKey];
                var responseFileLoc = new Opus.Core.ScaffoldLocation(
                    moduleBuildDir,
                    node.UniqueModuleName + ".rsp",
                    Opus.Core.ScaffoldLocation.ETypeHint.File,
                    Opus.Core.Location.EExists.WillExist);
                var responseFile = responseFileLoc.GetSinglePath();

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(responseFile))
                {
                    writer.WriteLine(commandLineBuilder.ToString('\n'));
                }

                var responseFileOption = (tool as Opus.Core.IToolSupportsResponseFile).Option;
                processStartInfo.Arguments += System.String.Format("{0}{1}", responseFileOption, responseFile);
            }
            else
            {
                processStartInfo.Arguments += commandLineBuilder.ToString(' ');
            }

            Opus.Core.Log.Detail("{0} {1}", executablePath, processStartInfo.Arguments);

            // useful debugging of the command line processor
            Opus.Core.Log.DebugMessage("Working directory: '{0}'", processStartInfo.WorkingDirectory);
            if (processStartInfo.EnvironmentVariables.Count > 0)
            {
                Opus.Core.Log.DebugMessage("Environment variables:");
                foreach (string envVar in processStartInfo.EnvironmentVariables.Keys)
                {
                    Opus.Core.Log.DebugMessage("\t{0} = {1}", envVar, processStartInfo.EnvironmentVariables[envVar]);
                }
            }

            System.Diagnostics.Process process = null;
            try
            {
                process = System.Diagnostics.Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new Opus.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
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
                //Opus.Core.Log.DebugMessage("Tool exit code: {0}", exitCode);

                return exitCode;
            }

            return -1;
        }
    }
}
