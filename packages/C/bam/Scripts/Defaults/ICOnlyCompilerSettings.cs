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
    /// Extension class for handling defaults and operations for C.ICOnlyCompilerSettings
    /// </summary>
    public static partial class DefaultSettingsExtensions
    {
        /// <summary>
        /// Set default property values of C.ICOnlyCompilerSettings
        /// </summary>
        /// <param name="settings">C.ICOnlyCompilerSettings instance</param>
        public static void
        Defaults(
            this C.ICOnlyCompilerSettings settings)
        {
            settings.LanguageStandard = ELanguageStandard.C89;
        }

        /// <summary>
        /// Intersection of two C.ICOnlyCompilerSettings
        /// </summary>
        /// <param name="shared">C.ICOnlyCompilerSettings instance for the shared properties</param>
        /// <param name="other">C.ICOnlyCompilerSettings instance to intersect with</param>
        public static void
        Intersect(
            this C.ICOnlyCompilerSettings shared,
            C.ICOnlyCompilerSettings other)
        {
            shared.LanguageStandard = shared.LanguageStandard.Intersect(other.LanguageStandard);
        }

        /// <summary>
        /// Delta between two C.ICOnlyCompilerSettings
        /// </summary>
        /// <param name="delta">C.ICOnlyCompilerSettings to write the delta to</param>
        /// <param name="lhs">C.ICOnlyCompilerSettings to diff against</param>
        /// <param name="rhs">C.ICOnlyCompilerSettings to diff with</param>
        public static void
        Delta(
            this C.ICOnlyCompilerSettings delta,
            C.ICOnlyCompilerSettings lhs,
            C.ICOnlyCompilerSettings rhs)
        {
            delta.LanguageStandard = lhs.LanguageStandard.Complement(rhs.LanguageStandard);
        }

        /// <summary>
        /// Clone a C.ICOnlyCompilerSettings
        /// </summary>
        /// <param name="settings">C.ICOnlyCompilerSettings containing the cloned properties</param>
        /// <param name="other">Source C.ICOnlyCompilerSettings</param>
        public static void
        Clone(
            this C.ICOnlyCompilerSettings settings,
            C.ICOnlyCompilerSettings other)
        {
            settings.LanguageStandard = other.LanguageStandard;
        }
    }
}
