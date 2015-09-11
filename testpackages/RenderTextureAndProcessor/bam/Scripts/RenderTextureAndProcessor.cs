#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace RenderTextureAndProcessor
{
    sealed class RenderTextureV2 :
        C.Cxx.V2.GUIApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer("$(pkgroot)/source/common/*.h");
            headers.AddFiles("$(pkgroot)/source/rendertexture/*.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/common/*.cpp");
            source.AddFiles("$(pkgroot)/source/rendertexture/*.cpp");
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/common", this));

                var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });

            this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            this.LinkAgainst<OpenGLSDK.OpenGLV2>();

            this.PrivatePatch(settings =>
            {
                var linker = settings as C.V2.ICommonLinkerOptions;
                linker.Libraries.Add("WS2_32.lib");
                linker.Libraries.Add("GDI32.lib");
                linker.Libraries.Add("USER32.lib");
                linker.Libraries.Add("SHELL32.lib"); // for DragQueryFile
            });

            this.RequiredToExist<TextureProcessorV2>();
        }
    }

    sealed class TextureProcessorV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/source/common/*.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/common/*.cpp");
            source.AddFiles("$(pkgroot)/source/textureprocessor/*.cpp");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/common", this));

                    var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });

            this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.V2.ICommonLinkerOptions;
                    linker.Libraries.Add("WS2_32.lib");
                });
        }
    }

    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var app = this.Include<RenderTextureV2>(C.V2.GUIApplication.Key, EPublishingType.WindowedApplication);
            this.Include<TextureProcessorV2>(C.V2.ConsoleApplication.Key, ".", app);
        }
    }

    // Define module classes here
    class RenderTexture :
        C.WindowsApplication
    {
        public
        RenderTexture()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var commonDir = sourceDir.SubDirectory("common");
            var renderTextureDir = sourceDir.SubDirectory("rendertexture");
            this.headerFiles.Include(commonDir, "*.h");
            this.headerFiles.Include(renderTextureDir, "*.h");
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var commonDir = sourceDir.SubDirectory("common");
                var renderTextureDir = sourceDir.SubDirectory("rendertexture");
                this.Include(commonDir, "*.cpp");
                this.Include(renderTextureDir, "*.cpp");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                {
                    var options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    var options = module.Options as C.ICCompilerOptions;
                    var sourceDir = this.PackageLocation.SubDirectory("source");
                    options.IncludePaths.Include(sourceDir, "common");
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray requiredLibrariesVC = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "GDI32.lib",
            "USER32.lib",
            "WS2_32.lib",
            "SHELL32.lib"
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Bam.Core.StringArray requiredLibrariesMingw = new Bam.Core.StringArray(
            "-lws2_32",
            "-lgdi32"
        );

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependentModules = new Bam.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependentModules = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [Bam.Core.RequiredModules]
        Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(typeof(TextureProcessor));
    }

    class TextureProcessor :
        C.Application
    {
        public
        TextureProcessor()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var commonDir = sourceDir.SubDirectory("common");
            this.headerFiles.Include(commonDir, "*.h");
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var commonDir = sourceDir.SubDirectory("common");
                var textureProcessorDir = sourceDir.SubDirectory("textureprocessor");
                this.Include(commonDir, "*.cpp");
                this.Include(textureProcessorDir, "*.cpp");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                {
                    var options = module.Options as C.ICxxCompilerOptions;
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                }

                {
                    var options = module.Options as C.ICCompilerOptions;
                    var sourceDir = this.PackageLocation.SubDirectory("source");
                    options.IncludePaths.Include(sourceDir, "common");
                }
            }
        }

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray requiredLibrariesVC = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "WS2_32.lib"
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Bam.Core.StringArray requiredLibrariesMingw = new Bam.Core.StringArray("-lws2_32");

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray dependentModules = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
