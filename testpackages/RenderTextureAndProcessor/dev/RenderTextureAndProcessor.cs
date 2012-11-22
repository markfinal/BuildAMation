// Automatically generated by Opus v0.00
namespace RenderTextureAndProcessor
{
    // Define module classes here
    class RenderTexture : C.WindowsApplication
    {
        public RenderTexture()
        {
            this.headerFiles.Include(this, "source", "common", "*.h");
            this.headerFiles.Include(this, "source", "rendertexture", "*.h");
        }

        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "common", "*.cpp");
                this.Include(this, "source", "rendertexture", "*.cpp");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                {
                    C.ICxxCompilerOptions options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    C.ICCompilerOptions options = module.Options as C.ICCompilerOptions;
                    options.IncludePaths.Include(this, "source", "common");
                }
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Opus.Core.FileCollection headerFiles = new Opus.Core.FileCollection();

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray requiredLibrariesVC = new Opus.Core.StringArray(
            "KERNEL32.lib",
            "GDI32.lib",
            "USER32.lib",
            "OPENGL32.lib",
            "WS2_32.lib",
            "SHELL32.lib"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Opus.Core.StringArray requiredLibrariesMingw = new Opus.Core.StringArray(
            "-lws2_32",
            "-lopengl32",
            "-lgdi32"
        );

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray dependentModules = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK),
            typeof(OpenGLSDK.OpenGL)
        );

        [Opus.Core.RequiredModules]
        Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(typeof(TextureProcessor));
    }

    class TextureProcessor : C.Application
    {
        public TextureProcessor()
        {
            this.headerFiles.Include(this, "source", "common", "*.h");
            this.headerFiles.Include(this, "source", "textureprocessor", "*.h");
        }

        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Include(this, "source", "common", "*.cpp");
                this.Include(this, "source", "textureprocessor", "*.cpp");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                {
                    C.ICxxCompilerOptions options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    C.ICCompilerOptions options = module.Options as C.ICCompilerOptions;
                    options.IncludePaths.Include(this, "source", "common");
                }
            }
        }

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray requiredLibrariesVC = new Opus.Core.StringArray(
            "KERNEL32.lib",
            "WS2_32.lib"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Opus.Core.StringArray requiredLibrariesMingw = new Opus.Core.StringArray("-lws2_32");

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Opus.Core.FileCollection headerFiles = new Opus.Core.FileCollection();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray dependentModules = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}