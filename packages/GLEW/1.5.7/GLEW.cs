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
        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.AddRelativePaths(this, "glew-1.5.7", "src", "glew.c");
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
                    C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }

            void GLEW_VCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
            {
                VisualCCommon.ICCompilerOptions compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                }
            }

            [C.ExportCompilerOptionsDelegate]
            void GLEW_IncludePathAndStaticDefine(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["GLEW"], @"glew-1.5.7\include");
                compilerOptions.Defines.Add("GLEW_STATIC");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );
    }
}
