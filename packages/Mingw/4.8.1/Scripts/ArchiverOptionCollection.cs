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
        public static void Defaults(this IArchiverOptions settings, Bam.Core.V2.Module module)
        {
            settings.Ranlib = true;
            settings.DoNotWarnIfLibraryCreated = true;
            settings.Command = MingwCommon.EArchiverCommand.Replace;
        }
    }
}
    public interface IArchiverOptions
    {
        bool Ranlib
        {
            get;
            set;
        }

        bool DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

        MingwCommon.EArchiverCommand Command
        {
            get;
            set;
        }
    }

    public static partial class NativeImplementation
    {
        public static void Convert(this C.V2.ICommonArchiverOptions options, Bam.Core.V2.Module module)
        {
            var commandLine = module.CommandLine;
            var staticLibrary = module as C.V2.StaticLibrary;

            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    commandLine.Add(module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString());
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported output type");
            }
        }

        public static void Convert(this IArchiverOptions options, Bam.Core.V2.Module module)
        {
            var commandLine = module.CommandLine;
            if (options.Ranlib)
            {
                commandLine.Add("-s");
            }
            if (options.DoNotWarnIfLibraryCreated)
            {
                commandLine.Add("-c");
            }
            switch (options.Command)
            {
                case MingwCommon.EArchiverCommand.Replace:
                    commandLine.Add("-r");
                    break;

                default:
                    throw new Bam.Core.Exception("No such archiver command");
            }
        }
    }

    public class LibrarianSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonArchiverOptions,
        IArchiverOptions,
        CommandLineProcessor.V2.IConvertToCommandLine
    {
        public LibrarianSettings(Bam.Core.V2.Module module)
        {
            (this as C.V2.ICommonArchiverOptions).Defaults(module);
            (this as IArchiverOptions).Defaults(module);
        }

        C.EArchiverOutput C.V2.ICommonArchiverOptions.OutputType
        {
            get;
            set;
        }

        bool IArchiverOptions.Ranlib
        {
            get;
            set;
        }

        bool IArchiverOptions.DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

        MingwCommon.EArchiverCommand IArchiverOptions.Command
        {
            get;
            set;
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module)
        {
            (this as IArchiverOptions).Convert(module);
            // output file comes last, before inputs
            (this as C.V2.ICommonArchiverOptions).Convert(module);
        }
    }

    public sealed class Librarian :
        C.V2.LibrarianTool
    {
        public Librarian()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");
        }

        public override string Executable
        {
            get
            {
                return @"C:\MinGW\bin\ar.exe";
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LibrarianSettings(module);
            return settings;
        }
    }
}
    public class ArchiverOptionCollection :
        MingwCommon.ArchiverOptionCollection
    {
        public
        ArchiverOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
