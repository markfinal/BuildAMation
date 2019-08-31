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
namespace MingwCommon
{
    /// <summary>
    /// 32-bit Mingw assembler
    /// </summary>
    [C.RegisterAssembler("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    sealed class Assembler32 :
        C.AssemblerTool
    {
        public Assembler32()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");

            var mingwMeta = Bam.Core.Graph.Instance.PackageMetaData<Mingw.MetaData>("Mingw");
            var discovery = mingwMeta as C.IToolchainDiscovery;
            discovery.Discover(null);

            this.Macros.AddVerbatim("AssemblerSuffix", mingwMeta.ToolSuffix);

            this.Macros.Add("BinPath", this.CreateTokenizedString(@"$(0)\bin", mingwMeta["InstallDir"] as Bam.Core.TokenizedString));
            this.Macros.Add("AssemblerPath", this.CreateTokenizedString(@"$(BinPath)\mingw32-gcc$(AssemblerSuffix).exe"));
            this.Macros.AddVerbatim(C.ModuleMacroNames.ObjectFileExtension, ".o");

            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(this.Macros["BinPath"]));
        }

        /// <summary>
        /// Executable path to the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["AssemblerPath"];

        /// <summary>
        /// Command line switch for response files
        /// </summary>
        public override string UseResponseFileOption => "@";

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(Mingw.AssemblerSettings);
    }
}
