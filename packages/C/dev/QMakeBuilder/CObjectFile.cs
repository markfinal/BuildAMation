// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();

            // any source file that is derived from a moc step should not be listed
            // explicitly, because QMake handles this via searching for Q_OBJECT classes
            if (System.IO.Path.GetFileNameWithoutExtension(sourceFilePath).StartsWith("moc_"))
            {
                success = true;
                return null;
            }

            var node = moduleToBuild.OwningNode;
            var optionInterface = moduleToBuild.Options as C.ICCompilerOptions;

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            data.Sources.Add(sourceFilePath);
            data.Output = QMakeData.OutputType.ObjectFile;
#if true
            data.ObjectsDir = moduleToBuild.Locations[C.ObjectFile.ObjectFileDirLocationKey];
#else
            var options = moduleToBuild.Options as C.CompilerOptionCollection;
            data.ObjectsDir = options.OutputDirectoryPath;
#endif
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
