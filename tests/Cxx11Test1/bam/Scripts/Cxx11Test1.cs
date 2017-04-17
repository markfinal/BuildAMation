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
namespace Cxx11Test1
{
    public sealed class LocalPolicy :
        Bam.Core.ISitePolicy
    {
        void
        ISitePolicy.DefineLocalSettings(
            Settings settings,
            Module module)
        {
            var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
            if (null != cxxCompiler)
            {
                cxxCompiler.LanguageStandard = C.Cxx.ELanguageStandard.Cxx11;
                cxxCompiler.StandardLibrary = C.Cxx.EStandardLibrary.libcxx;
            }

            var cxxLinker = settings as C.ICxxOnlyLinkerSettings;
            if (null != cxxLinker)
            {
                cxxLinker.StandardLibrary = C.Cxx.EStandardLibrary.libcxx;
            }
        }
    }

    public sealed class ConfigureOSX :
        Bam.Core.IPackageMetaDataConfigure<Clang.MetaData>
    {
        void
        Bam.Core.IPackageMetaDataConfigure<Clang.MetaData>.Configure(
            Clang.MetaData instance)
        {
            // using libc++ requires 10.7 minimum deployment
            instance.MinimumVersionSupported = "macosx10.7";
        }
    }

    public sealed class TestProg :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/main.cpp");

            source.PrivatePatch(settings =>
                {
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                        this.Linker is VisualCCommon.LinkerBase)
                    {
                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }
}
