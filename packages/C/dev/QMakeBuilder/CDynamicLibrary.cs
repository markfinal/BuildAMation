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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            C.DynamicLibrary moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.LinkerOptionCollection;

            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (var dependent in node.ExternalDependents)
                {
                    var depData = dependent.Data as QMakeData;
                    if (null != depData)
                    {
                        data.Merge(depData, QMakeData.OutputType.StaticLibrary | QMakeData.OutputType.DynamicLibrary | QMakeData.OutputType.HeaderLibrary);
                    }
                }
            }

            data.Target = options.OutputName;
            data.Output = QMakeData.OutputType.DynamicLibrary;
            data.DestDir = moduleToBuild.Locations[C.Application.OutputDir];

            // find dependent library files
            if (null != node.ExternalDependents)
            {
                var target = node.Target;
                var libraryKeysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey
                    );
                if (target.HasPlatform(Bam.Core.EPlatform.Posix))
                {
                    libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }

                var dependentLibraryFiles = new Bam.Core.LocationArray();
                node.ExternalDependents.FilterOutputLocations(libraryKeysToFilter, dependentLibraryFiles);
                data.Libraries.AddRangeUnique(dependentLibraryFiles);
            }

            var optionsInterface = moduleToBuild.Options as C.ILinkerOptions;

            // find static library files
            data.ExternalLibraries.AddRangeUnique(optionsInterface.Libraries.ToStringArray());

            // find library paths
            // TODO: convert to var, or Locations
            foreach (string libPath in optionsInterface.LibraryPaths)
            {
                if (libPath.Contains(" "))
                {
                    data.ExternalLibraries.Add("-L$$quote(" + libPath + ")");
                }
                else
                {
                    data.ExternalLibraries.Add("-L" + libPath);
                }
            }

            // find headers
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    data.Headers.AddRangeUnique(headerFileCollection.ToStringArray());
                }
            }

            if (optionsInterface is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineBuilder = new Bam.Core.StringArray();
                var target = node.Target;
                var commandLineOption = optionsInterface as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Bam.Core.StringArray();
                excludedOptionNames.Add("OutputType");
                excludedOptionNames.Add("LibraryPaths");
                excludedOptionNames.Add("GenerateMapFile"); // TODO: better way of extracting the map file?, yes locations
                excludedOptionNames.Add("DebugSymbols");
                excludedOptionNames.Add("DynamicLibrary"); // TODO: better way of extracting the import library?, yes locations
                if (target.HasPlatform(Bam.Core.EPlatform.NotWindows))
                {
                    excludedOptionNames.Add("RPath");
                }
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptionNames);
                data.LinkFlags.AddRangeUnique(commandLineBuilder);

                // library paths and libs
                {
                    var linkerOptions = options as C.ILinkerOptions;
                    foreach (var linkerSearchPath in linkerOptions.LibraryPaths)
                    {
                        // Note: the -L prefix is for all platforms
                        // Note: this is not the best way to use a DirectoryLocation
                        data.Libraries.AddUnique(Bam.Core.DirectoryLocation.Get(System.String.Format("-L{0}", linkerSearchPath), Bam.Core.Location.EExists.WillExist));
                    }
                    // TODO: convert to var
                    foreach (Bam.Core.FileLocation libraryPath in linkerOptions.Libraries)
                    {
                        data.Libraries.AddUnique(libraryPath);
                    }
                }

                // debug symbols
                {
                    var commandLine = new Bam.Core.StringArray();
                    var optionNames = new Bam.Core.StringArray("DebugSymbols");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(options, commandLine, target, optionNames);
                    if (!data.CustomPathVariables.ContainsKey("QMAKE_LFLAGS_DEBUG"))
                    {
                        data.CustomPathVariables["QMAKE_LFLAGS_DEBUG"] = new Bam.Core.StringArray();
                    }
                    foreach (var option in commandLine)
                    {
                        data.CustomPathVariables["QMAKE_LFLAGS_DEBUG"].AddUnique(option);
                    }
                }

                // rpath
                if (target.HasPlatform(Bam.Core.EPlatform.NotWindows))
                {
                    var commandLine = new Bam.Core.StringArray();
                    var optionNames = new Bam.Core.StringArray("RPath");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(options, commandLine, target, optionNames);
                    foreach (var option in commandLine)
                    {
                        var linkerCommand = option.Split(',');
                        var rpathDir = linkerCommand[linkerCommand.Length - 1];
                        // unable to insert $ORIGIN into QMAKE_RPATHDIR, that is suggested at
                        // http://www.opensource.apple.com/source/WebKit/WebKit-7534.56.5/qt/declarative/declarative.pro
                        // so use a workaround
                        rpathDir = rpathDir.Replace("$ORIGIN", "$$DESTDIR");
                        data.RPathDir.AddUnique(rpathDir);
                    }
                }
            }
            else
            {
                throw new Bam.Core.Exception("Linker options does not support command line translation");
            }

            if (node.Target.HasPlatform(Bam.Core.EPlatform.Posix))
            {
                data.VersionMajor = optionsInterface.MajorVersion.ToString();
                data.VersionMinor = optionsInterface.MinorVersion.ToString();
                data.VersionPatch = optionsInterface.PatchVersion.ToString();
            }

            success = true;
            return data;
        }
    }
}
