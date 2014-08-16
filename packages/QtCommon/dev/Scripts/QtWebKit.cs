// <copyright file="QtWebKit.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class WebKit :
        Base
    {
        public
        WebKit()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtWebKit_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtWebKit_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtWebKit_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "QtWebKit");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtWebKit_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtWebKit");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtWebKit_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtWebKit headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtWebKit_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtWebKit");
            }
        }
    }
}
