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
namespace ClangCommon
{
    /// <summary>
    /// 32-bit and 64-bit librarians in clang
    /// </summary>
    [C.RegisterLibrarian("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterLibrarian("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    sealed class Librarian :
        C.LibrarianTool
    {
        /// <summary>
        /// List of arguments
        /// </summary>
        private readonly Bam.Core.TokenizedStringArray arguments = new Bam.Core.TokenizedStringArray();

        public Librarian()
        {
            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            var discovery = clangMeta as C.IToolchainDiscovery;
            discovery.Discover(null);
            this.Version = clangMeta.ToolchainVersion;
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim($"--sdk {clangMeta.SDK}"));
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("ar"));

            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryPrefix, "lib");
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryFileExtension, ".a");
        }

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(Clang.ArchiverSettings);

        /// <summary>
        /// Executable path to the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(ConfigureUtilities.XcrunPath);

        /// <summary>
        /// List of arguments to pass prior to the Module settings
        /// </summary>
        public override Bam.Core.TokenizedStringArray InitialArguments => this.arguments;
    }
}
