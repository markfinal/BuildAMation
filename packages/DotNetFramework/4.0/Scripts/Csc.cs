// <copyright file="Csc.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
namespace DotNetFramework
{
    public sealed class Csc : CSharp.ICSharpCompilerTool, Opus.Core.IToolForwardedEnvironmentVariables
    {
        private Opus.Core.IToolset toolset;

        public Csc(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            string CscPath = null;

            string installPath = this.toolset.InstallPath(baseTarget);
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "Csc.exe");
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "mono-csc");
            }
            else if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "mcs");
            }

            return CscPath;
        }

        Opus.Core.Array<Opus.Core.LocationKey> Opus.Core.ITool.OutputLocationKeys
        {
            get
            {
                var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                    CSharp.Assembly.OutputFile,
                    CSharp.Assembly.OutputDirectory
                    );
                return array;
            }
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                Opus.Core.StringArray envVars = new Opus.Core.StringArray();
                envVars.Add("SystemRoot");
                envVars.Add("TEMP"); // otherwise you get errors like this CS0016: Could not write to output file 'd:\build\CSharpTest1-dev\SimpleAssembly\win32-dotnet2.0.50727-debug\bin\SimpleAssembly.dll' -- 'Access is denied.
                return envVars;
            }
        }

        #endregion
    }
}