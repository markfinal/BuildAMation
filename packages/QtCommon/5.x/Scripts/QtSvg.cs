#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QtCommon
{
    public abstract class Svg :
        Base
    {
        public
        Svg()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtSvg_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtSvg_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtSvg_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "QtSvg");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtSvg_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, true, "Svg");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtSvg_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtSvg headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtSvg_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var osxOptions = module.Options as C.ICCompilerOptionsOSX;
            if (osxOptions != null)
            {
                this.AddFrameworkIncludePath(osxOptions, target);
            }
            else
            {
                var options = module.Options as C.ICCompilerOptions;
                if (null != options)
                {
                    this.AddIncludePath(options, target, "QtSvg");
                }
            }
        }
    }
}
