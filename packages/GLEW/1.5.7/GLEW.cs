// <copyright file="GLEW.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GLEW package</summary>
// <author>Mark Final</author>
namespace GLEW
{
    // Define module classes here
    class GLEWStatic :
        C.StaticLibrary
    {
        public
        GLEWStatic()
        {
            var glewDir = this.PackageLocation.SubDirectory("glew-1.5.7");
            var includeDir = glewDir.SubDirectory("include");
            var GLDir = includeDir.SubDirectory("GL");
            this.headerFiles.Include(GLDir, "*.h");
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var glewDir = this.PackageLocation.SubDirectory("glew-1.5.7");
                var sourceDir = glewDir.SubDirectory("src");
                this.Include(sourceDir, "glew.c");
                //this.Add(new C.ObjectFile(new Bam.Core.File(@"glew-1.5.7/src/glewinfo.c")));
                //this.Add(new C.ObjectFile(new Bam.Core.File(@"glew-1.5.7/src/visualinfo.c")));

                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(GLEW_IncludePathAndStaticDefine);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(GLEW_VCWarningLevel);
                //this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(GLEW_VCSecurity);
            }

            void
            GLEW_VCSecurity(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (module.Options is VisualCCommon.ICCompilerOptions)
                {
                    var compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }

            void
            GLEW_VCWarningLevel(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                }
            }

            [C.ExportCompilerOptionsDelegate]
            void
            GLEW_IncludePathAndStaticDefine(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                var glewDir = this.PackageLocation.SubDirectory("glew-1.5.7");
                compilerOptions.IncludePaths.Include(glewDir, "include");
                compilerOptions.Defines.Add("GLEW_STATIC");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );
    }
}
