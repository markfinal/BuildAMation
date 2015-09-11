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
namespace glew
{
    sealed class GLEWStaticV2 :
        C.V2.StaticLibrary
    {
        private Bam.Core.V2.Module.PublicPatchDelegate exported = (settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                if (null != compiler)
                {
                    compiler.PreprocessorDefines.Add("GLEW_STATIC");
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", appliedTo));
                }
            };

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer("$(pkgroot)/include/GL/glew.h");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                headers.AddFile("$(pkgroot)/include/GL/wglew.h");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                headers.AddFile("$(pkgroot)/include/GL/glxew.h");
            }

            var source = this.CreateCSourceContainer("$(pkgroot)/src/glew.c");
            source.PrivatePatch(settings => this.exported(settings, this));

            this.PublicPatch((settings, appliedTo) => this.exported(settings, this));

            this.CompileAgainst<OpenGLSDK.OpenGLV2>(source);
            if (this.Librarian is VisualC.V2.Librarian)
            {
                this.CompileAgainst<WindowsSDK.WindowsSDKV2>(source);
            }
        }
    }

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
