// <copyright file="VisualC.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
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
            var registryKey = @"Microsoft\VCExpress\9.0\Projects";
            if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(registryKey))
            {
                vsEdition = "Express";
            }
            else
            {
                registryKey = @"Microsoft\VisualStudio\9.0\Projects";
                if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(registryKey))
                {
                    vsEdition = "Professional";
                }
                else
                {
                    throw new Bam.Core.Exception("VisualStudio C++ 2008 (Express or Professional) was not installed");
                }
            }

            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(registryKey))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("VisualStudio C++ {0} 2008 was not installed", vsEdition);
                }

                var subKeyNames = key.GetSubKeyNames();
                foreach (var subKeyName in subKeyNames)
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        var projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if (projectExtension == "vcproj")
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
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2008 {0}", vsEdition);
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Bam.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2008");
            }
#endif
        }

        public string Header
        {
            get
            {
                var header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 10.00");
                header.AppendLine("# Visual C++ Express 2008");
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
                return "9.00";
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
