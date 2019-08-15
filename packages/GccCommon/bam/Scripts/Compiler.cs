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
    /// Abstract class representing any Gcc compiler
    /// </summary>
    public abstract class CompilerBase :
        C.CompilerTool
    {
        protected CompilerBase()
        {
            this.Macros.AddVerbatim(C.ModuleMacroNames.ObjectFileExtension, ".o");

            this.GccMetaData = Bam.Core.Graph.Instance.PackageMetaData<Gcc.MetaData>("Gcc");
            var discovery = this.GccMetaData as C.IToolchainDiscovery;
            discovery.discover(depth: null);
            this.Version = this.GccMetaData.ToolchainVersion;
        }

        /// <summary>
        /// Get the Gcc meta data for this tool
        /// </summary>
        protected Gcc.MetaData GccMetaData { get; private set; }

        /// <summary>
        /// Get the executable path to this tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["CompilerPath"];

        public override void
        CompileAsShared(
            Bam.Core.Settings settings)
        {
            var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
            gccCompiler.PositionIndependentCode = true;
        }
    }

    /// <summary>
    /// Both 32-bit and 64-bit GCC C compilers
    /// </summary>
    [C.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class CCompiler :
        CompilerBase
    {
        public CCompiler() => this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GccPath));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Gcc.CCompilerSettings(module);
    }

    /// <summary>
    /// Both 32-bit and 64-bit GCC C++ compilers
    /// </summary>
    [C.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class CxxCompiler :
        CompilerBase
    {
        public CxxCompiler() => this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GxxPath));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Gcc.CxxCompilerSettings(module);
    }

    /// <summary>
    /// Both 32-bit and 64-bit GCC Objective C compilers
    /// </summary>
    [C.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class ObjectiveCCompiler :
        CompilerBase
    {
        public ObjectiveCCompiler() => this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GccPath));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Gcc.ObjectiveCCompilerSettings(module);
    }

    /// <summary>
    /// Both 32-bit and 64-bit GCC Objective C++ compilers
    /// </summary>
    [C.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class ObjectiveCxxCompiler :
        CompilerBase
    {
        public ObjectiveCxxCompiler() => this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GxxPath));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Gcc.ObjectiveCxxCompilerSettings(module);
    }
}
