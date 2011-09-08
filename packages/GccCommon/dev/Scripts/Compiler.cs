// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // Not sealed since the C++ compiler inherits from it
    public abstract class CCompiler : C.Compiler, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        public abstract Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get;
        }

        public abstract Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target);

        private static System.Collections.Generic.Dictionary<Opus.Core.Target, string> machineTypesForTarget = new System.Collections.Generic.Dictionary<Opus.Core.Target, string>();

        public string MachineType(Opus.Core.Target target)
        {
            if (!machineTypesForTarget.ContainsKey(target))
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = this.Executable(target);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Arguments = "-dumpmachine";

                System.Diagnostics.Process process = null;
                try
                {
                    process = System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    throw new Opus.Core.Exception(System.String.Format("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName), false);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to execute '{0}'", processStartInfo.FileName), false);
                }

                string machineType = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                machineType = machineType.Trim();
                if (System.String.IsNullOrEmpty(machineType))
                {
                    throw new Opus.Core.Exception("Unable to obtain the machine type from gcc -dumpmachine", false);
                }
                machineTypesForTarget[target] = machineType;

                Opus.Core.Log.DebugMessage("Gcc machine type for target '{0}' is '{1}'", target.ToString(), machineType);
            }

            return machineTypesForTarget[target];
        }
    }
}