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
    /// Base class containing shared functionality for all toolchain versions.
    /// Simple comparisons against versions are provided.
    /// Specific toolchains are responsible for constructing derived instances of this class
    /// using appropriate data to identify a version. The version must be collapsable to a single
    /// integer that is ordered in the same manner as the source format of the version.
    /// </summary>
    public abstract class ToolchainVersion
    {
        /// <summary>
        /// Exceptions of this type are thrown when two different types of toolchain versions are compared.
        /// </summary>
        public sealed class IncompatibleTypesException :
            Bam.Core.Exception
        {
            /// <summary>
            /// Create an exception instance.
            /// </summary>
            /// <param name="baseType">The underlying type of the toolchain version in use.</param>
            /// <param name="comparingType">The underlying type of the toolchain version to be compared against.</param>
            public IncompatibleTypesException(
                System.Type baseType,
                System.Type comparingType)
                :
                base($"Unable to compare compiler versions of different types: '{baseType.ToString()}' vs '{comparingType.ToString()}'")
            {
                this.BaseType = baseType;
                this.ComparingType = comparingType;
            }

            /// <summary>
            /// Get the underlying type of the toolchain version in use.
            /// </summary>
            public System.Type BaseType { get; private set; }

            /// <summary>
            /// Get the underlying type of the toolchain version to be compared against.
            /// </summary>
            public System.Type ComparingType { get; private set; }
        }

        /// <summary>
        /// A combination of all version numbers
        /// </summary>
        protected int combinedVersion;

        /// <summary>
        /// Are two toolchain versions identical?
        /// </summary>
        /// <param name="compare">Toolchain version to compare</param>
        /// <returns>true if identical</returns>
        public bool
        Match(
            ToolchainVersion compare) => this.combinedVersion == compare.combinedVersion;

        /// <summary>
        /// Is the current version at least as much (>=) the specified version?
        /// </summary>
        /// <param name="minimum">Speified minimum toolchain version.</param>
        /// <returns>true if current version >= specified minimum.</returns>
        public bool
        AtLeast(
            ToolchainVersion minimum)
        {
            if (this.GetType() != minimum.GetType())
            {
                throw new IncompatibleTypesException(this.GetType(), minimum.GetType());
            }
            return this.combinedVersion >= minimum.combinedVersion;
        }

        /// <summary>
        /// Is the current version at most (&lt;=) the specified version?
        /// </summary>
        /// <param name="maximum">Specified maximum toolchain version.</param>
        /// <returns>true if current version &lt;= specified maximum.</returns>
        public bool
        AtMost(
            ToolchainVersion maximum)
        {
            if (this.GetType() != maximum.GetType())
            {
                throw new IncompatibleTypesException(this.GetType(), maximum.GetType());
            }
            return this.combinedVersion <= maximum.combinedVersion;
        }

        /// <summary>
        /// Is the current version in the range [minimum, maximum]?
        /// </summary>
        /// <param name="minimum">Specified minimum toolchain version.</param>
        /// <param name="maximum">Specified maximum toolchain version.</param>
        /// <returns>true if the current version lies in the range [minimum,maximum].</returns>
        public bool
        InRange(
            ToolchainVersion minimum,
            ToolchainVersion maximum) => this.AtLeast(minimum) && this.AtMost(maximum);

        /// <summary>
        /// Convert the toolchain version to a human readable string.
        /// </summary>
        /// <returns>Version as string</returns>
        public override string
        ToString() => combinedVersion.ToString();
    }
}
