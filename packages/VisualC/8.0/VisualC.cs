// <copyright file="VisualC.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "visualc", "VisualC.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.ArchiverTool, typeof(VisualCCommon.Archiver), typeof(VisualC.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.CCompilerTool, typeof(VisualCCommon.CCompiler), typeof(VisualC.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.CPlusPlusCompilerTool, typeof(VisualCCommon.CPlusPlusCompiler), typeof(VisualC.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.LinkerTool, typeof(VisualCCommon.Linker), typeof(VisualC.LinkerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.Toolchain, typeof(VisualC.Toolchain), typeof(VisualC.ToolchainOptionCollection))]

namespace VisualC
{
    public static class Solution
    {
        public static string Header
        {
            get
            {
                System.Text.StringBuilder header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 9.00");
                header.AppendLine("# Visual C++ Express 2005");
                return header.ToString();
            }
        }
    }

    public static class Project
    {
        static Project()
        {
            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\VisualStudio\8.0\Projects"))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("VisualStudio 2005 was not installed");
                }

                string[] subKeyNames = key.GetSubKeyNames();
                foreach (string subKeyName in subKeyNames)
                {
                    using (Microsoft.Win32.RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        string projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if (projectExtension == "vcproj")
                            {
                                Guid = new System.Guid(subKeyName);
                                break;
                            }
                        }
                    }
                }
            }

            if (null == Guid)
            {
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2005");
            }
        }

        public static System.Guid Guid
        {
            get;
            private set;
        }

        public static string Version
        {
            get
            {
                return "8.00";
            }
        }
    }
}
