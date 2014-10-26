#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QtCommon
{
    public abstract class Gui :
        Base
    {
        public
        Gui()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtGui_IncludePaths);
            //this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtGui_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtGui_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "Qt5Gui");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtGui_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, true, "Gui");
            }
        }

        // TODO: needed?
        [C.ExportCompilerOptionsDelegate]
        void
        QtGui_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtGui_IncludePaths(
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
                    this.AddIncludePath(options, target, "QtGui");
                }
            }
        }
    }
}
