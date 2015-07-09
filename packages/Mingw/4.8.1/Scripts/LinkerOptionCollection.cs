#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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
    public interface ILinkerOptions
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
        Bam.Core.V2.Settings,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonLinkerOptions,
        ILinkerOptions
    {
        public LinkerSettings(Bam.Core.V2.Module module)
        {
            (this as C.V2.ICommonLinkerOptions).Defaults(module);
            (this as ILinkerOptions).Defaults(module);
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
    }

    [C.V2.RegisterCLinker("Mingw", Bam.Core.EPlatform.Windows)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
            : base("mingw32-gcc-4.8.1.exe")
        {}
    }

    [C.V2.RegisterCxxLinker("Mingw", Bam.Core.EPlatform.Windows)]
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
