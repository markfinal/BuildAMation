// <copyright file="QtNetwork.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Network :
        Base
    {
        public
        Network()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtNetwork_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtNetwork_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtNetwork_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "QtNetwork");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtNetwork_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtNetwork");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtNetwork_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtNetwork headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtNetwork_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtNetwork");
            }
        }
    }
}
