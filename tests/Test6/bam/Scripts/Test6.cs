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
namespace Test6
{
    sealed class ConditionApplicationV2 :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer("$(pkgroot)/include/header.h");
            headers.AddFile("$(pkgroot)/include/platform/platform.h");

            var source = this.CreateCSourceContainer();
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.TokenizedString.Create("$(pkgroot)/include", this));
                });

            var main = source.AddFile("$(pkgroot)/source/main.c");
            main.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerOptions;
                    compiler.PreprocessorDefines.Add("MAIN_C");
                    compiler.IncludePaths.Add(Bam.Core.TokenizedString.Create("$(pkgroot)/include/platform", this));
                });

            var platformPath = (this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug) ?
                "$(pkgroot)/source/debug/debug.c" :
                "$(pkgroot)/source/optimized/optimized.c";
            source.AddFile(platformPath);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }
}
