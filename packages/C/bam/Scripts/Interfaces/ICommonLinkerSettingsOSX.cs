#region License
// Copyright (c) 2010-2018, Mark Final
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
    /// Linker settings that are common for OSX builds.
    /// </summary>
    [Bam.Core.SettingsExtensions(typeof(C.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonLinkerSettingsOSX :
        Bam.Core.ISettingsBase
    {
        /// <summary>
        /// Array of framework paths to link against.
        /// </summary>
        /// <value>The frameworks.</value>
        Bam.Core.TokenizedStringArray Frameworks
        {
            get;
            set;
        }

        /// <summary>
        /// Array of paths the linker uses to search for frameworks.
        /// </summary>
        /// <value>The framework search paths.</value>
        Bam.Core.TokenizedStringArray FrameworkSearchPaths
        {
            get;
            set;
        }

        /// <summary>
        /// For dylibs, the install name to use.
        /// </summary>
        /// <value>The name of the install.</value>
        Bam.Core.TokenizedString InstallName
        {
            get;
            set;
        }

        /// <summary>
        /// Minimum version of macOS supported for the binary.
        /// </summary>
        /// <value>The minimum version supported.</value>
        string MacOSMinimumVersionSupported
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the current version of the dylib, or null
        /// if not defined.
        /// </summary>
        Bam.Core.TokenizedString CurrentVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the compatibility version of the dylib, or null
        /// if not defined.
        /// </summary>
        Bam.Core.TokenizedString CompatibilityVersion
        {
            get;
            set;
        }
    }
}
