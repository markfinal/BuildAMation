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
namespace Test5
{
    class MyDynamicLibTestApp :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.PrivatePatch(settings =>
                {
                    var gccCommon = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccCommon)
                    {
                        gccCommon.CanUseOrigin = true;
                        gccCommon.RPath.AddUnique("$ORIGIN");
                    }
                });

            var source = this.CreateCSourceContainer("$(packagedir)/source/dynamicmain.c");

            this.LinkAgainst<Test4.MyStaticLib>();
            this.CompileAndLinkAgainst<Test4.MyDynamicLib>(source);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    sealed class RuntimePackage :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);

            var appAnchor = this.Include<MyDynamicLibTestApp>(C.ConsoleApplication.Key);

            // copy the required runtime library next to the binary
            if (this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                (appAnchor.SourceModule as MyDynamicLibTestApp).Linker is VisualCCommon.LinkerBase)
            {
                // just C runtime here
                var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                this.IncludeFiles(runtimeLibrary.CRuntimePaths((appAnchor.SourceModule as C.CModule).BitDepth), this.ExecutableDir, appAnchor);
            }
        }
    }

    sealed class SDKPackage :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.SetDefaultMacrosAndMappings(EPublishingType.Library);

            var dynamicLibAnchor = this.Include<Test4.MyDynamicLib>(C.DynamicLibrary.Key);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Include<Test4.MyDynamicLib>(C.DynamicLibrary.ImportLibraryKey);
            }
            this.IncludeFiles<Test4.MyDynamicLib>("$(packagedir)/include/dynamiclibrary.h", this.HeaderDir, dynamicLibAnchor);
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class DebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateSymbolsFrom<RuntimePackage>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Stripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.StripBinariesFrom<RuntimePackage, DebugSymbols>();
        }
    }
}
