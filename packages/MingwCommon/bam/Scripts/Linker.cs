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
namespace MingwCommon
{
    public abstract class LinkerBase :
        C.LinkerTool
    {
        protected LinkerBase()
        {
            var mingwPackage = Bam.Core.Graph.Instance.Packages.Where(item => item.Name == "Mingw").First();
            var suffix = mingwPackage.MetaData["ToolSuffix"] as string;
            this.Macros.Add("LinkerSuffix", suffix);

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("BinPath", Bam.Core.TokenizedString.Create(@"$(InstallPath)\bin", this));
            this.Macros.Add("exeext", ".exe");
            this.Macros.Add("dynamicprefix", "lib");
            this.Macros.Add("dynamicext", ".so");
            this.Macros.Add("libprefix", "lib");
            this.Macros.Add("libext", ".a");

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

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Mingw.LinkerSettings(module);
            return settings;
        }

        private static string
        GetLibraryPath(
            Bam.Core.Module module)
        {
            if (module is C.StaticLibrary)
            {
                return module.GeneratedPaths[C.StaticLibrary.Key].ToString();
            }
            else if (module is C.DynamicLibrary)
            {
                return module.GeneratedPaths[C.DynamicLibrary.ImportLibraryKey].ToString();
            }
            else if (module is C.CSDKModule)
            {
                // collection of libraries, none in particular
                return null;
            }
            else if (module is C.HeaderLibrary)
            {
                // no library
                return null;
            }
            else if (module is C.ExternalFramework)
            {
                // dealt with elsewhere
                return null;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
            }
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
            var fullLibraryPath = GetLibraryPath(library);
            if (null == fullLibraryPath)
            {
                return;
            }
            var dir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(fullLibraryPath), null);
            var libFilename = GetLPrefixLibraryName(fullLibraryPath);
            var linker = executable.Settings as C.ICommonLinkerSettings;
            linker.Libraries.AddUnique(libFilename);
            linker.LibraryPaths.AddUnique(dir);
        }
    }

    [C.RegisterCLinker("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
        {
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.Create(@"$(BinPath)\mingw32-gcc$(LinkerSuffix).exe", this));
        }
    }

    [C.RegisterCxxLinker("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
        {
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.Create(@"$(BinPath)\mingw32-g++.exe", this));
        }
    }
}
