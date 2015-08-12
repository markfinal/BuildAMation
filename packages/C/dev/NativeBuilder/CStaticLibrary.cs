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
namespace C
{
    namespace V2
    {
        public sealed class NativeLibrarian :
            ILibrarianPolicy
        {
            void
            ILibrarianPolicy.Archive(
                StaticLibrary sender,
                Bam.Core.V2.ExecutionContext context,
                Bam.Core.V2.TokenizedString libraryPath,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> inputs,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers)
            {
                sender.MetaData = new Bam.Core.StringArray();
                var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
                if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
                {
                    var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                    map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, sender.MetaData });
                }

                var libraryFileDir = System.IO.Path.GetDirectoryName(libraryPath.ToString());
                if (!System.IO.Directory.Exists(libraryFileDir))
                {
                    System.IO.Directory.CreateDirectory(libraryFileDir);
                }

                var commandLine = sender.MetaData as Bam.Core.StringArray;

                foreach (var input in inputs)
                {
                    if (input is Bam.Core.V2.IModuleGroup)
                    {
                        foreach (var child in input.Children)
                        {
                            commandLine.Add(child.GeneratedPaths[C.V2.ObjectFile.Key].ToString());
                        }
                    }
                    else
                    {
                        commandLine.Add(input.GeneratedPaths[C.V2.ObjectFile.Key].ToString());
                    }
                }

                /*var exitStatus = */CommandLineProcessor.V2.Processor.Execute(context, sender.Tool, sender.MetaData as Bam.Core.StringArray);
            }
        }
    }
}
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.StaticLibrary moduleToBuild,
            out bool success)
        {
            var staticLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
            var target = node.Target;

            // find dependent object files
            var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.ObjectFile.OutputFile
                );
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
                Bam.Core.Log.Detail("There were no object files to archive for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            // dependency checking
            {
                var inputFiles = new Bam.Core.LocationArray();
                inputFiles.AddRange(dependentObjectFiles);
                var outputFiles = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
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
            if (staticLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = staticLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Archiver options does not support command line translation");
            }

            foreach (var dependentObjectFile in dependentObjectFiles)
            {
                commandLineBuilder.Add(dependentObjectFile.GetSinglePath());
            }

            var archiverTool = target.Toolset.Tool(typeof(C.IArchiverTool));
            var exitCode = CommandLineProcessor.Processor.Execute(node, archiverTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
