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
namespace C
{
    /// <summary>
    /// Utility class offering support for Xcode project generation
    /// </summary>
    static class XcodeSharedSettings
    {
        /// <summary>
        /// Tweak the settings based on some criteria.
        /// This is because there are limitations of the Xcode project, so the Settings as
        /// presented will not generate a valid project, and need tweaking.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="assemblerSourcesPresent"></param>
        public static void
        Tweak(
            Bam.Core.Settings settings,
            bool assemblerSourcesPresent)
        {
            if (!(settings is ClangCommon.ICommonCompilerSettings))
            {
                throw new Bam.Core.Exception(
                    $"Compiler settings, {settings.GetType().ToString()}, do not implement the ClangCommon.ICommonCompilerSettings interface. Is this building on a platform other than macOS? Use --C.discoveralltoolchains --C.toolchain=Clang if so."
                );
            }
            // if Pedantic is variable among sources, and there are warning suppressions,
            // take the default of Pedantic as true
            // this is because the Xcode build setting for Pedantic appears BEFORE warning
            // suppressions, and if -Wpedantic appears in per-file overrides, this appears
            // AFTER warning suppressions, effectively undoing any that come under the
            // Pedantic umbrella
            if (null == (settings as ClangCommon.ICommonCompilerSettings).Pedantic &&
                (settings as ICommonCompilerSettings).DisableWarnings.Any())
            {
                (settings as ClangCommon.ICommonCompilerSettings).Pedantic = true;
            }

            // since assembly files share the 'common' settings, but are treated differently
            // any common setting must be removed for the C/C++/ObjC/ObjC++ code
            // otherwise the assembly is attempted to be used as that common target language
            if (assemblerSourcesPresent)
            {
                (settings as C.ICommonPreprocessorSettings).TargetLanguage = ETargetLanguage.Default;
            }
        }
    }
}
