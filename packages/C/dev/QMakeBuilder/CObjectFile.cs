// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
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
            if (moduleToBuild is Opus.Core.IIsGeneratedSource)
            {
                var generatedSource = moduleToBuild as Opus.Core.IIsGeneratedSource;
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
                var commandLineBuilder = new Opus.Core.StringArray();
                var target = node.Target;
                var commandLineOption = optionInterface as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Opus.Core.StringArray();
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
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            success = true;
            return data;
        }
    }
}
