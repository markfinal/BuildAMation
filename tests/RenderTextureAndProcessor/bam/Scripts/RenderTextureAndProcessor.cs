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
using Bam.Core;
namespace RenderTextureAndProcessor
{
    sealed class RenderTextureV2 :
        C.Cxx.GUIApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer("$(pkgroot)/source/common/*.h");
            headers.AddFiles("$(pkgroot)/source/rendertexture/*.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/common/*.cpp");
            source.AddFiles("$(pkgroot)/source/rendertexture/*.cpp");
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerOptions;
                compiler.IncludePaths.Add(Bam.Core.TokenizedString.Create("$(pkgroot)/source/common", this));

                var cxxCompiler = settings as C.ICxxOnlyCompilerOptions;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            }
            this.LinkAgainst<OpenGLSDK.OpenGLV2>();

            this.PrivatePatch(settings =>
            {
                var linker = settings as C.ICommonLinkerOptions;
                if (this.Linker is VisualC.LinkerBase)
                {
                    linker.Libraries.Add("WS2_32.lib");
                    linker.Libraries.Add("GDI32.lib");
                    linker.Libraries.Add("USER32.lib");
                    linker.Libraries.Add("SHELL32.lib"); // for DragQueryFile
                }
                else
                {
                    linker.Libraries.Add("-lWS2_32");
                    linker.Libraries.Add("-lGDI32");
                }
            });

            this.RequiredToExist<TextureProcessorV2>();
        }
    }

    sealed class TextureProcessorV2 :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/source/common/*.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/common/*.cpp");
            source.AddFiles("$(pkgroot)/source/textureprocessor/*.cpp");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.TokenizedString.Create("$(pkgroot)/source/common", this));

                    var cxxCompiler = settings as C.ICxxOnlyCompilerOptions;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            }

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.ICommonLinkerOptions;
                    if (this.Linker is VisualC.LinkerBase)
                    {
                        linker.Libraries.Add("WS2_32.lib");
                    }
                    else
                    {
                        linker.Libraries.Add("-lWS2_32");
                    }
                });
        }
    }

    sealed class RuntimePackage :
        Publisher.Package
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<RenderTextureV2>(C.GUIApplication.Key, EPublishingType.WindowedApplication);
            this.Include<TextureProcessorV2>(C.ConsoleApplication.Key, ".", app);
        }
    }
}
