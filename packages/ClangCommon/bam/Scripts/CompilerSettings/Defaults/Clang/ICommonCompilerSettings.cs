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
using C;
namespace ClangCommon.DefaultSettings
{
    /// <summary>
    /// Extension class for setting Clang common compiler setting defaults
    /// </summary>
    static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this ClangCommon.ICommonCompilerSettings settings)
        {
            settings.AllWarnings = false;
            settings.ExtraWarnings = false;
            settings.Pedantic = false;
            settings.Visibility = EVisibility.Hidden;
            var module = (settings as Bam.Core.Settings).Module;
            settings.StrictAliasing = (0 != (module.BuildEnvironment.Configuration & Bam.Core.EConfiguration.NotDebug));
            settings.Optimization = null; // assume that the setting in C.ICommonCompilerSettings is sufficient
        }

        public static void
        Intersect(
            this ClangCommon.ICommonCompilerSettings shared,
            ClangCommon.ICommonCompilerSettings other)
        {
            shared.AllWarnings = shared.AllWarnings.Intersect(other.AllWarnings);
            shared.ExtraWarnings = shared.ExtraWarnings.Intersect(other.ExtraWarnings);
            shared.Pedantic = shared.Pedantic.Intersect(other.Pedantic);
            shared.Visibility = shared.Visibility.Intersect(other.Visibility);
            shared.StrictAliasing = shared.StrictAliasing.Intersect(other.StrictAliasing);
            shared.Optimization = shared.Optimization.Intersect(other.Optimization);
        }

        public static void
        Delta(
            this ClangCommon.ICommonCompilerSettings delta,
            ClangCommon.ICommonCompilerSettings lhs,
            ClangCommon.ICommonCompilerSettings rhs)
        {
            delta.AllWarnings = lhs.AllWarnings.Complement(rhs.AllWarnings);
            delta.ExtraWarnings = lhs.ExtraWarnings.Complement(rhs.ExtraWarnings);
            delta.Pedantic = lhs.Pedantic.Complement(rhs.Pedantic);
            delta.Visibility = lhs.Visibility.Complement(rhs.Visibility);
            delta.StrictAliasing = lhs.StrictAliasing.Complement(rhs.StrictAliasing);
            delta.Optimization = lhs.Optimization.Complement(rhs.Optimization);
        }

        public static void
        Clone(
            this ClangCommon.ICommonCompilerSettings settings,
            ClangCommon.ICommonCompilerSettings other)
        {
            settings.AllWarnings = other.AllWarnings;
            settings.ExtraWarnings = other.ExtraWarnings;
            settings.Pedantic = other.Pedantic;
            settings.Visibility = other.Visibility;
            settings.StrictAliasing = other.StrictAliasing;
            settings.Optimization = other.Optimization;
        }
    }
}
