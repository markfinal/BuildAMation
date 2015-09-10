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
using C.V2.DefaultSettings;
using Mingw.V2.DefaultSettings;
namespace Mingw
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this ILinkerOptions settings, Bam.Core.V2.Module module)
        {
        }
    }
}
    [Bam.Core.V2.SettingsExtensions(typeof(Mingw.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ILinkerOptions : Bam.Core.V2.ISettingsBase
    {
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonLinkerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            //var applicationFile = module as C.V2.ConsoleApplication;
            switch (options.OutputType)
            {
                case C.ELinkerOutput.Executable:
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.V2.ConsoleApplication.Key].ToString()));
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    commandLine.Add("-shared");
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.V2.ConsoleApplication.Key].ToString()));
                    commandLine.Add(System.String.Format("-Wl,--out-implib,{0}", module.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].ToString()));
                    break;
            }
            foreach (var path in options.LibraryPaths)
            {
                var format = path.ContainsSpace ? "-L\"{0}\"" : "-L{0}";
                commandLine.Add(System.String.Format(format, path.ToString()));
            }
            foreach (var path in options.Libraries)
            {
                commandLine.Add(path);
            }
            if (options.DebugSymbols.GetValueOrDefault())
            {
                commandLine.Add("-g");
            }
        }

        public static void
        Convert(
            this ILinkerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }
    }

    public sealed class LinkerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonLinkerOptions,
        ILinkerOptions
    {
        public LinkerSettings(Bam.Core.V2.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.V2.ICommonLinkerOptions).Defaults(module);
            (this as ILinkerOptions).Defaults(module);
#endif
        }

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonLinkerOptions).Convert(module, commandLine);
            (this as ILinkerOptions).Convert(module, commandLine);
        }

        C.ELinkerOutput C.V2.ICommonLinkerOptions.OutputType
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonLinkerOptions.LibraryPaths
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonLinkerOptions.Libraries
        {
            get;
            set;
        }

        bool? C.V2.ICommonLinkerOptions.DebugSymbols
        {
            get;
            set;
        }
    }

    public abstract class LinkerBase :
        C.V2.LinkerTool
    {
        public LinkerBase(
            string executable)
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("BinPath", Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\bin", this));
            this.Macros.Add("LinkerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\" + executable, this));
            this.Macros.Add("exeext", ".exe");
            this.Macros.Add("dynamicprefix", "lib");
            this.Macros.Add("dynamicext", ".so");
            this.Macros.Add("libprefix", "lib");
            this.Macros.Add("libext", ".a");

            this.InheritedEnvironmentVariables.Add("TEMP");
            this.EnvironmentVariables.Add("PATH", new Bam.Core.V2.TokenizedStringArray(this.Macros["BinPath"]));
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.Macros["LinkerPath"];
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LinkerSettings(module);
            return settings;
        }

        public override bool UseLPrefixLibraryPaths
        {
            get
            {
                return true;
            }
        }

        private static string
        GetLibraryPath(
            Bam.Core.V2.Module module)
        {
            if (module is C.V2.StaticLibrary)
            {
                return module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString();
            }
            else if (module is C.V2.DynamicLibrary)
            {
                return module.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].ToString();
            }
            else if (module is C.V2.CSDKModule)
            {
                // collection of libraries, none in particular
                return null;
            }
            else if (module is C.V2.HeaderLibrary)
            {
                // no library
                return null;
            }
            else if (module is C.V2.ExternalFramework)
            {
                // dealt with elsewhere
                return null;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
            }
        }

        public override void ProcessLibraryDependency(
            C.V2.CModule executable,
            C.V2.CModule library)
        {
            var fullLibraryPath = GetLibraryPath(library);
            if (null == fullLibraryPath)
            {
                return;
            }
            var dir = Bam.Core.V2.TokenizedString.Create(System.IO.Path.GetDirectoryName(fullLibraryPath), null);
            var libFilename = System.IO.Path.GetFileName(fullLibraryPath);
            var linker = executable.Settings as C.V2.ICommonLinkerOptions;
            linker.Libraries.AddUnique(libFilename);
            linker.LibraryPaths.AddUnique(dir);
        }
    }

    [C.V2.RegisterCLinker("Mingw", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
            : base("mingw32-gcc-4.8.1.exe")
        {}
    }

    [C.V2.RegisterCxxLinker("Mingw", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
            : base("mingw32-g++.exe")
        { }
    }
}
    public class LinkerOptionCollection :
        MingwCommon.LinkerOptionCollection
    {
        public
        LinkerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
