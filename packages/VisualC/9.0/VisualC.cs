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
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.Toolchain, typeof(VisualC.Toolchain), typeof(VisualC.ToolchainOptionCollection))]
#endif
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.Win32ResourceCompilerTool, typeof(VisualCCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

[assembly: C.RegisterToolchain(
    "visualc",
    typeof(VisualC.ToolsetInfo),
    typeof(VisualCCommon.CCompiler), typeof(VisualC.CCompilerOptionCollection),
    typeof(VisualCCommon.CPlusPlusCompiler), typeof(VisualC.CPlusPlusCompilerOptionCollection),
    typeof(VisualCCommon.Linker), typeof(VisualC.LinkerOptionCollection),
    typeof(VisualCCommon.Archiver), typeof(VisualC.ArchiverOptionCollection),
    typeof(VisualCCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

namespace VisualC
{
    public class ToolsetInfo : Opus.Core.IToolsetInfo, C.ICompilerInfo, C.ILinkerInfo, C.IArchiverInfo, C.IWinResourceCompilerInfo, VisualStudioProcessor.IVisualStudioTargetInfo
    {
        private static string installPath;
        private static string bin32Folder;
        private static string bin64Folder;
        private static string bin6432Folder;
        private static Opus.Core.StringArray lib32Folder = new Opus.Core.StringArray();
        private static Opus.Core.StringArray lib64Folder = new Opus.Core.StringArray();
        private static Opus.Core.StringArray environment = new Opus.Core.StringArray();

        static void GetInstallPath()
        {
            if (null != installPath)
            {
                return;
            }
            
            if (Opus.Core.State.HasCategory("VisualC") && Opus.Core.State.Has("VisualC", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("VisualC", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("VisualC 2008 install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VisualStudio\Sxs\VC7"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("VisualStudio was not installed");
                    }

                    installPath = key.GetValue("9.0") as string;
                    if (null == installPath)
                    {
                        throw new Opus.Core.Exception("VisualStudio 2008 was not installed");
                    }

                    installPath = installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Opus.Core.Log.DebugMessage("VisualStudio 2008: Installation path from registry '{0}'", installPath);
                }
            }

            bin32Folder = System.IO.Path.Combine(installPath, "bin");
            bin64Folder = System.IO.Path.Combine(bin32Folder, "amd64");
            bin6432Folder = System.IO.Path.Combine(bin32Folder, "x86_amd64");

            lib32Folder.Add(System.IO.Path.Combine(installPath, "lib"));
            lib64Folder.Add(System.IO.Path.Combine(lib32Folder[0], "amd64"));

            string parent = System.IO.Directory.GetParent(installPath).FullName;
            string common7 = System.IO.Path.Combine(parent, "Common7");
            string ide = System.IO.Path.Combine(common7, "IDE");

            environment.Add(ide);
        }

        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            GetInstallPath();
            
            if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                if (Opus.Core.OSUtilities.Is64BitHosting)
                {
                    return bin64Folder;
                }
                else
                {
                    return bin6432Folder;
                }
            }
            else
            {
                return bin32Folder;
            }
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get
            {
                GetInstallPath();
                return environment;
            }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            GetInstallPath();
            return installPath;
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            GetInstallPath();
            return "9.0";
        }

        #endregion

        #region ICompilerInfo Members

        string C.ICompilerInfo.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerInfo.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerInfo.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerInfo.IncludePaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ILinkerInfo Members

        string C.ILinkerInfo.ExecutableSuffix
        {
            get
            {
                return ".exe";
            }
        }

        string C.ILinkerInfo.MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        string C.ILinkerInfo.ImportLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerInfo.ImportLibrarySuffix
        {
            get
            {
                return ".lib";
            }
        }

        string C.ILinkerInfo.DynamicLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerInfo.DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
            }
        }

        string C.ILinkerInfo.ImportLibrarySubDirectory
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerInfo.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Opus.Core.StringArray C.ILinkerInfo.LibPaths(Opus.Core.Target target)
        {
            GetInstallPath();
            if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                return lib64Folder;
            }
            else
            {
                return lib32Folder;
            }
        }

        #endregion

        #region IArchiverInfo Members

        string C.IArchiverInfo.StaticLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IArchiverInfo.StaticLibrarySuffix
        {
            get
            {
                return ".lib";
            }
        }

        string C.IArchiverInfo.StaticLibraryOutputSubDirectory
        {
            get
            {
                return "lib";
            }
        }

        #endregion

        #region IWinResourceCompilerInfo Members

        string C.IWinResourceCompilerInfo.CompiledResourceSuffix
        {
            get
            {
                return ".res";
            }
        }

        #endregion

        #region IVisualStudioTargetInfo Members

        VisualStudioProcessor.EVisualStudioTarget VisualStudioProcessor.IVisualStudioTargetInfo.VisualStudioTarget
        {
            get
            {
                return VisualStudioProcessor.EVisualStudioTarget.VCPROJ;
            }
        }

        #endregion
    }

    public class Solution
    {
        private static System.Guid ProjectTypeGuid;
        private static System.Guid SolutionFolderTypeGuid;

        static Solution()
        {
            // TODO: this path is for VCExpress - what about the professional version?
            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\VCExpress\9.0\Projects"))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("VisualStudio C++ Express 2008 was not installed");
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
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2008");
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
                System.Text.StringBuilder header = new System.Text.StringBuilder();
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
