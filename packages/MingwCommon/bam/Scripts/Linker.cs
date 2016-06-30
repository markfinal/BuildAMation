#region License
// Copyright (c) 2010-2016, Mark Final
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
    public abstract class LinkerBase :
        C.LinkerTool
    {
        protected LinkerBase()
        {
            var mingwMeta = Bam.Core.Graph.Instance.PackageMetaData<Mingw.MetaData>("Mingw");
            this.Macros.AddVerbatim("LinkerSuffix", mingwMeta.ToolSuffix);

            this.Macros.Add("BinPath", this.CreateTokenizedString(@"$(0)\bin", mingwMeta["InstallDir"] as Bam.Core.TokenizedString));
            this.Macros.AddVerbatim("exeext", ".exe");
            this.Macros.AddVerbatim("dynamicprefix", "lib");
            this.Macros.AddVerbatim("dynamicext", ".dll");
            this.Macros.AddVerbatim("pluginprefix", "lib");
            this.Macros.AddVerbatim("pluginext", ".dll");
            this.Macros.AddVerbatim("libprefix", "lib");
            this.Macros.AddVerbatim("libext", ".a");

            this.InheritedEnvironmentVariables.Add("TEMP");
            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(this.Macros["BinPath"]));
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["LinkerPath"];
            }
        }

        public override string UseResponseFileOption
        {
            get
            {
                return "@";
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Mingw.LinkerSettings(module);
            return settings;
        }

        public override Bam.Core.TokenizedString
        GetLibraryPath(
            C.CModule library)
        {
            if (library is C.StaticLibrary)
            {
                return library.GeneratedPaths[C.StaticLibrary.Key];
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
            throw new Bam.Core.Exception("Unsupported library type, {0}", library.GetType().ToString());
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
            var fullLibraryPath = this.GetLibraryPath(library);
            if (null == fullLibraryPath)
            {
                return;
            }
            // TODO: use @filenamenoext
            var libFilename = GetLPrefixLibraryName(fullLibraryPath.Parse());
            var linker = executable.Settings as C.ICommonLinkerSettings;
            linker.Libraries.AddUnique(libFilename);
            linker.LibraryPaths.AddUnique(library.CreateTokenizedString("@dir($(0))", fullLibraryPath));
        }
    }

    [C.RegisterCLinker("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
        {
            this.Macros.Add("LinkerPath", this.CreateTokenizedString(@"$(BinPath)\mingw32-gcc$(LinkerSuffix).exe"));
        }
    }

    [C.RegisterCxxLinker("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
        {
            this.Macros.Add("LinkerPath", this.CreateTokenizedString(@"$(BinPath)\mingw32-g++.exe"));
        }
    }
}
