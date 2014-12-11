#region License
// Copyright 2010-2014 Mark Final
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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ILinkerOptions.cs&ILinkerOptions.cs
//     -n=MingwCommon
//     -c=LinkerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace MingwCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option delegates
        private static void
        OutputTypeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.ELinkerOutput>;
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                {
                    var outputPath = options.OwningNode.Module.Locations[C.Application.OutputFile].GetSinglePath();
                    commandLineBuilder.Add(System.String.Format("-o {0}", outputPath));
                    break;
                }
                case C.ELinkerOutput.DynamicLibrary:
                {
                    var outputPath = options.OwningNode.Module.Locations[C.Application.OutputFile].GetSinglePath();
                    commandLineBuilder.Add(System.String.Format("-o {0}", outputPath));
                    commandLineBuilder.Add("-shared");
                    var importLibraryFileLoc = options.GetModuleLocation(C.DynamicLibrary.ImportLibraryFile);
                    if (importLibraryFileLoc != null)
                    {
                        var importLibPath = options.OwningNode.Module.Locations[C.DynamicLibrary.ImportLibraryFile].GetSinglePath();
                        commandLineBuilder.Add(System.String.Format("-Wl,--out-implib,{0}", importLibPath));
                    }
                    break;
                }
                default:
                    throw new Bam.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static void
        DoNotAutoIncludeStandardLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoreStandardLibrariesOption = option as Bam.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
            }
        }
        private static void
        DebugSymbolsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var debugSymbolsOption = option as Bam.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }
        private static void
        SubSystemCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var subSystemOption = option as Bam.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;
                case C.ESubsystem.Console:
                    commandLineBuilder.Add("-Wl,--subsystem,console");
                    break;
                case C.ESubsystem.Windows:
                    commandLineBuilder.Add("-Wl,--subsystem,windows");
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static void
        LibraryPathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var libraryPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to var
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-L\"{0}\"", libraryPath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-L{0}", libraryPath));
                }
            }
        }
        private static void
        StandardLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            // TODO: change to var, and returning Locations
            foreach (Bam.Core.Location libraryPath in librariesOption.Value)
            {
                commandLineBuilder.Add(libraryPath.GetSinglePath());
            }
        }
        private static void
        LibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            // TODO: change to var, and returning Locations
            foreach (Bam.Core.Location libraryPath in librariesOption.Value)
            {
                commandLineBuilder.Add(libraryPath.GetSinglePath());
            }
        }
        private static void
        GenerateMapFileCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                var locationMap = (sender as LinkerOptionCollection).OwningNode.Module.Locations;
                var mapFileLoc = locationMap[C.Application.MapFile];
                commandLineBuilder.Add(System.String.Format("-Wl,-Map,{0}", mapFileLoc.GetSinglePath()));
            }
        }
        private static void
        AdditionalOptionsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void
        EnableAutoImportCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wl,--enable-auto-import");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor);
            // Property 'MajorVersion' is value only - no delegates
            // Property 'MinorVersion' is value only - no delegates
            // Property 'PatchVersion' is value only - no delegates
            this["EnableAutoImport"].PrivateData = new PrivateData(EnableAutoImportCommandLineProcessor);
        }
    }
}
