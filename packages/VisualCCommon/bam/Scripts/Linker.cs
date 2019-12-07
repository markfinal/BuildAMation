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
namespace VisualCCommon
{
    /// <summary>
    /// Class representing any VisualC linker tool
    /// </summary>
    abstract class LinkerBase :
        C.LinkerTool
    {
        private string
        GetLinkerPath(
            C.EBit depth)
        {
            const string executable = "link.exe";
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                var installLocation = Bam.Core.OSUtilities.GetInstallLocation(
                    executable,
                    path.ToString(),
                    this.GetType().Name,
                    throwOnFailure: false
                );
                if (null != installLocation)
                {
                    return installLocation.First();
                }
            }
            var message = new System.Text.StringBuilder();
            message.AppendLine($"Unable to locate {executable} for {(int)depth}-bit on these search locations:");
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                message.AppendLine($"\t{path.ToString()}");
            }
            throw new Bam.Core.Exception(message.ToString());
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="depth">For the given bit-depth</param>
        protected LinkerBase(
            C.EBit depth)
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            var discovery = meta as C.IToolchainDiscovery;
            discovery.Discover(depth);
            this.Version = meta.ToolchainVersion;
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.EnvironmentVariables = meta.Environment(depth);
            var fullLinkExePath = this.GetLinkerPath(depth);
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.CreateVerbatim(fullLinkExePath));

            this.Macros.AddVerbatim(C.ModuleMacroNames.ExecutableFileExtension, ".exe");
            this.Macros.AddVerbatim(C.ModuleMacroNames.DynamicLibraryPrefix, string.Empty);
            this.Macros.AddVerbatim(C.ModuleMacroNames.DynamicLibraryFileExtension, ".dll");
            this.Macros.AddVerbatim(C.ModuleMacroNames.PluginPrefix, string.Empty);
            this.Macros.AddVerbatim(C.ModuleMacroNames.PluginFileExtension, ".dll");
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryPrefix, string.Empty);
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryFileExtension, ".lib");
            this.Macros.AddVerbatim("pdbext", ".pdb");

            this.InheritedEnvironmentVariables.Add("TEMP");
            this.InheritedEnvironmentVariables.Add("TMP");
        }

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(VisualC.LinkerSettings);

        /// <summary>
        /// Get the executable for the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["LinkerPath"];

        /// <summary>
        /// Get the command line option for response files
        /// </summary>
        public override string UseResponseFileOption => "@";

        /// <summary>
        /// Get the true library path for the given module.
        /// </summary>
        /// <param name="library">Module representing a library</param>
        /// <returns>True path for the library</returns>
        public override Bam.Core.TokenizedString
        GetLibraryPath(
            C.CModule library)
        {
            if (library is C.StaticLibrary)
            {
                return library.GeneratedPaths[C.StaticLibrary.LibraryKey];
            }
            else if (library is C.IDynamicLibrary)
            {
                return library.GeneratedPaths[C.DynamicLibrary.ImportLibraryKey];
            }
            else if ((library is C.CSDKModule) ||
                     (library is C.HeaderLibrary) ||
                     (library is C.OSXFramework))
            {
                return null;
            }
            throw new Bam.Core.Exception($"Unsupported library type, {library.GetType().ToString()}");
        }

        /// <summary>
        /// Process a library dependency on an executable
        /// </summary>
        /// <param name="executable">Executable being linked</param>
        /// <param name="library">Library to link against</param>
        public override void
        ProcessLibraryDependency(
            C.CModule executable,
            C.CModule library)
        {
            var fullLibraryPath = this.GetLibraryPath(library);
            if (null == fullLibraryPath)
            {
                return;
            }
            var linker = executable.Settings as C.ICommonLinkerSettings;
            var libFilename = library.CreateTokenizedString("@filename($(0))", fullLibraryPath);
            lock (libFilename)
            {
                if (!libFilename.IsParsed)
                {
                    libFilename.Parse();
                }
            }
            linker.Libraries.AddUnique(libFilename.ToString());
            var libDir = library.CreateTokenizedString("@dir($(0))", fullLibraryPath);
            linker.LibraryPaths.AddUnique(libDir);
        }
    }

    /// <summary>
    /// 32-bit VisualC linker
    /// </summary>
    [C.RegisterCLinker("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    [C.RegisterCxxLinker("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    sealed class Linker32 :
        LinkerBase
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public Linker32()
            :
            base(C.EBit.ThirtyTwo)
        {}
    }

    /// <summary>
    /// 64-bit VisualC linker
    /// </summary>
    [C.RegisterCLinker("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    [C.RegisterCxxLinker("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    sealed class Linker64 :
        LinkerBase
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public Linker64()
            :
            base(C.EBit.SixtyFour)
        {}
    }
}
