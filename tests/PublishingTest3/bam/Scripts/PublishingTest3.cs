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
namespace PublishingTest3
{
    class SimpleDynamicLib :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/source/dynamiclib.h");
            this.CreateCSourceContainer("$(packagedir)/source/dynamiclib.c");

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    class SimpleExe1 :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/main1.c");
            this.CompileAndLinkAgainst<SimpleDynamicLib>(source);

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }
        }
    }

    class SimpleExe2 :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/main2.c");
            this.CompileAndLinkAgainst<SimpleDynamicLib>(source);

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }
        }
    }

    sealed class Runtime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacros(EPublishingType.ConsoleApplication);
            // TODO: this will probably fail in VSSolution mode if not using an inline tokenizedstring
            // or not using ConsoleApplication mode
            this.Include2<SimpleExe1>(C.ConsoleApplication.Key, this.BinDir);
            this.Include2<SimpleExe2>(C.ConsoleApplication.Key, this.BinDir);
#else
            var app = this.Include<SimpleExe>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            this.Include<SimpleDynamicLib>(C.DynamicLibrary.Key, ".", app);
#endif
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

            this.CreateSymbolsFrom<Runtime>();
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

            this.StripBinariesFrom<Runtime, DebugSymbols>();
        }
    }
}
