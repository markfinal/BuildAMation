// <copyright file="DotNetFramework.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("CSharp", "dotnet", "DotNetFramework.DotNet.VersionString")]

namespace DotNetFramework
{
    public class Solution
    {
        private static System.Guid ProjectTypeGuid;
        private static System.Guid SolutionFolderTypeGuid;

        static Solution()
        {
            // TODO: this path is for VCSExpress - what about the professional version?
            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\VCSExpress\10.0\Projects"))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("VisualStudio C# Express 2010 was not installed");
                }

                string[] subKeyNames = key.GetSubKeyNames();
                foreach (string subKeyName in subKeyNames)
                {
                    using (Microsoft.Win32.RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        string projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if (projectExtension == "csproj")
                            {
                                ProjectTypeGuid = new System.Guid(subKeyName);
                                break;
                            }
                        }
                        string defaultValue = subKey.GetValue("") as string;
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
                throw new Opus.Core.Exception("Unable to locate C# project GUID for VisualStudio 2010");
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2010");
            }
#endif
        }

        public string Header
        {
            get
            {
                System.Text.StringBuilder header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 11.00");
                header.AppendLine("# Visual C# Express 2010");
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
        static DotNet()
        {
            if (!Opus.Core.State.HasCategory("VSSolutionBuilder"))
            {
                Opus.Core.State.AddCategory("VSSolutionBuilder");
                Opus.Core.State.Add<System.Type>("VSSolutionBuilder", "SolutionType", typeof(DotNetFramework.Solution));
            }
        }

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
                    using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\4.0"))
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
