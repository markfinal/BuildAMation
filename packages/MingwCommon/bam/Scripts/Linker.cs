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
namespace MingwCommon
{
    /// <summary>
    /// Abstract base class for Mingw linkers
    /// </summary>
    abstract class LinkerBase :
        C.LinkerTool
    {
        protected LinkerBase()
        {
            var mingwMeta = Bam.Core.Graph.Instance.PackageMetaData<Mingw.MetaData>("Mingw");
            var discovery = mingwMeta as C.IToolchainDiscovery;
            discovery.Discover(depth: null);

            this.Macros.AddVerbatim("LinkerSuffix", mingwMeta.ToolSuffix);

            this.Macros.Add("BinPath", this.CreateTokenizedString(@"$(0)\bin", mingwMeta["InstallDir"] as Bam.Core.TokenizedString));
            this.Macros.AddVerbatim(C.ModuleMacroNames.ExecutableFileExtension, ".exe");
            this.Macros.AddVerbatim(C.ModuleMacroNames.DynamicLibraryPrefix, "lib");
            this.Macros.AddVerbatim(C.ModuleMacroNames.DynamicLibraryFileExtension, ".dll");
            this.Macros.AddVerbatim(C.ModuleMacroNames.PluginPrefix, "lib");
            this.Macros.AddVerbatim(C.ModuleMacroNames.PluginFileExtension, ".dll");
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryPrefix, "lib");
            this.Macros.AddVerbatim(C.ModuleMacroNames.LibraryFileExtension, ".a");

            this.InheritedEnvironmentVariables.Add("TEMP");
            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(this.Macros.FromName("BinPath")));
        }

        /// <summary>
        /// Executable path to tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros.FromName("LinkerPath");

        /// <summary>
        /// Command line switch to identify response file
        /// </summary>
        public override string UseResponseFileOption => "@";

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(Mingw.LinkerSettings);

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

        private static string
        GetLPrefixLibraryName(
            string fullLibraryPath)
        {
            var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
            libName = libName.Substring(3); // trim off lib prefix
            return $"-l{libName}";
        }

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
            // TODO: use @filenamenoext
            lock (fullLibraryPath)
            {
                if (!fullLibraryPath.IsParsed)
                {
                    fullLibraryPath.Parse();
                }
            }
            var libFilename = GetLPrefixLibraryName(fullLibraryPath.ToString());
            var linker = executable.Settings as C.ICommonLinkerSettings;
            linker.Libraries.AddUnique(libFilename);
            foreach (var dir in library.OutputDirectories)
            {
                linker.LibraryPaths.AddUnique(dir);
            }
        }
    }

    /// <summary>
    /// 32-bit C linker
    /// </summary>
    [C.RegisterCLinker("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    sealed class Linker :
        LinkerBase
    {
        public Linker() => this.Macros.Add("LinkerPath", this.CreateTokenizedString(@"$(BinPath)\mingw32-gcc$(LinkerSuffix).exe"));
    }

    /// <summary>
    /// 32-bit C++ linkger
    /// </summary>
    [C.RegisterCxxLinker("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx() => this.Macros.Add("LinkerPath", this.CreateTokenizedString(@"$(BinPath)\mingw32-g++.exe"));
    }
}
