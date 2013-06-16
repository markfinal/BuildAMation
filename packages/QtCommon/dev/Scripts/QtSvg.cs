// <copyright file="QtSvg.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Svg : Base
    {
        public Svg(System.Type toolsetType, bool includeModule)
        {
            this.ToolsetType = toolsetType;
            this.IncludeModule = includeModule;

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtSvg_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtSvg_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtSvg_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(this.ToolsetType, target, "QtSvg");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtSvg_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(this.ToolsetType, options, target);
                this.AddModuleLibrary(options, target, "QtSvg");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtSvg_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtSvg headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtSvg_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(this.ToolsetType, options, target, "QtSvg", this.IncludeModule);
            }
        }
    }
}
