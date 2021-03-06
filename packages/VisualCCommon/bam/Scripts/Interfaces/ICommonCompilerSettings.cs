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
namespace VisualCCommon
{
    /// <summary>
    /// VisualC specific settings for the compiler
    /// </summary>
    [Bam.Core.SettingsExtensions(typeof(VisualCCommon.DefaultSettings.DefaultSettingsExtensions))]
    [Bam.Core.SettingsPrecedence(System.Int32.MaxValue)] // warning settings must come before warning suppressions
    interface ICommonCompilerSettings :
        Bam.Core.ISettingsBase
    {
        /// <summary>
        /// Get or set whether the logo is displayed
        /// </summary>
        bool? NoLogo { get; set; }

        /// <summary>
        /// Get or set the runtime library to use.
        /// </summary>
        ERuntimeLibrary? RuntimeLibrary { get; set; }

        /// <summary>
        /// Get or set the warning level
        /// </summary>
        VisualCCommon.EWarningLevel? WarningLevel { get; set; }

        /// <summary>
        /// Get or set whether to use language extensions
        /// </summary>
        bool? EnableLanguageExtensions { get; set; }

        /// <summary>
        /// Get or set the optimisation level
        /// </summary>
        EOptimization? Optimization { get; set; }

        /// <summary>
        /// Get or set whether the object file section count should be increased
        /// </summary>
        bool? IncreaseObjectFileSectionCount { get; set; }

        /// <summary>
        /// Get or set whether whole program optimisation is used
        /// </summary>
        bool? WholeProgramOptimization { get; set; }
    }
}
