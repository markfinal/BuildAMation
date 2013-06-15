// <copyright file="QtMultimedia.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Multimedia : Base
    {
        public Multimedia(System.Type toolsetType, bool includeModule)
        {
            this.ToolsetType = toolsetType;
            this.IncludeModule = includeModule;

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtMultimedia_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtMultimedia_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtMultimedia_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
            options.OutputPaths[C.OutputFileFlags.Executable] = new Opus.Core.StringArray(this.GetModuleDynamicLibrary(this.ToolsetType, target, "QtMultimedia"));
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtMultimedia_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(this.ToolsetType, options, target);
                this.AddModuleLibrary(options, target, "QtMultimedia");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtMultimedia_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtMultimedia headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtMultimedia_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(this.ToolsetType, options, target, "QtMultimedia", this.IncludeModule);
            }
        }
    }
}
