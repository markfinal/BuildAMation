// <copyright file="DotNetFramework.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterToolset("dotnet", typeof(DotNetFramework.Toolset))]

namespace DotNetFramework
{
    public class Solution
    {
        private static System.Guid ProjectTypeGuid;
        private static System.Guid SolutionFolderTypeGuid;

        static
        Solution()
        {
            // TODO: this path is for VCSExpress - what about the professional version?
            using (var key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VCSExpress\9.0\Projects"))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("VisualStudio C# Express 2008 was not installed");
                }

                var subKeyNames = key.GetSubKeyNames();
                foreach (var subKeyName in subKeyNames)
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        var projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if (projectExtension == "csproj")
                            {
                                ProjectTypeGuid = new System.Guid(subKeyName);
                                break;
                            }
                        }
                        var defaultValue = subKey.GetValue("") as string;
                        if (null != defaultValue)
                        {
                            if ("Solution Folder Project" == defaultValue)
                            {
                                SolutionFolderTypeGuid = new System.Guid(subKeyName);
                            }
                        }
                    }
                }
            }

            if (0 == ProjectTypeGuid.CompareTo(System.Guid.Empty))
            {
                throw new Opus.Core.Exception("Unable to locate C# project GUID for VisualStudio 2008");
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2008");
            }
#endif
        }

        public string Header
        {
            get
            {
                var header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 10.00");
                header.AppendLine("# Visual C# Express 2008");
                return header.ToString();
            }
        }

        public System.Guid ProjectGuid
        {
            get
            {
                return ProjectTypeGuid;
            }
        }

        public System.Guid SolutionFolderGuid
        {
            get
            {
                return SolutionFolderTypeGuid;
            }
        }

        public string ProjectExtension
        {
            get
            {
                return ".csproj";
            }
        }
    }

    // Define module classes here
    public class DotNet
    {
        public static string VersionString
        {
            get
            {
                var dotNetPackage = Opus.Core.State.PackageInfo["DotNetFramework"];
                var version = dotNetPackage.Version;
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
                    using (var key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\3.5"))
                    {
                        toolsPath = key.GetValue("MSBuildToolsPath") as string;
                    }

                    return toolsPath;
                }
                else if (Opus.Core.OSUtilities.IsUnixHosting || Opus.Core.OSUtilities.IsOSXHosting)
                {
                    return "/usr/bin";
                }
                else
                {
                    throw new Opus.Core.Exception("DotNetFramework not supported on the current platform");
                }
            }
        }
    }
}
