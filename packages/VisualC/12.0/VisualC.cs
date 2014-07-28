// <copyright file="VisualC.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterToolset("visualc", typeof(VisualC.Toolset))]

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
            if (Opus.Core.Win32RegistryUtilities.DoesCUSoftwareKeyExist(registryKey))
            {
                vsEdition = "Express";
            }
            else
            {
                registryKey = @"Microsoft\WDStudio\12.0\Projects";
                if (Opus.Core.Win32RegistryUtilities.DoesCUSoftwareKeyExist(registryKey))
                {
                    vsEdition = "Professional";
                }
                else
                {
                    throw new Opus.Core.Exception("VisualStudio C++ 2013 (Express or Professional) was not installed or not yet run for the current user");
                }
            }

            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenCUSoftwareKey(registryKey))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("VisualStudio C++ {0} 2013 was not installed", vsEdition);
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
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2013 {0}", vsEdition);
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2013");
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
