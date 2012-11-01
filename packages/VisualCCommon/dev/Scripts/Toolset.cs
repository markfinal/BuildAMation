// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract class Toolset : Opus.Core.IToolset, /*C.ICompilerInfo, C.ILinkerInfo, */C.IArchiverInfo, C.IWinResourceCompilerInfo, VisualStudioProcessor.IVisualStudioTargetInfo
    {
        public string installPath;
        public string bin32Folder;
        public string bin64Folder;
        public string bin6432Folder;
        public Opus.Core.StringArray lib32Folder = new Opus.Core.StringArray();
        public Opus.Core.StringArray lib64Folder = new Opus.Core.StringArray();
        protected Opus.Core.StringArray environment = new Opus.Core.StringArray();

        protected abstract void GetInstallPath();
        protected abstract string GetVersion(Opus.Core.BaseTarget baseTarget);

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        protected System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath();

            if (baseTarget.HasPlatform(Opus.Core.EPlatform.Win64))
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

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get
            {
                this.GetInstallPath();
                return this.environment;
            }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath();
            return this.installPath;
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return this.GetVersion(baseTarget);
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!this.toolMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception(System.String.Format("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString()), false);
            }

            return this.toolMap[toolType];
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            if (!this.toolOptionsMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception(System.String.Format("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString()), false);
            }

            return this.toolOptionsMap[toolType];
        }

        #endregion

#if false
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
#endif

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