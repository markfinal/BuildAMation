// <copyright file="GLEW.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GLEW package</summary>
// <author>Mark Final</author>
namespace GLEW
{
    // Define module classes here
    class GLEWStatic : C.StaticLibrary
    {
        public GLEWStatic()
        {
            var glIncludeDir = this.Locations["PackageDir"].SubDirectory("glew-1.5.7", "include", "GL");
            this.headerFiles.Include(glIncludeDir, "*.h");
        }

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("glew-1.5.7", "src");
                this.Include(sourceDir, "glew.c");
                //this.Add(new C.ObjectFile(new Opus.Core.File(@"glew-1.5.7/src/glewinfo.c")));
                //this.Add(new C.ObjectFile(new Opus.Core.File(@"glew-1.5.7/src/visualinfo.c")));

                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(GLEW_IncludePathAndStaticDefine);
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(GLEW_VCWarningLevel);
                //this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(GLEW_VCSecurity);
            }

            void GLEW_VCSecurity(Opus.Core.IModule module, Opus.Core.Target target)
            {
                if (module.Options is VisualCCommon.ICCompilerOptions)
                {
                    var compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }

            void GLEW_VCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                }
            }

            [C.ExportCompilerOptionsDelegate]
            void GLEW_IncludePathAndStaticDefine(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "glew-1.5.7", "include");
                compilerOptions.Defines.Add("GLEW_STATIC");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Opus.Core.FileCollection headerFiles = new Opus.Core.FileCollection();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Opus.Core.TypeArray winDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );
    }
}
