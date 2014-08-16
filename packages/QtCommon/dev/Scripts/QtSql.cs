// <copyright file="QtSql.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Sql :
        Base
    {
        public
        Sql()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtSql_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtSql_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtSql_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "QtSql");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtSql_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtSql");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtSql_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtSql headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtSql_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtSql");
            }
        }
    }
}
