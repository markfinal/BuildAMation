// <copyright file="QtDesigner.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Designer : Base
    {
        public Designer()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtDesigner_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtDesigner_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtDesigner_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
#if true
            var module = this.GetModuleDynamicLibrary(target, "QtDesigner");
            if (null != module)
            {
                this.Locations[C.DynamicLibrary.OutputFile] = module;
            }
#else
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(target, "QtDesigner");
#endif
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtDesigner_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtDesigner");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtDesigner_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtDesigner headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtDesigner_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtDesigner");
            }
        }
    }
}
