#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VCSExpress\10.0\Projects"))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("VisualStudio C# Express 2010 was not installed");
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
                throw new Bam.Core.Exception("Unable to locate C# project GUID for VisualStudio 2010");
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2010");
            }
#endif
        }

        public string Header
        {
            get
            {
                var header = new System.Text.StringBuilder();
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
                    using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\4.0"))
                    {
                        toolsPath = key.GetValue("MSBuildToolsPath") as string;
                    }

                    return toolsPath;
                }
                else if (Bam.Core.OSUtilities.IsLinuxHosting || Bam.Core.OSUtilities.IsOSXHosting)
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
