#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace Test9
{
    sealed class CFile :
        C.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.InputPath = this.CreateTokenizedString("$(packagedir)/source/main_c.c");
        }
    }

    sealed class CFileCollection :
        C.CObjectFileCollection
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.AddFile("$(packagedir)/source/main_c.c");
        }
    }

    sealed class CppFile :
        C.Cxx.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.InputPath = this.CreateTokenizedString("$(packagedir)/source/main_cpp.c");
            this.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICxxOnlyCompilerSettings;
                    compiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });
        }
    }

    // Note: Uses the C++ application module, in order to use the C++ linker, in order to link in C++ runtimes
    sealed class MixedLanguageApplication :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/*.h");

            var cSource = this.CreateCSourceContainer("$(packagedir)/source/library_c.c");
            cSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                });

            var cxxSource = this.CreateCxxSourceContainer();
            cxxSource.AddFile("$(packagedir)/source/library_cpp.c");
            cxxSource.AddFile("$(packagedir)/source/appmain_cpp.c");
            cxxSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    sealed class CStaticLibraryFromCollection :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/library_c.h");

            var source = this.CreateCSourceContainer("$(packagedir)/source/library_c.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                });
        }
    }

    sealed class CppStaticLibaryFromCollection :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/library_cpp.h");

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/library_cpp.c");
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));

                var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });
        }
    }

    sealed class CDynamicLibraryFromCollection :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/library_c.h");

            var source = this.CreateCSourceContainer("$(packagedir)/source/library_c.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    sealed class CppDynamicLibaryFromCollection :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/library_cpp.h");

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/library_cpp.c");
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));

                var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }
}
