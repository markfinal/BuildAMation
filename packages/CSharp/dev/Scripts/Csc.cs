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

    public sealed class Csc : Opus.Core.ITool, Opus.Core.IToolRequiredEnvironmentVariables
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

        public Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            Opus.Core.StringArray envPaths = new Opus.Core.StringArray();
            return envPaths;
        }

        public string Executable(Opus.Core.Target target)
        {
            return CscPath;
        }

        Opus.Core.StringArray Opus.Core.IToolRequiredEnvironmentVariables.VariableNames
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
}