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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace OpenGLUniformBufferTest
{
    sealed class GLUniformBufferTestV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/application.cpp");
            source.AddFile("$(pkgroot)/source/errorhandler.cpp");
            source.AddFile("$(pkgroot)/source/main.cpp");
            var rendererObj = source.AddFile("$(pkgroot)/source/renderer.cpp");

            source.PrivatePatch(settings =>
                {
                    var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });

            this.LinkAgainst<OpenGLSDK.OpenGLV2>();
            this.CompileAndLinkAgainst<GLEW.GLEWStaticV2>(rendererObj);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            }

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.V2.ICommonLinkerOptions;
                    if (this.Linker is VisualC.V2.LinkerBase)
                    {
                        linker.Libraries.Add("OPENGL32.lib");
                        linker.Libraries.Add("USER32.lib");
                        linker.Libraries.Add("GDI32.lib");
                    }
                    else if (this.Linker is Mingw.V2.LinkerBase)
                    {
                        linker.Libraries.Add("-lopengl32");
                        linker.Libraries.Add("-lgdi32");
                    }
                });
        }
    }

    // Define module classes here
    class GLUniformBufferTest :
        C.WindowsApplication
    {
        public
        GLUniformBufferTest()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.headerFiles.Include(sourceDir, "*.h");
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");

                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_VCDefines);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_EnableException);
            }

            void
            SourceFiles_EnableException(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                C.ICxxCompilerOptions compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
            }

            void
            SourceFiles_VCDefines(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (module.Options is VisualCCommon.ICCompilerOptions)
                {
                    C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL),
            typeof(GLEW.GLEWStatic)
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray winVCLibraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "USER32.lib",
            "GDI32.lib"
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(Mingw.Toolset) })]
        Bam.Core.StringArray winMingwLibraries = new Bam.Core.StringArray(
            "-lgdi32"
        );
    }
}
