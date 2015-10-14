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
using System.Linq;
namespace GccCommon
{
    public abstract class LinkerBase :
        C.LinkerTool
    {
        protected LinkerBase()
        {
            var gccPackage = Bam.Core.Graph.Instance.Packages.Where(item => item.Name == "Gcc").First();
            this.GccMetaData = gccPackage.MetaData as Gcc.MetaData;

            var ldPath = this.GccMetaData.LdPath;
            var installPath = Bam.Core.TokenizedString.CreateVerbatim(System.IO.Path.GetDirectoryName(ldPath));
            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(installPath));

            this.Macros.AddVerbatim("exeext", string.Empty);
            this.Macros.AddVerbatim("dynamicprefix", "lib");
            // TODO: should be able to build these up cumulatively, but the deferred expansion only
            // works for a single depth (up to the Module using this Tool) so this needs looking into
            this.Macros.AddVerbatim("linkernameext", ".so");
            this.Macros.Add("sonameext", Bam.Core.TokenizedString.Create(".so.$(MajorVersion)", null, flags: Bam.Core.TokenizedString.EFlags.DeferredExpansion));
            this.Macros.Add("dynamicext", Bam.Core.TokenizedString.Create(".so.$(MajorVersion).$(MinorVersion)", null, flags: Bam.Core.TokenizedString.EFlags.DeferredExpansion));
            this.Macros.AddVerbatim("pluginprefix", "lib");
            this.Macros.AddVerbatim("pluginext", ".so");
        }

        protected Gcc.MetaData GccMetaData
        {
            get;
            private set;
        }

        private static string
        GetLPrefixLibraryName(
            string fullLibraryPath)
        {
            var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
            libName = libName.Substring(3); // trim off lib prefix
            return System.String.Format("-l{0}", libName);
        }

        private static Bam.Core.Array<C.CModule>
        FindAllDynamicDependents(
            C.IDynamicLibrary dynamicModule)
        {
            var dynamicDeps = new Bam.Core.Array<C.CModule>();
            if (0 == (dynamicModule as C.CModule).Dependents.Count)
            {
                return dynamicDeps;
            }

            foreach (var dep in (dynamicModule as C.CModule).Dependents)
            {
                if (!(dep is C.IDynamicLibrary))
                {
                    continue;
                }
                var dynDep = dep as C.CModule;
                dynamicDeps.AddUnique(dynDep);
                dynamicDeps.AddRangeUnique(FindAllDynamicDependents(dynDep as C.IDynamicLibrary));
            }
            return dynamicDeps;
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
                var libraryPath = library.GeneratedPaths[C.StaticLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                linker.LibraryPaths.AddUnique(library.CreateTokenizedString("@dir($(0))", library.GeneratedPaths[C.StaticLibrary.Key]));
            }
            else if (library is C.IDynamicLibrary)
            {
                // TODO: @filenamenoext
                var libraryPath = library.GeneratedPaths[C.DynamicLibrary.Key].Parse();
                var libraryName = library.Macros.Contains("LinkerName") ?
                    GetLPrefixLibraryName(library.Macros["LinkerName"].Parse()) :
                    GetLPrefixLibraryName(libraryPath);
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                linker.LibraryPaths.AddUnique(library.CreateTokenizedString("@dir($(0))", library.GeneratedPaths[C.DynamicLibrary.Key]));

                var gccLinker = executable.Settings as GccCommon.ICommonLinkerSettings;
                var allDynamicDependents = FindAllDynamicDependents(library as C.IDynamicLibrary);
                foreach (var dep in allDynamicDependents)
                {
                    gccLinker.RPathLink.AddUnique(dep.CreateTokenizedString("@dir($(0))", dep.GeneratedPaths[C.DynamicLibrary.Key]).Parse());
                }
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Gcc.LinkerSettings(module);
            return settings;
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["LinkerPath"];
            }
        }
    }

    [C.RegisterCLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
        {
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GccPath));
        }
    }

    [C.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
        {
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.CreateVerbatim(this.GccMetaData.GxxPath));
        }
    }
}
