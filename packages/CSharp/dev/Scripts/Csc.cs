// <copyright file="Csc.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportCscOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalCscOptionsDelegateAttribute : System.Attribute
    {
    }

    // NEW STYLE
#if true
    public sealed class Csc : ICSharpCompilerTool, Opus.Core.IToolForwardedEnvironmentVariables
    {
        private Opus.Core.IToolset toolset;

        public Csc(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string CscPath = null;

            string toolsPath = DotNetFramework.DotNet.ToolsPath;
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                CscPath = System.IO.Path.Combine(toolsPath, "Csc.exe");
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                CscPath = System.IO.Path.Combine(toolsPath, "mono-csc");
            }
            else if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                CscPath = System.IO.Path.Combine(toolsPath, "mcs");
            }

            return CscPath;
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
#else
    [Opus.Core.LocalAndExportTypes(typeof(LocalCscOptionsDelegateAttribute),
                                   typeof(ExportCscOptionsDelegateAttribute))]
    public sealed class Csc : Opus.Core.ITool, Opus.Core.IToolForwardedEnvironmentVariables
    {
        private static string CscPath;
        static Csc()
        {
            string toolsPath = DotNetFramework.DotNet.ToolsPath;
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                CscPath = System.IO.Path.Combine(toolsPath, "Csc.exe");
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                CscPath = System.IO.Path.Combine(toolsPath, "mono-csc");
            }
            else if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                CscPath = System.IO.Path.Combine(toolsPath, "mcs");
            }
        }

        public Csc(Opus.Core.Target target)
        {
            // do nothing
        }

        public string Executable(Opus.Core.Target target)
        {
            return CscPath;
        }

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
    }
#endif
}