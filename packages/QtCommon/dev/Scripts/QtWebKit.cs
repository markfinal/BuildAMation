// <copyright file="QtWebKit.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class WebKit : Base
    {
        public WebKit()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtWebKit_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtWebKit_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtWebKit_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(target, "QtWebKit");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtWebKit_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtWebKit");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtWebKit_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtWebKit headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtWebKit_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtWebKit");
            }
        }
    }
}
