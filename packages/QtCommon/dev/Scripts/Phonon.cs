// <copyright file="Phonon.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Phonon :
        Base
    {
        public
        Phonon()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Phonon_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Phonon_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Phonon_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "Phonon");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        Phonon_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "Phonon");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        Phonon_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // Phonon headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        Phonon_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "Phonon");
            }
        }
    }
}
