#region License
// Copyright (c) 2010-2015, Mark Final
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
    /// Linker settings common to all builds.
    /// </summary>
    [Bam.Core.SettingsExtensions(typeof(C.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonLinkerSettings :
        Bam.Core.ISettingsBase
    {
        /// <summary>
        /// Link for the particular number of bits in the architecture.
        /// </summary>
        /// <value>The bits.</value>
        EBit Bits
        {
            get;
            set;
        }

        /// <summary>
        /// Linker will generate a particular output type.
        /// </summary>
        /// <value>The type of the output.</value>
        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Search paths the linker will use to find libraries.
        /// </summary>
        /// <value>The library paths.</value>
        Bam.Core.TokenizedStringArray LibraryPaths
        {
            get;
            set;
        }

        /// <summary>
        /// Libraries to link against that are not Bam modules.
        /// </summary>
        /// <value>The libraries.</value>
        Bam.Core.StringArray Libraries
        {
            get;
            set;
        }

        /// <summary>
        /// Link with debugging symbol information.
        /// </summary>
        /// <value><c>true</c> if debug symbols; otherwise, <c>false</c>.</value>
        bool DebugSymbols
        {
            get;
            set;
        }
    }
}
