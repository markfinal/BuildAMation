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
[assembly: Bam.Core.RegisterToolset("visualc", typeof(VisualC.Toolset))]

namespace VisualC
{
    public class Solution
    {
        private static System.Guid ProjectTypeGuid;
        private static System.Guid SolutionFolderTypeGuid;
        private static string vsEdition;

        static
        Solution()
        {
            // try the VS Express version first, since it's free
            string registryKey = @"Microsoft\VCExpress\8.0\Projects";
            if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(registryKey))
            {
                vsEdition = "Express";
            }
            else
            {
                registryKey = @"Microsoft\VisualStudio\8.0\Projects";
                if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(registryKey))
                {
                    vsEdition = "Professional";
                }
                else
                {
                    throw new Bam.Core.Exception("VisualStudio C++ 2010 (Express or Professional) was not installed");
                }
            }

            using (Microsoft.Win32.RegistryKey key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(registryKey))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("VisualStudio C++ {0} 2005 was not installed", vsEdition);
                }

                string[] subKeyNames = key.GetSubKeyNames();
                foreach (string subKeyName in subKeyNames)
                {
                    using (Microsoft.Win32.RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        string projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if ("vcproj" == projectExtension)
                            {
                                ProjectTypeGuid = new System.Guid(subKeyName);
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
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2005 {0}", vsEdition);
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
                System.Text.StringBuilder header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 9.00");
                header.AppendLine("# Visual C++ Express 2005");
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
                return "8.00";
            }
        }

        public string ProjectExtension
        {
            get
            {
                return ".vcproj";
            }
        }
    }
}
