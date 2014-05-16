// <copyright file="QtMultimedia.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Multimedia : Base
    {
        public Multimedia()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtMultimedia_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtMultimedia_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtMultimedia_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
#if true
            var module = this.GetModuleDynamicLibrary(target, "QtMultimedia");
            if (null != module)
            {
                this.Locations[C.DynamicLibrary.OutputFile] = module;
            }
#else
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(target, "QtMultimedia");
#endif
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtMultimedia_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
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
                this.AddIncludePath(options, target, "QtMultimedia");
            }
        }
    }
}
