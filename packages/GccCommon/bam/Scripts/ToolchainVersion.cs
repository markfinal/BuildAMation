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
using System.Linq;
namespace GccCommon
{
    /// <summary>
    /// Gcc toolchain version wrapper.
    /// </summary>
    sealed class ToolchainVersion :
        C.ToolchainVersion
    {
        /// <summary>
        /// Gcc 4.8.4
        /// </summary>
        public static readonly C.ToolchainVersion GCC_4_8_4 = FromComponentVersions(4, 8, 4);

        /// <summary>
        /// Gcc 5
        /// </summary>
        public static readonly C.ToolchainVersion GCC_5 = FromComponentVersions(5, 0, 0);

        /// <summary>
        /// Gcc 5.4
        /// </summary>
        public static readonly C.ToolchainVersion GCC_5_4 = FromComponentVersions(5, 4, 0);

        /// <summary>
        /// Gcc 6
        /// </summary>
        public static readonly C.ToolchainVersion GCC_6 = FromComponentVersions(6, 0, 0);

        /// <summary>
        /// Gcc 7
        /// </summary>
        public static readonly C.ToolchainVersion GCC_7 = FromComponentVersions(7, 0, 0);

        /// <summary>
        /// Gcc 8
        /// </summary>
        public static readonly C.ToolchainVersion GCC_8 = FromComponentVersions(8, 0, 0);

        /// <summary>
        /// Gcc 9
        /// </summary>
        public static readonly C.ToolchainVersion GCC_9 = FromComponentVersions(9, 0, 0);

        private ToolchainVersion(
            int major_version,
            int minor_version,
            int patch_level)
        {
            this.combinedVersion = 10000 * major_version + 100 * minor_version + patch_level;
        }

        /// <summary>
        /// Generate a Gcc toolchain version from major, minor, patch components.
        /// </summary>
        /// <param name="major">Major version number</param>
        /// <param name="minor">Minor version number</param>
        /// <param name="patch">Patch version</param>
        /// <returns>Toolchain version</returns>
        static public C.ToolchainVersion
        FromComponentVersions(
            int major,
            int minor,
            int patch)
        {
            return new ToolchainVersion(major, minor, patch);
        }
    }
}
