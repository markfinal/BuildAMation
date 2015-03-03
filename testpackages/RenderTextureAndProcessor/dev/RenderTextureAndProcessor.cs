#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace RenderTextureAndProcessor
{
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
