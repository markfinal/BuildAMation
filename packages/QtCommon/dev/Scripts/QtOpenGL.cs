// <copyright file="QtOpenGL.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class OpenGL : Base
    {
        public OpenGL(System.Type toolsetType, bool includeModule)
        {
            this.ToolsetType = toolsetType;
            this.IncludeModule = includeModule;

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtOpenGL_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtOpenGL_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtOpenGL_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target)
        {
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(this.ToolsetType, target, "QtOpenGL");
            base.RegisterOutputFiles(options, target);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtOpenGL_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(this.ToolsetType, options, target);
                this.AddModuleLibrary(options, target, "QtOpenGL");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtOpenGL_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtOpenGL headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtOpenGL_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(this.ToolsetType, options, target, "QtOpenGL", this.IncludeModule);
            }
        }
    }
}
