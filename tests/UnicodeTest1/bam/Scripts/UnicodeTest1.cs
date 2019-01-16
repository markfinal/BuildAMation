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
namespace UnicodeTest1
{
    public abstract class Common :
        C.Cxx.ConsoleApplication
    {
        protected C.Cxx.ObjectFileCollection Source { get; private set; }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Source = this.CreateCxxSourceContainer("$(packagedir)/source/main.cpp");
            this.Source.PrivatePatch(settings =>
            {
                var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
            });
        }
    }

    public sealed class NonUnicodeTest :
        Common
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("D_COMPILE_AS_NON_UNICODE");

                var winCompiler = settings as C.ICommonCompilerSettingsWin;
                winCompiler.CharacterSet = C.ECharacterSet.NotSet;
            });
        }
    }

    public sealed class UnicodeTest :
        Common
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("D_COMPILE_AS_UNICODE");

                var winCompiler = settings as C.ICommonCompilerSettingsWin;
                winCompiler.CharacterSet = C.ECharacterSet.Unicode;
            });
        }
    }

    public sealed class MultiByteTest :
        Common
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("D_COMPILE_AS_MULTIBYTE");

                var winCompiler = settings as C.ICommonCompilerSettingsWin;
                winCompiler.CharacterSet = C.ECharacterSet.MultiByte;
            });
        }
    }
}
