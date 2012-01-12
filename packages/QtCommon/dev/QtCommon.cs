// <copyright file="QtCommon.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class QtCommon : C.ThirdPartyModule
    {
        protected static string installPath;
        protected static string libPath;
        protected static Opus.Core.StringArray includePaths = new Opus.Core.StringArray();

        public static string BinPath
        {
            get;
            protected set;
        }

        public QtCommon()
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }
            
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_LibraryPaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_VisualCWarningLevel);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtCommon_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }
            
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.LibraryPaths.AddAbsoluteDirectory(libPath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }
            
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            foreach (string includePath in includePaths)
            {
                compilerOptions.IncludePaths.AddAbsoluteDirectory(includePath, true);
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
#if OPUS_HOST_WIN32 || OPUS_HOST_WIN64
            VisualCCommon.ICCompilerOptions compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
#endif
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }
    }
}
