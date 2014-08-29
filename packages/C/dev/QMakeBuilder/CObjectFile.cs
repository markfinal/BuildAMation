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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var includeSourceInProject = true;
            if (moduleToBuild is Bam.Core.IIsGeneratedSource)
            {
                var generatedSource = moduleToBuild as Bam.Core.IIsGeneratedSource;
                includeSourceInProject = !generatedSource.AutomaticallyHandledByBuilder(node.Target);
            }

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            if (includeSourceInProject)
            {
                var sourceLoc = moduleToBuild.SourceFileLocation;
                var sourceFilePath = sourceLoc.GetSinglePath();
                if ((moduleToBuild is C.ObjC.ObjectFile) || (moduleToBuild is C.ObjCxx.ObjectFile))
                {
                    data.ObjectiveSources.Add(sourceFilePath);
                }
                else
                {
                    data.Sources.Add(sourceFilePath);
                }
                data.Output = QMakeData.OutputType.ObjectFile;
            }

            var optionInterface = moduleToBuild.Options as C.ICCompilerOptions;

            data.ObjectsDir = moduleToBuild.Locations[C.ObjectFile.OutputDir];
            data.IncludePaths.AddRangeUnique(optionInterface.IncludePaths.ToStringArray());
            if (optionInterface.IgnoreStandardIncludePaths)
            {
                data.IncludePaths.AddRangeUnique(optionInterface.SystemIncludePaths.ToStringArray());
            }
            data.Defines.AddRangeUnique(optionInterface.Defines.ToStringArray());

            if (optionInterface is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineBuilder = new Bam.Core.StringArray();
                var target = node.Target;
                var commandLineOption = optionInterface as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Bam.Core.StringArray();
                excludedOptionNames.Add("OutputType");
                excludedOptionNames.Add("Defines");
                excludedOptionNames.Add("IncludePaths");
                if (optionInterface.IgnoreStandardIncludePaths)
                {
                    excludedOptionNames.Add("SystemIncludePaths");
                }
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptionNames);
                if (optionInterface is C.ICxxCompilerOptions)
                {
                    data.CXXFlags.AddRangeUnique(commandLineBuilder);
                }
                else
                {
                    data.CCFlags.AddRangeUnique(commandLineBuilder);
                }
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            success = true;
            return data;
        }
    }
}
