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
namespace Gcc
{
    public abstract class LinkerBase :
        C.LinkerTool
    {
        public LinkerBase(
            string executable)
        {
            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(Bam.Core.TokenizedString.Create(@"$(InstallPath)", this)));

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("exeext", string.Empty);
            this.Macros.Add("dynamicprefix", "lib");
            this.Macros.Add("dynamicext", ".so");
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/" + executable, this));
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
            C.DynamicLibrary dynamicModule)
        {
            var dynamicDeps = new Bam.Core.Array<C.CModule>();
            if (0 == dynamicModule.Dependents.Count)
            {
                return dynamicDeps;
            }

            foreach (var dep in dynamicModule.Dependents)
            {
                if (!(dep is C.DynamicLibrary))
                {
                    continue;
                }
                var dynDep = dep as C.DynamicLibrary;
                dynamicDeps.AddUnique(dynDep);
                dynamicDeps.AddRangeUnique(FindAllDynamicDependents(dynDep));
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
                var libraryPath = library.GeneratedPaths[C.StaticLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                var libraryDir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(libraryPath), null);
                linker.LibraryPaths.AddUnique(libraryDir);
            }
            else if (library is C.DynamicLibrary)
            {
                var libraryPath = library.GeneratedPaths[C.DynamicLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                var libraryDir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(libraryPath), null);
                linker.LibraryPaths.AddUnique(libraryDir);

                var gccLinker = executable.Settings as GccCommon.ICommonLinkerSettings;
                var allDynamicDependents = FindAllDynamicDependents(library as C.DynamicLibrary);
                foreach (var dep in allDynamicDependents)
                {
                    var depLibraryPath = dep.GeneratedPaths[C.DynamicLibrary.Key].Parse();
                    var depLibraryDir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(depLibraryPath), null);
                    gccLinker.RPathLink.AddUnique(depLibraryDir.Parse());
                }
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new LinkerSettings(module);
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
            : base("gcc-4.8")
        {}
    }

    [C.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
            : base("g++-4.8")
        {}
    }
}
