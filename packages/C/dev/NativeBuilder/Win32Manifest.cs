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
#endregion
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.Win32Manifest moduleToBuild,
            out bool success)
        {
            var binaryLoc = moduleToBuild.BinaryFileLocation;
            var binaryLocPath = binaryLoc.GetSinglePath();
            if (!System.IO.File.Exists(binaryLocPath))
            {
                throw new Bam.Core.Exception("Binary file '{0}' does not exist", binaryLocPath);
            }

            var asModule = moduleToBuild as Bam.Core.BaseModule;
            var options = asModule.Options;

            var compilerOptions = options as C.Win32ManifestOptionCollection;

            // don't do dependency checking, as it's an in-place operation
            // TODO: should really do dependency checking on whether the manifest is newer than the executable

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = asModule.OwningNode.Target;

            var commandLineBuilder = new Bam.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var compilerTool = target.Toolset.Tool(typeof(C.IWinManifestTool)) as C.IWinManifestTool;

            // add input path
            commandLineBuilder.Add(System.String.Format("-manifest {0}.manifest", binaryLocPath));
            commandLineBuilder.Add("-nologo");
            // add output path
            var binaryModule = moduleToBuild.OwningNode.ExternalDependents[0].Module;
            if (binaryModule is C.DynamicLibrary)
            {
                commandLineBuilder.Add(System.String.Format("-outputresource:{0};2", binaryLocPath));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("-outputresource:{0};1", binaryLocPath));
            }

            var exitCode = CommandLineProcessor.Processor.Execute(asModule.OwningNode, compilerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
