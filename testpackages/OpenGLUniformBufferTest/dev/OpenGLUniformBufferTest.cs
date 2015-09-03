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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
using System.Linq;
namespace OpenGLUniformBufferTest
{
    sealed class GLUniformBufferTestV2 :
        C.Cxx.V2.GUIApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/source/*.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/*.cpp");
            source.PrivatePatch(settings =>
                {
                    var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });

            this.LinkAgainst<OpenGLSDK.OpenGLV2>();

            var rendererObj = source.Children.Where(item => (item as C.Cxx.V2.ObjectFile).InputPath.Parse().Contains("renderer")).ElementAt(0) as C.Cxx.V2.ObjectFile;
            this.CompileAndLinkAgainst<GLEW.GLEWStaticV2>(rendererObj);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
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
