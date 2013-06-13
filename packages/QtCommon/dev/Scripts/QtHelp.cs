// <copyright file="QtHelp.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Help : Base
    {
        public Help(System.Type toolsetType, bool includeModule)
        {
            this.ToolsetType = toolsetType;
            this.IncludeModule = includeModule;

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtHelp_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtHelp_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtHelp_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target)
        {
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(this.ToolsetType, target, "QtHelp");
            base.RegisterOutputFiles(options, target);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtHelp_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(this.ToolsetType, options, target);
                this.AddModuleLibrary(options, target, "QtHelp");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtHelp_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtHelp headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtHelp_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(this.ToolsetType, options, target, "QtHelp", this.IncludeModule);
            }
        }
    }
}
