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
namespace ClangCommon
{
    public abstract class LinkerBase :
        C.LinkerTool
    {
        protected Bam.Core.TokenizedStringArray arguments = new Bam.Core.TokenizedStringArray();

        protected LinkerBase()
        {
            this.Macros.AddVerbatim("exeext", string.Empty);
            this.Macros.AddVerbatim("dynamicprefix", "lib");
            this.Macros.Add("dynamicext", Bam.Core.TokenizedString.Create(".$(MajorVersion).dylib", null, flags: Bam.Core.TokenizedString.EFlags.DeferredExpansion));
            this.Macros.AddVerbatim("pluginprefix", "lib");
            this.Macros.AddVerbatim("pluginext", ".dylib");

            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim(System.String.Format("--sdk {0}", clangMeta.SDK)));
        }

        private static string
        GetLPrefixLibraryName(
            string fullLibraryPath)
        {
            var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
            libName = libName.Substring(3); // trim off lib prefix
            return System.String.Format("-l{0}", libName);
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
                linker.Libraries.AddUnique(GetLPrefixLibraryName(libraryPath));

                linker.LibraryPaths.AddUnique(library.CreateTokenizedString("@dir($(0))", library.GeneratedPaths[C.StaticLibrary.Key]));
            }
            else if (library is C.IDynamicLibrary)
            {
                // TODO: @filenamenoext
                var libraryPath = library.GeneratedPaths[C.DynamicLibrary.Key].Parse();
                linker.Libraries.AddUnique(GetLPrefixLibraryName(libraryPath));

                linker.LibraryPaths.AddUnique(library.CreateTokenizedString("@dir($(0))", library.GeneratedPaths[C.DynamicLibrary.Key]));
            }
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.CreateVerbatim("xcrun");
            }
        }

        public override Bam.Core.TokenizedStringArray InitialArguments
        {
            get
            {
                return this.arguments;
            }
        }
    }

    [C.RegisterCLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
        {
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang"));
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Clang.LinkerSettings(module);
            return settings;
        }
    }

    [C.RegisterCxxLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCxxLinker("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
        {
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang++"));
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Clang.CxxLinkerSettings(module);
            return settings;
        }
    }
}
