#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// Optimization level for the compiler
    /// </summary>
    public enum EOptimization
    {
        /// <summary>
        /// No optimizations
        /// </summary>
        Off = 0,

        /// <summary>
        /// Optimize for size
        /// </summary>
        Size = 1,

        /// <summary>
        /// Optimize for speed
        /// </summary>
        Speed = 2,

        /// <summary>
        /// Enable full optimizations
        /// </summary>
        Full = 3,

        /// <summary>
        /// Customize the optimization level (TODO)
        /// </summary>
        Custom = 4 // TODO: confirm
    }

    /// <summary>
    /// Define the output of the compiler
    /// </summary>
    public enum ECompilerOutput
    {
        /// <summary>
        /// Compiler generates object code
        /// </summary>
        CompileOnly = 0,

        /// <summary>
        /// Compiler stops after the preprocessor
        /// </summary>
        Preprocess = 1
    }

    /// <summary>
    /// Which language should the compiler target
    /// </summary>
    public enum ETargetLanguage
    {
        /// <summary>
        /// Default language based on the file extension
        /// </summary>
        Default = 0,

        /// <summary>
        /// Compile as C code
        /// </summary>
        C = 1,

        /// <summary>
        /// Compile as C++ code
        /// </summary>
        Cxx = 2,

        /// <summary>
        /// Compile as ObjectiveC code
        /// </summary>
        ObjectiveC = 3,

        /// <summary>
        /// Compile as ObjectiveC++ code
        /// </summary>
        ObjectiveCxx = 4
    }

    /// <summary>
    /// Which character set is used by the compiler
    /// May affect system APIs used.
    /// </summary>
    public enum ECharacterSet
    {
        /// <summary>
        /// No character set is defined
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Compile as Unicode
        /// </summary>
        Unicode = 1,

        /// <summary>
        /// Compile as multibyte
        /// </summary>
        MultiByte = 2
    }

    /// <summary>
    /// C language standard options
    /// </summary>
    public enum ELanguageStandard
    {
        /// <summary>
        /// No standard set, use compiler default
        /// </summary>
        NotSet,

        /// <summary>
        /// Compile as the C89 standard
        /// </summary>
        C89,

        /// <summary>
        /// Compile as the C99 standard
        /// </summary>
        C99
    }
}
