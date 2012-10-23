// <copyright file="ToolsetInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    public sealed class ToolsetInfo : Opus.Core.IToolsetInfo, C.ICompilerInfo, C.ILinkerInfo, C.IArchiverInfo, C.IWinResourceCompilerInfo, VisualStudioProcessor.IVisualStudioTargetInfo
    {
        private string installPath;
        private string bin32Folder;
        private string bin64Folder;
        private string bin6432Folder;
        private Opus.Core.StringArray lib32Folder = new Opus.Core.StringArray();
        private Opus.Core.StringArray lib64Folder = new Opus.Core.StringArray();
        private Opus.Core.StringArray environment = new Opus.Core.StringArray();

        void GetInstallPath()
        {
            if (null != this.installPath)
            {
                return;
            }

            if (Opus.Core.State.HasCategory("VisualC") && Opus.Core.State.Has("VisualC", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("VisualC", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("VisualC 2008 install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VisualStudio\Sxs\VC7"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("VisualStudio was not installed");
                    }

                    this.installPath = key.GetValue("9.0") as string;
                    if (null == this.installPath)
                    {
                        throw new Opus.Core.Exception("VisualStudio 2008 was not installed");
                    }

                    this.installPath = this.installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Opus.Core.Log.DebugMessage("VisualStudio 2008: Installation path from registry '{0}'", this.installPath);
                }
            }

            this.bin32Folder = System.IO.Path.Combine(this.installPath, "bin");
            this.bin64Folder = System.IO.Path.Combine(this.bin32Folder, "amd64");
            this.bin6432Folder = System.IO.Path.Combine(this.bin32Folder, "x86_amd64");

            this.lib32Folder.Add(System.IO.Path.Combine(this.installPath, "lib"));
            this.lib64Folder.Add(System.IO.Path.Combine(this.lib32Folder[0], "amd64"));

            string parent = System.IO.Directory.GetParent(this.installPath).FullName;
            string common7 = System.IO.Path.Combine(parent, "Common7");
            string ide = System.IO.Path.Combine(common7, "IDE");

            this.environment.Add(ide);
        }

        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            this.GetInstallPath();

            if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                if (Opus.Core.OSUtilities.Is64BitHosting)
                {
                    return this.bin64Folder;
                }
                else
                {
                    return this.bin6432Folder;
                }
            }
            else
            {
                return this.bin32Folder;
            }
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get
            {
                this.GetInstallPath();
                return this.environment;
            }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            this.GetInstallPath();
            return this.installPath;
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            this.GetInstallPath();
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
            this.GetInstallPath();
            if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                return this.lib64Folder;
            }
            else
            {
                return this.lib32Folder;
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
}