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
namespace VisualCCommon.DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this VisualCCommon.ICommonCompilerSettings settings,
            Bam.Core.Module module)
        {
            settings.NoLogo = true;
            settings.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;
            settings.WarningLevel = EWarningLevel.Level1;
        }

        public static void
        SharedSettings(
            this VisualCCommon.ICommonCompilerSettings shared,
            VisualCCommon.ICommonCompilerSettings lhs,
            VisualCCommon.ICommonCompilerSettings rhs)
        {
            shared.NoLogo = (lhs.NoLogo == rhs.NoLogo) ? lhs.NoLogo : null;
            shared.RuntimeLibrary = (lhs.RuntimeLibrary == rhs.RuntimeLibrary) ? lhs.RuntimeLibrary : null;
            shared.WarningLevel = (lhs.WarningLevel == rhs.WarningLevel) ? lhs.WarningLevel : null;
        }

        public static void
        Delta(
            this VisualCCommon.ICommonCompilerSettings delta,
            VisualCCommon.ICommonCompilerSettings lhs,
            VisualCCommon.ICommonCompilerSettings rhs)
        {
            delta.NoLogo = (lhs.NoLogo != rhs.NoLogo) ? lhs.NoLogo : null;
            delta.RuntimeLibrary = (lhs.RuntimeLibrary != rhs.RuntimeLibrary) ? lhs.RuntimeLibrary : null;
            delta.WarningLevel = (lhs.WarningLevel != rhs.WarningLevel) ? lhs.WarningLevel : null;
        }

        public static void
        Clone(
            this VisualCCommon.ICommonCompilerSettings settings,
            VisualCCommon.ICommonCompilerSettings other)
        {
            settings.NoLogo = other.NoLogo;
            settings.RuntimeLibrary = other.RuntimeLibrary;
            settings.WarningLevel = other.WarningLevel;
        }
    }
}
