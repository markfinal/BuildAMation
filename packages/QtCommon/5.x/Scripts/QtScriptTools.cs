#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QtCommon
{
    public abstract class ScriptTools :
        Base
    {
        public
        ScriptTools()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtScriptTools_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtScriptTools_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtScriptTools_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "QtScriptTools");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtScriptTools_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, true, "ScriptTools");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtScriptTools_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtScriptTools headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtScriptTools_IncludePaths(
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
                    this.AddIncludePath(options, target, "QtScriptTools");
                }
            }
        }
    }
}
