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
namespace ClangCommon
{
    public abstract class LinkerBase :
        C.LinkerTool
    {
        /// <summary>
        /// List of arguments
        /// </summary>
        protected Bam.Core.TokenizedStringArray arguments = new Bam.Core.TokenizedStringArray();

        protected LinkerBase()
        {
            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            var discovery = clangMeta as C.IToolchainDiscovery;
            discovery.discover(null);
            this.Version = clangMeta.ToolchainVersion;
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim($"--sdk {clangMeta.SDK}"));

            this.Macros.AddVerbatim("exeext", string.Empty);
            this.Macros.AddVerbatim("dynamicprefix", "lib");
            this.Macros.AddVerbatim("dynamicextonly", ".dylib");
            this.Macros.Add("dynamicext", Bam.Core.TokenizedString.Create(".$(MajorVersion)$(dynamicextonly)", null));
            this.Macros.AddVerbatim("pluginprefix", "lib");
            this.Macros.AddVerbatim("pluginext", ".dylib");
        }

        private static string
        GetLPrefixLibraryName(
            string fullLibraryPath)
        {
            var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
            libName = libName.Substring(3); // trim off lib prefix
            return $"-l{libName}";
        }

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
                return library.GeneratedPaths[C.DynamicLibrary.ExecutableKey];
            }
            else if ((library is C.CSDKModule) ||
                     (library is C.HeaderLibrary) ||
                     (library is C.OSXFramework))
            {
                return null;
            }
            throw new Bam.Core.Exception(
                $"Unsupported library type, {library.GetType().ToString()}"
            );
        }

        public override void
        ProcessLibraryDependency(
            C.CModule executable,
            C.CModule library)
        {
            var linker = executable.Settings as C.ICommonLinkerSettings;
            if (library is C.StaticLibrary)
            {
                // TODO: @filenamenoext
                var libraryPath = library.GeneratedPaths[C.StaticLibrary.LibraryKey].ToString();
                linker.Libraries.AddUnique(GetLPrefixLibraryName(libraryPath));

                foreach (var dir in library.OutputDirectories)
                {
                    linker.LibraryPaths.AddUnique(dir);
                }
            }
            else if (library is C.IDynamicLibrary)
            {
                // TODO: @filenamenoext
                var libraryPath = library.GeneratedPaths[C.DynamicLibrary.ExecutableKey].ToString();
                linker.Libraries.AddUnique(GetLPrefixLibraryName(libraryPath));

                foreach (var dir in library.OutputDirectories)
                {
                    linker.LibraryPaths.AddUnique(dir);
                }
            }
        }

        /// <summary>
        /// Executable path to the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(ConfigureUtilities.XcrunPath);

        /// <summary>
        /// Arguments to pass to the tool prior to Module settings
        /// </summary>
        public override Bam.Core.TokenizedStringArray InitialArguments => this.arguments;
    }

    [C.RegisterCLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker() => this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang"));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Clang.CLinkerSettings(module);
    }

    [C.RegisterCxxLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCxxLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx() => this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang++"));

        /// <summary>
        /// Create the default settings for the specified module.
        /// </summary>
        /// <typeparam name="T">Module type</typeparam>
        /// <param name="module">Module to create settings for</param>
        /// <returns>New settings instance</returns>
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new Clang.CxxLinkerSettings(module);
    }
}
