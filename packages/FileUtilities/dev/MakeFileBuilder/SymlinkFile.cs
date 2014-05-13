// <copyright file="SymlinkFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(FileUtilities.SymlinkFile moduleToBuild, out bool success)
        {
            Opus.Core.DependencyNode node = moduleToBuild.OwningNode;

            MakeFileVariableDictionary inputVariables = new MakeFileVariableDictionary();
            System.Collections.Generic.List<MakeFileData> dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
                        inputVariables.Append(data.VariableDictionary);
                        dataArray.Add(data);
                    }
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

            Opus.Core.BaseOptionCollection baseOptions = moduleToBuild.Options;
            Opus.Core.Target target = node.Target;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (baseOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = baseOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ISymlinkTool));
            string toolExecutablePath = tool.Executable((Opus.Core.BaseTarget)target);

            string recipe = null;
            if (toolExecutablePath.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", toolExecutablePath);
            }
            else
            {
                recipe += toolExecutablePath;
            }
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                recipe += System.String.Format(" {0} $@ $<", commandLineBuilder.ToString(' '));
            }
            else
            {
                recipe += System.String.Format(" {0} $< $@", commandLineBuilder.ToString(' '));
            }
            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile path : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            string sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            MakeFileRule rule = new MakeFileRule(baseOptions.OutputPaths,
                                                 FileUtilities.OutputFileFlags.Symlink,
                                                 node.UniqueModuleName,
                                                 directoriesToCreate,
                                                 null,
                                                 new Opus.Core.StringArray(sourceFilePath),
                                                 recipes);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;

            MakeFileData nodeData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            return nodeData;
        }
    }
}
