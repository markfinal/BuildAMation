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
namespace GccCommon
{
    /// <summary>
    /// Abstract class representing any Gcc assembler tool
    /// </summary>
    abstract class AssemblerBase :
        C.AssemblerTool
    {
        protected AssemblerBase()
        {
            this.Macros.AddVerbatim(C.ModuleMacroNames.ObjectFileExtension, ".o");

            this.GccMetaData = Bam.Core.Graph.Instance.PackageMetaData<Gcc.MetaData>("Gcc");
            var discovery = this.GccMetaData as C.IToolchainDiscovery;
            discovery.Discover(depth: null);
            this.Version = this.GccMetaData.ToolchainVersion;
            this.Macros.Add("AssemblerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GccPath));
        }

        /// <summary>
        /// Get the meta data for this tool
        /// </summary>
        protected Gcc.MetaData GccMetaData { get; private set; }

        /// <summary>
        /// Get the executable path to this tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros.FromName("AssemblerPath");
    }

    /// <summary>
    /// Both 32-bit and 64-bit assemblers
    /// </summary>
    [C.RegisterAssembler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterAssembler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    sealed class Assembler :
        AssemblerBase
    {
        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(Gcc.AssemblerSettings);
    }
}
