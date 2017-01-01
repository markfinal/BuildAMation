#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace EmbedStaticIntoDynamicLibrary
{
    class CProxyForStaticLibrary :
        C.CObjectFileCollection
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.AddFiles("$(packagedir)/source/static/*.c");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include/static"));
                    }
                });

            this.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("STATICLIB_SOURCE");

                    // the source files are shared for both C and C++ compilation
                    // but this option will only be set when compiled as C - the preprocessor checks this
                    compiler.PreprocessorDefines.Add("COMPILE_AS_C");
                });
        }
    }

    class CxxProxyForStaticLibrary :
        C.Cxx.ObjectFileCollection
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.AddFiles("$(packagedir)/source/static/*.c");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include/static"));
                    }
                });

            this.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("STATICLIB_SOURCE");

                    // the source files are shared for both C and C++ compilation
                    // but this option will only be set when compiled as C++ - the preprocessor checks this
                    compiler.PreprocessorDefines.Add("COMPILE_AS_CXX");
                });
        }
    }

    sealed class CDynamicLibrary :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var bamVersion = Bam.Core.Graph.Instance.ProcessState.Version;
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Major.ToString());
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Minor.ToString());
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Build.ToString());
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("EmbedStaticIntoDynamicLibrary: Example C dynamic library");

            var source = this.CreateCSourceContainer("$(packagedir)/source/dynamic/*.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("DYNAMICLIB_SOURCE");
                });

            // publicly because the app requires the include path from the dependent
            this.ExtendSourcePublicly<CProxyForStaticLibrary>(source);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include/dynamic"));
                    }
                });

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    sealed class CxxDynamicLibrary :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var bamVersion = Bam.Core.Graph.Instance.ProcessState.Version;
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Major.ToString());
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Minor.ToString());
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Build.ToString());
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("EmbedStaticIntoDynamicLibrary: Example C++ dynamic library");

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/dynamic/*.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("DYNAMICLIB_SOURCE");
                });

            // publicly because the app requires the include path from the dependent
            this.ExtendSourcePublicly<CxxProxyForStaticLibrary>(source);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include/dynamic"));
                    }
                });

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    class CApp :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/app/*.c");
            this.CompileAndLinkAgainst<CDynamicLibrary>(source);

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
            else if (this.Linker is GccCommon.LinkerBase)
            {
                this.PrivatePatch(settings =>
                    {
                        var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN");
                    });
            }
        }
    }

    class CxxApp :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/app/*.c");
            this.CompileAndLinkAgainst<CxxDynamicLibrary>(source);

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
            else if (this.Linker is GccCommon.LinkerBase)
            {
                this.PrivatePatch(settings =>
                    {
                        var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN");
                    });
            }
        }
    }

    sealed class CAppRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<CApp>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            this.Include<CDynamicLibrary>(C.DynamicLibrary.Key, ".", app);
        }
    }

    sealed class CxxAppRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<CxxApp>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            this.Include<CxxDynamicLibrary>(C.DynamicLibrary.Key, ".", app);
        }
    }
}
