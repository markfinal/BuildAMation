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
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.DynamicLibrary moduleToBuild,
            out bool success)
        {
            var dynamicLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = dynamicLibraryModule.OwningNode;
            var target = node.Target;
            var dynamicLibraryOptions = dynamicLibraryModule.Options;
            var linkerOptions = dynamicLibraryOptions as C.ILinkerOptions;

            // find dependent object files
            var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.ObjectFile.OutputFile
                );
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                keysToFilter.Add(C.Win32Resource.OutputFile);
            }
            var dependentObjectFiles = new Bam.Core.LocationArray();
            if (null != node.Children)
            {
                node.Children.FilterOutputLocations(keysToFilter, dependentObjectFiles);
            }
            if (null != node.ExternalDependents)
            {
                node.ExternalDependents.FilterOutputLocations(keysToFilter, dependentObjectFiles);
            }
            if (0 == dependentObjectFiles.Count)
            {
                Bam.Core.Log.Detail("There were no object files to link for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            // find dependent library files
            Bam.Core.LocationArray dependentLibraryFiles = null;
            if (null != node.ExternalDependents)
            {
                dependentLibraryFiles = new Bam.Core.LocationArray();

                var libraryKeysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey);
                if (target.HasPlatform(Bam.Core.EPlatform.Unix))
                {
                    libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }

                node.ExternalDependents.FilterOutputLocations(libraryKeysToFilter, dependentLibraryFiles);
            }

            // dependency checking
            {
                var inputFiles = new Bam.Core.LocationArray();
                inputFiles.AddRange(dependentObjectFiles);
                if (null != dependentLibraryFiles)
                {
                    inputFiles.AddRange(dependentLibraryFiles);
                }

                var outputFiles = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                // don't dependency check against the static import library, since it is generally not rewritten
                // when code changes
                if (outputFiles.Contains(moduleToBuild.Locations[C.DynamicLibrary.ImportLibraryFile]))
                {
                    outputFiles.Remove(moduleToBuild.Locations[C.DynamicLibrary.ImportLibraryFile]);
                }
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Bam.Core.StringArray();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                // libraries are manually added later
                var excludedOptions = new Bam.Core.StringArray("Libraries", "StandardLibraries");
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);
            }
            else
            {
                throw new Bam.Core.Exception("Linker options does not support command line translation");
            }

            // object files must come before everything else, for some compilers
            commandLineBuilder.Insert(0, dependentObjectFiles.Stringify(" "));

            // then libraries
            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            C.LinkerUtilities.AppendLibrariesToCommandLine(commandLineBuilder, linkerTool, linkerOptions, dependentLibraryFiles);

            var exitCode = CommandLineProcessor.Processor.Execute(node, linkerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
