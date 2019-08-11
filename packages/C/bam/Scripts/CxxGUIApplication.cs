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
using Bam.Core;
namespace C.Cxx
{
    /// <summary>
    /// Derive from this module to create a GUI application linking against the C++ runtime.
    /// </summary>
    public class GUIApplication :
        ConsoleApplication
    {
        /// <summary>
        /// Initialize the GUI application
        /// </summary>
        /// <param name="parent">From this parent</param>
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Linker = C.DefaultToolchain.Cxx_Linker(this.BitDepth);

            this.PrivatePatch(settings =>
            {
                if (settings is C.ICommonLinkerSettingsWin linker)
                {
                    linker.SubSystem = ESubsystem.Windows;
                }
            });
        }

        /// <summary>
        /// Private patch delegate for indicating a Windowed application
        /// </summary>
        protected Bam.Core.Module.PrivatePatchDelegate WindowsPreprocessor = settings =>
        {
            var preprocessor = settings as C.ICommonPreprocessorSettings;
            preprocessor.PreprocessorDefines.Remove("_CONSOLE");
            preprocessor.PreprocessorDefines.Add("_WINDOWS");
        };

        /// <summary>
        /// Create a container whose matching source files compile against C.
        /// </summary>
        /// <returns>The C source container.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public sealed override C.CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var container = base.CreateCSourceContainer(wildcardPath, macroModuleOverride, filter);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                container.PrivatePatch(this.WindowsPreprocessor);
            }
            return container;
        }

        /// <summary>
        /// Create a container whose matching source files compile against C++>
        /// </summary>
        /// <returns>The cxx source container.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public sealed override Cxx.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var container = base.CreateCxxSourceContainer(wildcardPath, macroModuleOverride, filter);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                container.PrivatePatch(this.WindowsPreprocessor);
            }
            return container;
        }
    }
}
