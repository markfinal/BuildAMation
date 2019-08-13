#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace Test
{
    public sealed class LocalPolicy :
        Bam.Core.ISitePolicy
    {
        void
        Bam.Core.ISitePolicy.DefineLocalSettings(
            Bam.Core.Settings settings,
            Bam.Core.Module module)
        {
            if (settings is C.ICommonPreprocessorSettings preprocessor)
            {
                preprocessor.PreprocessorDefines.Add("GLOBALOVERRIDE");
            }

            if (settings is C.ICxxOnlyCompilerSettings cxxCompiler)
            {
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }
    }

    sealed class CompileSingleCFile :
        C.ObjectFile
    {
        protected override void
        Init()
        {
            base.Init();
            this.InputPath = this.CreateTokenizedString("$(packagedir)/source/main.c");
        }
    }

    sealed class CompileSingleCFileWithDifferentCompiler :
        C.ObjectFile
    {
        protected override void
        Init()
        {
            base.Init();
            this.InputPath = this.CreateTokenizedString("$(packagedir)/source/main.c");
            try
            {
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    // example of switching out the tool within a module
                    this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<MingwCommon.Compiler32>();
                }
            }
            catch (Bam.Core.UnableToBuildModuleException exception)
            {
                Bam.Core.Exception.DisplayException(
                    exception,
                    $"Continuing to build module {this.GetType().ToString()} with default compiler"
                );
            }
        }
    }

    sealed class CompileSingleCFileWithCustomOptions :
        C.ObjectFile
    {
        protected override void
        Init()
        {
            base.Init();
            this.InputPath = this.CreateTokenizedString("$(packagedir)/source/main.c");
            this.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.DebugSymbols = false;
            });
        }
    }

    sealed class PreprocessSingleCFile :
        C.ObjectFile
    {
        protected override void
        Init()
        {
            base.Init();
            this.InputPath = this.CreateTokenizedString("$(packagedir)/source/main.c");
            this.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessOnly = true;
                });
        }
    }

    sealed class BuildTerminalApplicationFromC :
        C.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateCSourceContainer("$(packagedir)/source/main.c");
        }
    }

    sealed class BuildTerminalApplicationFromCxx :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateCxxSourceContainer("$(packagedir)/source/main.c");
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class BuildWindowsApplication :
        C.GUIApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCSourceContainer("$(packagedir)/source/main.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.WarningsAsErrors = false;
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CreateWinResourceContainer("$(packagedir)/resources/win32.rc");
            }
        }
    }
}
