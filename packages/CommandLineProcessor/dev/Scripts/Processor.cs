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

        public static int Execute(Opus.Core.DependencyNode node, Opus.Core.ITool tool, Opus.Core.StringArray commandLineBuilder)
        {
            return Execute (node, tool, commandLineBuilder, null);
        }

        public static int Execute(Opus.Core.DependencyNode node, Opus.Core.ITool tool, Opus.Core.StringArray commandLineBuilder, string hostApplication)
        {
            Opus.Core.Target target = node.Target;
            string executablePath = tool.Executable((Opus.Core.BaseTarget)target);

            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
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

            System.Collections.Generic.Dictionary<string, string> requiredEnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>();
            if (tool is Opus.Core.IToolForwardedEnvironmentVariables)
            {
                foreach (string requiredEnvVar in (tool as Opus.Core.IToolForwardedEnvironmentVariables).VariableNames)
                {
                    requiredEnvironmentVariables[requiredEnvVar] = processStartInfo.EnvironmentVariables[requiredEnvVar];
                    //Opus.Core.Log.DebugMessage("Saved envvar '{0}'", requiredEnvVar);
                }
            }

            processStartInfo.EnvironmentVariables.Clear();

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
                System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> variables = (tool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
                foreach (string key in variables.Keys)
                {
                    // values - assume when there are multiple values that they are paths
                    processStartInfo.EnvironmentVariables[key] = variables[key].ToString(System.IO.Path.PathSeparator);
                }
            }

            if ((tool is Opus.Core.IToolSupportsResponseFile) && !disableResponseFiles)
            {
                string responseFileOption = (tool as Opus.Core.IToolSupportsResponseFile).Option;

                Opus.Core.IModule module = node.Module;
                Opus.Core.BaseOptionCollection options = module.Options;
                string firstOutputPath = options.OutputPaths.Paths[0];
                string responseFile = System.IO.Path.ChangeExtension(firstOutputPath, "rsp");

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(responseFile))
                {
                    writer.WriteLine(commandLineBuilder.ToString('\n'));
                }

                processStartInfo.Arguments += System.String.Format("{0}{1}", responseFileOption, responseFile);
            }
            else
            {
                processStartInfo.Arguments += commandLineBuilder.ToString(' ');
            }

            Opus.Core.Log.Detail("{0} {1}", executablePath, processStartInfo.Arguments);

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
                int exitCode = process.ExitCode;
                //Opus.Core.Log.DebugMessage("Tool exit code: {0}", exitCode);

                return exitCode;
            }

            return -1;
        }
    }
}