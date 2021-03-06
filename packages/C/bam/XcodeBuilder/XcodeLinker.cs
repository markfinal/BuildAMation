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
    /// Utility class offering support for Xcode project generation
    /// </summary>
    static partial class XcodeSupport
    {
        /// <summary>
        /// Create an Xcode Target for a linked executable.
        /// </summary>
        /// <param name="module">The linked executable</param>
        public static void
        Link(
            ConsoleApplication module)
        {
            Bam.Core.TokenizedString productName;
            if (module is IDynamicLibrary && !((module is Plugin) || (module is C.Cxx.Plugin)))
            {
                if (module.Macros[Bam.Core.ModuleMacroNames.OutputName].ToString().Equals(module.Macros[Bam.Core.ModuleMacroNames.ModuleName].ToString(), System.StringComparison.Ordinal))
                {
                    productName = module.CreateTokenizedString("${TARGET_NAME}.$(MajorVersion)");

                }
                else
                {
                    productName = module.CreateTokenizedString("$(OutputName).$(MajorVersion)");
                }
            }
            else
            {
                if (module.Macros[Bam.Core.ModuleMacroNames.OutputName].ToString().Equals(module.Macros[Bam.Core.ModuleMacroNames.ModuleName].ToString(), System.StringComparison.Ordinal))
                {
                    productName = Bam.Core.TokenizedString.CreateVerbatim("${TARGET_NAME}");
                }
                else
                {
                    productName = module.Macros[Bam.Core.ModuleMacroNames.OutputName];
                }
            }

            XcodeBuilder.FileReference.EFileType fileType;
            XcodeBuilder.Target.EProductType productType;

            if (module is IDynamicLibrary)
            {
                fileType = XcodeBuilder.FileReference.EFileType.DynamicLibrary;
                productType = XcodeBuilder.Target.EProductType.DynamicLibrary;
            }
            else
            {
                fileType = XcodeBuilder.FileReference.EFileType.Executable;
                productType = XcodeBuilder.Target.EProductType.Executable;
            }

            LinkOrArchive(
                out XcodeBuilder.Target target,
                out XcodeBuilder.Configuration configuration,
                module,
                fileType,
                productType,
                productName,
                module.GeneratedPaths[C.ConsoleApplication.ExecutableKey],
                module.HeaderFiles
            );
            if (null == target)
            {
                return;
            }

            ProcessLibraryDependencies(
                module,
                target
            );

            // convert link settings to the Xcode project
            XcodeProjectProcessor.XcodeConversion.Convert(
                module.Settings,
                module,
                configuration
            );

            // add order only dependents
            AddOrderOnlyDependentProjects(
                module,
                target
            );
        }
    }
}
