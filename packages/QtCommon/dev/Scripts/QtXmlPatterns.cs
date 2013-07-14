// <copyright file="QtXmlPatternsPatterns.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class XmlPatterns : Base
    {
        public XmlPatterns(System.Type toolsetType, bool includeModule)
        {
            this.ToolsetType = toolsetType;
            this.IncludeModule = includeModule;

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtXmlPatternsPatterns_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtXmlPatternsPatterns_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtXmlPatternsPatterns_LinkerOptions);
        }

        public override void RegisterOutputFiles(Opus.Core.BaseOptionCollection options, Opus.Core.Target target, string modulePath)
        {
            options.OutputPaths[C.OutputFileFlags.Executable] = this.GetModuleDynamicLibrary(this.ToolsetType, target, "QtXmlPatternsPatterns");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtXmlPatternsPatterns_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(this.ToolsetType, options, target);
                this.AddModuleLibrary(options, target, "QtXmlPatternsPatterns");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtXmlPatternsPatterns_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtXmlPatternsPatterns headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtXmlPatternsPatterns_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(this.ToolsetType, options, target, "QtXmlPatternsPatterns", this.IncludeModule);
            }
        }
    }
}
