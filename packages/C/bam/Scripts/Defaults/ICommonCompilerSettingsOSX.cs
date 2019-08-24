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
namespace C.DefaultSettings
{
    /// <summary>
    /// Extension class for handling defaults and operations for C.ICommonCompilerSettingsOSX
    /// </summary>
    public static partial class DefaultSettingsExtensions
    {
        /// <summary>
        /// Set default property values of C.ICommonCompilerSettingsOSX
        /// </summary>
        /// <param name="settings">C.ICommonCompilerSettingsOSX instance</param>
        public static void
        Defaults(
            this C.ICommonCompilerSettingsOSX settings)
        {
            // N.B. this default is set in the specific Clang version
            // as it's set on both compiler and linker
            settings.FrameworkSearchPaths = new Bam.Core.TokenizedStringArray();
        }

        /// <summary>
        /// Intersection of two C.ICommonCompilerSettingsOSX
        /// </summary>
        /// <param name="shared">C.ICommonCompilerSettingsOSX instance for the shared properties</param>
        /// <param name="other">C.ICommonCompilerSettingsOSX instance to intersect with</param>
        public static void
        Intersect(
            this C.ICommonCompilerSettingsOSX shared,
            C.ICommonCompilerSettingsOSX other)
        {
            shared.FrameworkSearchPaths = shared.FrameworkSearchPaths.Intersect(other.FrameworkSearchPaths);
            shared.MacOSXMinimumVersionSupported = shared.MacOSXMinimumVersionSupported.Intersect(other.MacOSXMinimumVersionSupported);
        }

        /// <summary>
        /// Delta between two C.ICommonCompilerSettingsOSX
        /// </summary>
        /// <param name="delta">C.ICommonCompilerSettingsOSX to write the delta to</param>
        /// <param name="lhs">C.ICommonCompilerSettingsOSX to diff against</param>
        /// <param name="rhs">C.ICommonCompilerSettingsOSX to diff with</param>
        public static void
        Delta(
            this C.ICommonCompilerSettingsOSX delta,
            C.ICommonCompilerSettingsOSX lhs,
            C.ICommonCompilerSettingsOSX rhs)
        {
            delta.FrameworkSearchPaths = lhs.FrameworkSearchPaths.Complement(rhs.FrameworkSearchPaths);
            delta.MacOSXMinimumVersionSupported = lhs.MacOSXMinimumVersionSupported.Complement(rhs.MacOSXMinimumVersionSupported);
        }

        /// <summary>
        /// Clone a C.ICommonCompilerSettingsOSX
        /// </summary>
        /// <param name="settings">C.ICommonCompilerSettingsOSX containing the cloned properties</param>
        /// <param name="other">Source C.ICommonCompilerSettingsOSX</param>
        public static void
        Clone(
            this C.ICommonCompilerSettingsOSX settings,
            C.ICommonCompilerSettingsOSX other)
        {
            settings.FrameworkSearchPaths = new Bam.Core.TokenizedStringArray();
            foreach (var path in other.FrameworkSearchPaths)
            {
                settings.FrameworkSearchPaths.AddUnique(path);
            }
            settings.MacOSXMinimumVersionSupported = other.MacOSXMinimumVersionSupported.Clone() as string;
        }
    }
}
