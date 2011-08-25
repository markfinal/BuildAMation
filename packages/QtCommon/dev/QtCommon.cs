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
        protected static string includePath;

        public static string BinPath
        {
            get;
            protected set;
        }

        public QtCommon()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_LibraryPaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_VisualCWarningLevel);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtCommon_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.LibraryPaths.AddAbsoluteDirectory(libPath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.AddAbsoluteDirectory(includePath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            VisualCCommon.ICCompilerOptions compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }
    }
}
