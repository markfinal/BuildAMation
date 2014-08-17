#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
[assembly: Bam.Core.RegisterToolset("dotnet", typeof(DotNetFramework.Toolset))]

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
            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VCSExpress\8.0\Projects"))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("VisualStudio C# Express 2005 was not installed");
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
                throw new Bam.Core.Exception("Unable to locate C# project GUID for VisualStudio 2005");
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2005");
            }
#endif
        }

        public string Header
        {
            get
            {
                var header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 9.00");
                header.AppendLine("# Visual C# Express 2005");
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
                var dotNetPackage = Bam.Core.State.PackageInfo["DotNetFramework"];
                var version = dotNetPackage.Version;
                return version;
            }
        }

        public static string ToolsPath
        {
            get
            {
                if (Bam.Core.OSUtilities.IsWindowsHosting)
                {
                    string toolsPath = null;
                    using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\2.0"))
                    {
                        toolsPath = key.GetValue("MSBuildToolsPath") as string;
                    }

                    return toolsPath;
                }
                else if (Bam.Core.OSUtilities.IsUnixHosting || Bam.Core.OSUtilities.IsOSXHosting)
                {
                    return "/usr/bin";
                }
                else
                {
                    throw new Bam.Core.Exception("DotNetFramework not supported on the current platform");
                }
            }
        }
    }
}
