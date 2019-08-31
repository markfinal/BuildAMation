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
namespace C
{
    /// <summary>
    /// Common compiler settings available on all toolchains
    /// </summary>
    [Bam.Core.SettingsExtensions(typeof(C.DefaultSettings.DefaultSettingsExtensions))]
    interface ICommonCompilerSettings :
        Bam.Core.ISettingsBase
    {
        /// <summary>
        /// The number of bits in the architecture to compile for.
        /// This value is generally automatically set by the module.
        /// </summary>
        /// <value>The bits.</value>
        EBit? Bits { get; set; }

        /// <summary>
        /// Compile with debug symbols.
        /// </summary>
        /// <value>The debug symbols.</value>
        bool? DebugSymbols { get; set; }

        /// <summary>
        /// Compile with all warnings as errors.
        /// </summary>
        /// <value>The warnings as errors.</value>
        bool? WarningsAsErrors { get; set; }

        /// <summary>
        /// Compile with specific optimization levels.
        /// </summary>
        /// <value>The optimization.</value>
        C.EOptimization? Optimization { get; set; }

        /// <summary>
        /// Compiler omits the frame pointer from generated code.
        /// </summary>
        /// <value>The omit frame pointer.</value>
        bool? OmitFramePointer { get; set; }

        /// <summary>
        /// Warnings specified are suppressed. Format is different per compiler.
        /// </summary>
        /// <value>The disable warnings.</value>
        Bam.Core.StringArray DisableWarnings { get; set; }

        /// <summary>
        /// List of headers to inject first into the compilation step.
        /// </summary>
        /// <value>Header filenames to process.</value>
        Bam.Core.StringArray NamedHeaders { get; set; }

        /// <summary>
        /// Stop after preprocessing the source file. Do not compile.
        /// </summary>
        bool? PreprocessOnly { get; set; }
    }
}
