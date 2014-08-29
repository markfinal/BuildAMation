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
            CodeGenTest.CodeGenModule moduleToBuild,
            out bool success)
        {
            var codeGenModuleModule = moduleToBuild as Bam.Core.BaseModule;
            var node = codeGenModuleModule.OwningNode;
            var target = node.Target;
            var codeGenModuleOptions = codeGenModuleModule.Options;
            var toolOptions = codeGenModuleOptions as CodeGenTest.CodeGenOptionCollection;
            var tool = target.Toolset.Tool(typeof(CodeGenTest.ICodeGenTool));

            // dependency checking
            {
                var inputLocations = new Bam.Core.LocationArray(
                    Bam.Core.FileLocation.Get(tool.Executable((Bam.Core.BaseTarget)target), Bam.Core.Location.EExists.WillExist)
                    );
                var outputLocations = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputLocations, inputLocations))
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
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("CodeGen options does not support command line translation");
            }

            var exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}