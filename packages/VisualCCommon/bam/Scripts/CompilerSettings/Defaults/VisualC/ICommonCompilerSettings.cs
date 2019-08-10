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
namespace VisualCCommon.DefaultSettings
{
    /// <summary>
    /// Extension class for default settings to ICommonCompilerSettings
    /// </summary>
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this VisualCCommon.ICommonCompilerSettings settings,
            Bam.Core.Module module)
        {
            settings.NoLogo = true;
            settings.RuntimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC").RuntimeLibrary;
            settings.WarningLevel = EWarningLevel.Level1;
            settings.EnableLanguageExtensions = true; // only because Windows.h does not compile without this, even with WIN32_LEAN_AND_MEAN
            settings.Optimization = null; // assume that the setting in C.ICommonCompilerSettings is sufficient
            settings.IncreaseObjectFileSectionCount = false; // don't use large section counts by default
            settings.WholeProgramOptimization = false;
        }

        public static void
        Intersect(
            this VisualCCommon.ICommonCompilerSettings shared,
            VisualCCommon.ICommonCompilerSettings other)
        {
            shared.NoLogo = shared.NoLogo.Intersect(other.NoLogo);
            shared.RuntimeLibrary = shared.RuntimeLibrary.Intersect(other.RuntimeLibrary);
            shared.WarningLevel = shared.WarningLevel.Intersect(other.WarningLevel);
            shared.EnableLanguageExtensions = shared.EnableLanguageExtensions.Intersect(other.EnableLanguageExtensions);
            shared.Optimization = shared.Optimization.Intersect(other.Optimization);
            shared.IncreaseObjectFileSectionCount = shared.IncreaseObjectFileSectionCount.Intersect(other.IncreaseObjectFileSectionCount);
            shared.WholeProgramOptimization = shared.WholeProgramOptimization.Intersect(other.WholeProgramOptimization);
        }

        public static void
        Delta(
            this VisualCCommon.ICommonCompilerSettings delta,
            VisualCCommon.ICommonCompilerSettings lhs,
            VisualCCommon.ICommonCompilerSettings rhs)
        {
            delta.NoLogo = lhs.NoLogo.Complement(rhs.NoLogo);
            delta.RuntimeLibrary = lhs.RuntimeLibrary.Complement(rhs.RuntimeLibrary);
            delta.WarningLevel = lhs.WarningLevel.Complement(rhs.WarningLevel);
            delta.EnableLanguageExtensions = lhs.EnableLanguageExtensions.Complement(rhs.EnableLanguageExtensions);
            delta.Optimization = lhs.Optimization.Complement(rhs.Optimization);
            delta.IncreaseObjectFileSectionCount = lhs.IncreaseObjectFileSectionCount.Complement(rhs.IncreaseObjectFileSectionCount);
            delta.WholeProgramOptimization = lhs.WholeProgramOptimization.Complement(rhs.WholeProgramOptimization);
        }

        public static void
        Clone(
            this VisualCCommon.ICommonCompilerSettings settings,
            VisualCCommon.ICommonCompilerSettings other)
        {
            settings.NoLogo = other.NoLogo;
            settings.RuntimeLibrary = other.RuntimeLibrary;
            settings.WarningLevel = other.WarningLevel;
            settings.EnableLanguageExtensions = other.EnableLanguageExtensions;
            settings.Optimization = other.Optimization;
            settings.IncreaseObjectFileSectionCount = other.IncreaseObjectFileSectionCount;
            settings.WholeProgramOptimization = other.WholeProgramOptimization;
        }
    }
}
