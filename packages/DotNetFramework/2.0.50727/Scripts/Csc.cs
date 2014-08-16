// <copyright file="Csc.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
namespace DotNetFramework
{
    public sealed class Csc :
        CSharp.ICSharpCompilerTool,
        Bam.Core.IToolForwardedEnvironmentVariables
    {
        private Bam.Core.IToolset toolset;

        public
        Csc(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            string CscPath = null;

            var installPath = this.toolset.InstallPath(baseTarget);
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "Csc.exe");
            }
            else if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "mono-csc");
            }
            else if (Bam.Core.OSUtilities.IsOSXHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "mcs");
            }

            return CscPath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                CSharp.Assembly.OutputFile,
                CSharp.Assembly.OutputDir,
                CSharp.Assembly.PDBFile,
                CSharp.Assembly.PDBDir
                );
            return array;
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Bam.Core.StringArray Bam.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                var envVars = new Bam.Core.StringArray();
                envVars.Add("SystemRoot");
                envVars.Add("TEMP"); // otherwise you get errors like this CS0016: Could not write to output file 'd:\build\CSharpTest1-dev\SimpleAssembly\win32-dotnet2.0.50727-debug\bin\SimpleAssembly.dll' -- 'Access is denied.
                return envVars;
            }
        }

        #endregion
    }
}
