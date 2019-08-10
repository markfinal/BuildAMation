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
    /// Abstract class for all compiler tools
    /// </summary>
    public abstract class CompilerBase :
        C.CompilerTool
    {
        /// <summary>
        /// List of arguments
        /// </summary>
        protected Bam.Core.TokenizedStringArray arguments = new Bam.Core.TokenizedStringArray();

        protected CompilerBase()
        {
            this.Macros.AddVerbatim("objext", ".o");

            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            var discovery = clangMeta as C.IToolchainDiscovery;
            discovery.discover(null);
            this.Version = clangMeta.ToolchainVersion;
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim($"--sdk {clangMeta.SDK}"));
        }

        /// <summary>
        /// Executable path of the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(ConfigureUtilities.XcrunPath);

        /// <summary>
        /// Arguments to pass to the tool prior to module settings
        /// </summary>
        public override Bam.Core.TokenizedStringArray InitialArguments => this.arguments;
    }

    /// <summary>
    /// 32-bit and 64-bit C compilers
    /// </summary>
    [C.RegisterCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class CCompiler :
        CompilerBase
    {
        public CCompiler() => this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang"));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Clang.CCompilerSettings(module);
    }

    /// <summary>
    /// 32-bit and 64-bit C++ compilers
    /// </summary>
    [C.RegisterCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class CxxCompiler :
        CompilerBase
    {
        public CxxCompiler() => this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang++"));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Clang.CxxCompilerSettings(module);
    }

    /// <summary>
    /// 32-bit and 64-bit Objective C compiler
    /// </summary>
    [C.RegisterObjectiveCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class ObjectiveCCompiler :
        CompilerBase
    {
        public ObjectiveCCompiler() => this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang"));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Clang.ObjectiveCCompilerSettings(module);
    }

    /// <summary>
    /// 32-bit and 64-bit Objective C++ tools
    /// </summary>
    [C.RegisterObjectiveCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class ObjectiveCxxCompiler :
    CompilerBase
    {
        public ObjectiveCxxCompiler() => this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang++"));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Clang.ObjectiveCxxCompilerSettings(module);
    }
}
