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
    public sealed class NativeLinker :
        ILinkerPolicy
    {
        private string
        GetLibraryPath(Bam.Core.V2.Module module)
        {
            if (module is C.V2.StaticLibrary)
            {
                return module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString();
            }
            else if (module is C.V2.DynamicLibrary)
            {
                if (Bam.Core.OSUtilities.IsWindowsHosting)
                {
                    return module.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].ToString();
                }
                else
                {
                    return module.GeneratedPaths[C.V2.DynamicLibrary.Key].ToString();
                }
            }
            else if (module is C.V2.CSDKModule)
            {
                // collection of libraries, none in particular
                return null;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
            }
        }

        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
        {
            var linker = sender.Settings as C.V2.ICommonLinkerOptions;
            var libraryNames = new System.Collections.Generic.List<string>();
            // TODO: could the lib search paths be in the staticlibrary base class as a patch?
            foreach (var library in libraries)
            {
                var fullLibraryPath = this.GetLibraryPath(library);
                if (null == fullLibraryPath)
                {
                    continue;
                }
                var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                // TODO: watch for duplicates
                linker.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create(dir, null));
                if ((sender.Tool as C.V2.LinkerTool).UseLPrefixLibraryPaths)
                {
                    var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
                    libName = libName.Substring(3); // trim off lib prefix
                    libraryNames.Add(System.String.Format("-l{0}", libName));
                }
                else
                {
                    var libFilename = System.IO.Path.GetFileName(fullLibraryPath);
                    libraryNames.Add(libFilename);
                }
            }

            var executableDir = System.IO.Path.GetDirectoryName(executablePath.ToString());
            if (!System.IO.Directory.Exists(executableDir))
            {
                System.IO.Directory.CreateDirectory(executableDir);
            }

            sender.MetaData = new Bam.Core.StringArray();
            var commandLine = sender.MetaData as Bam.Core.StringArray;

            // first object files
            foreach (var input in objectFiles)
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

            // then dependent module libraries
            foreach (var lib in libraryNames)
            {
                commandLine.Add(lib);
            }

            // then all options
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, sender.MetaData });
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
            C.Application moduleToBuild,
            out bool success)
        {
            var applicationModule = moduleToBuild as Bam.Core.BaseModule;
            var node = applicationModule.OwningNode;
            var target = node.Target;
            var applicationOptions = applicationModule.Options;
            var linkerOptions = applicationOptions as C.ILinkerOptions;

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
