// <copyright file="DotNetFramework.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("CSharp", "dotnet", "DotNetFramework.DotNet.VersionString")]

namespace DotNetFramework
{
    // Define module classes here
    public class DotNet
    {
        public static string VersionString
        {
            get
            {
                Opus.Core.PackageInformation dotNetPackage = Opus.Core.State.PackageInfo["DotNetFramework"];
                string version = dotNetPackage.Version;
                return version;
            }
        }

        public static string ToolsPath
        {
            get
            {
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    string toolsPath = null;
                    using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\2.0"))
                    {
                        toolsPath = key.GetValue("MSBuildToolsPath") as string;
                    }

                    return toolsPath;
                }
                else if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return "/usr/bin";
                }
                else
                {
                    throw new Opus.Core.Exception("DotNetFramework not supported on platforms other than Windows");
                }
            }
        }
    }
}
