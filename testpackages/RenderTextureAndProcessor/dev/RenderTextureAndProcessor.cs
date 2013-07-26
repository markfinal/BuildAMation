// Automatically generated by Opus v0.00
namespace RenderTextureAndProcessor
{
    // Define module classes here
    class RenderTexture : C.WindowsApplication
    {
        public RenderTexture()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.headerFiles.Include(sourceDir, "common", "*.h");
            this.headerFiles.Include(sourceDir, "rendertexture", "*.h");
        }

        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "common", "*.cpp");
                this.Include(sourceDir, "rendertexture", "*.cpp");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                {
                    var options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    var options = module.Options as C.ICCompilerOptions;
                    options.IncludePaths.Include(this.PackageLocation, "source", "common");
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
            "WS2_32.lib",
            "SHELL32.lib"
        );

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Opus.Core.StringArray requiredLibrariesMingw = new Opus.Core.StringArray(
            "-lws2_32",
            "-lgdi32"
        );

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependentModules = new Opus.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependentModules = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [Opus.Core.RequiredModules]
        Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(typeof(TextureProcessor));
    }

    class TextureProcessor : C.Application
    {
        public TextureProcessor()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.headerFiles.Include(sourceDir, "common", "*.h");
            this.headerFiles.Include(sourceDir, "textureprocessor", "*.h");
        }

        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "common", "*.cpp");
                this.Include(sourceDir, "textureprocessor", "*.cpp");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                {
                    var options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    var options = module.Options as C.ICCompilerOptions;
                    options.IncludePaths.Include(this.PackageLocation, "source", "common");
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