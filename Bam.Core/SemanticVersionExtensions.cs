#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace Bam.Core
{
    /// <summary>
    /// Extension methods on the ISemanticVersion interface, in order to do comparisons
    /// </summary>
    public static class SemanticVersionExtensions
    {
        /// <summary>
        /// Compare only major version numbers.
        /// </summary>
        /// <param name="version">ISemanticVersion to compare</param>
        /// <param name="majorVersion">The minimum major version</param>
        /// <returns>true if the major version is at least the specified value.</returns>
        public static bool
        IsAtLeast(
            this ISemanticVersion version,
            int majorVersion)
        {
            return (version.MajorVersion.GetValueOrDefault(0) >= majorVersion);
        }

        /// <summary>
        /// Compare major and minor version numbers
        /// </summary>
        /// <param name="version">ISemanticVersion to compare</param>
        /// <param name="majorVersion">The minimum major version</param>
        /// <param name="minorVersion">The minimum minor version</param>
        /// <returns>true if the major version is at least the specified value, and the minor version is at least the specified value.</returns>
        public static bool
        IsAtLeast(
            this ISemanticVersion version,
            int majorVersion,
            int minorVersion)
        {
            return version.IsAtLeast(majorVersion) && (version.MinorVersion.GetValueOrDefault(0) >= minorVersion);
        }

        /// <summary>
        /// Compare major, minor and patch version numbers
        /// </summary>
        /// <param name="version">ISemanticVersion to compare</param>
        /// <param name="majorVersion">The minimum major version</param>
        /// <param name="minorVersion">The minimum minor version</param>
        /// <param name="patchVersion">The minimum patch version</param>
        /// <returns>true if the major version is at least the specified value, and the minor version is at least the specified value, and the patch version is at least the specified version.</returns>
        public static bool
        IsAtLeast(
            this ISemanticVersion version,
            int majorVersion,
            int minorVersion,
            int patchVersion)
        {
            return version.IsAtLeast(majorVersion, minorVersion) && (version.PatchVersion.GetValueOrDefault(0) >= patchVersion);
        }
    }
}
