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
[assembly: Bam.Core.RegisterToolset("visualc", typeof(VisualC.Toolset))]

namespace VisualC
{
    // Define module classes here
    public class Solution
    {
        private static System.Guid ProjectTypeGuid;
        private static System.Guid SolutionFolderTypeGuid;
        private static string vsEdition;

        static
        Solution()
        {
            // try the VS Express version first, since it's free
            string registryKey = @"Microsoft\WDExpress\12.0_Config\Projects";
            if (Bam.Core.Win32RegistryUtilities.DoesCUSoftwareKeyExist(registryKey))
            {
                vsEdition = "Express";
            }
            else
            {
                registryKey = @"Microsoft\WDStudio\12.0\Projects";
                if (Bam.Core.Win32RegistryUtilities.DoesCUSoftwareKeyExist(registryKey))
                {
                    vsEdition = "Professional";
                }
                else
                {
                    throw new Bam.Core.Exception("VisualStudio C++ 2013 (Express or Professional) was not installed or not yet run for the current user");
                }
            }

            using (Microsoft.Win32.RegistryKey key = Bam.Core.Win32RegistryUtilities.OpenCUSoftwareKey(registryKey))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("VisualStudio C++ {0} 2013 was not installed", vsEdition);
                }

                string[] subKeyNames = key.GetSubKeyNames();
                foreach (string subKeyName in subKeyNames)
                {
                    using (Microsoft.Win32.RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        string projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if (projectExtension == "vcxproj")
                            {
                                ProjectTypeGuid = new System.Guid(subKeyName);
                                break;
                            }
                        }
                    }
                }

                SolutionFolderTypeGuid = new System.Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");
            }

            if (0 == ProjectTypeGuid.CompareTo(System.Guid.Empty))
            {
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2013 {0}", vsEdition);
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2013");
            }
#endif
        }

        public string Header
        {
            get
            {
                System.Text.StringBuilder header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                header.AppendFormat("# Visual C++ {0} 2013 for Windows Desktop\n", vsEdition);
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

        public string ProjectVersion
        {
            get
            {
                return "12.00";
            }
        }

        public string PlatformToolset
        {
            get
            {
                return "v120";
            }
        }

        public string ProjectExtension
        {
            get
            {
                return ".vcxproj";
            }
        }
    }
}
