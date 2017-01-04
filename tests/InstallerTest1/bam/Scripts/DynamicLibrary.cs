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
namespace InstallerTest1
{
    public sealed class CDynamicLibrary :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var bamVersion = Bam.Core.Graph.Instance.ProcessState.Version;
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Major.ToString());
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Minor.ToString());
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Build.ToString());
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("InstallerTest1: Example C dynamic library");

            this.CreateHeaderContainer("$(packagedir)/source/dynamiclib/*.h");
            this.CreateCSourceContainer("$(packagedir)/source/dynamiclib/*.cpp");

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/source/dynamiclib"));
                }
            });

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    public sealed class CxxDynamicLibrary :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var bamVersion = Bam.Core.Graph.Instance.ProcessState.Version;
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Major.ToString());
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Minor.ToString());
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim(bamVersion.Build.ToString());
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("InstallerTest1: Example C++ dynamic library");

            this.CreateHeaderContainer("$(packagedir)/source/dynamiclib/*.h");
            this.CreateCxxSourceContainer("$(packagedir)/source/dynamiclib/*.cpp");

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/source/dynamiclib"));
                }
            });

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }
}
