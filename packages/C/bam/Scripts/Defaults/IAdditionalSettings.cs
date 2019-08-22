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
    /// Extension class for handling defaults and operations for C.IAdditionalSettings
    /// </summary>
    public static partial class DefaultSettingsExtensions
    {
        /// <summary>
        /// Set default property values of C.IAdditionalSettings
        /// </summary>
        /// <param name="settings">C.IAdditionalSettings instance</param>
        /// <param name="module">Module associated with Settings</param>
        public static void
        Defaults(
            this C.IAdditionalSettings settings,
            Bam.Core.Module module)
        {
            settings.AdditionalSettings = new Bam.Core.StringArray();
        }

        /// <summary>
        /// Intersection of two C.IAdditionalSettings
        /// </summary>
        /// <param name="shared">C.IAdditionalSettings instance for the shared properties</param>
        /// <param name="other">C.IAdditionalSettings instance to intersect with</param>
        public static void
        Intersect(
            this C.IAdditionalSettings shared,
            C.IAdditionalSettings other)
        {
            shared.AdditionalSettings = shared.AdditionalSettings.Intersect(other.AdditionalSettings);
        }

        /// <summary>
        /// Delta between two C.IAdditionalSettings
        /// </summary>
        /// <param name="delta">C.IAdditionalSettings to write the delta to</param>
        /// <param name="lhs">C.IAdditionalSettings to diff against</param>
        /// <param name="rhs">C.IAdditionalSettings to diff with</param>
        public static void
        Delta(
            this C.IAdditionalSettings delta,
            C.IAdditionalSettings lhs,
            C.IAdditionalSettings rhs)
        {
            delta.AdditionalSettings = lhs.AdditionalSettings.Complement(rhs.AdditionalSettings);
        }

        /// <summary>
        /// Clone a C.IAdditionalSettings
        /// </summary>
        /// <param name="settings">C.IAdditionalSettings containing the cloned properties</param>
        /// <param name="other">Source C.IAdditionalSettings</param>
        public static void
        Clone(
            this C.IAdditionalSettings settings,
            C.IAdditionalSettings other)
        {
            settings.AdditionalSettings = new Bam.Core.StringArray();
            foreach (var path in other.AdditionalSettings)
            {
                settings.AdditionalSettings.AddUnique(path);
            }
        }
    }
}
