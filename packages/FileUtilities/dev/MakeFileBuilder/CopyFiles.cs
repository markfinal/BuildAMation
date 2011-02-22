// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, Opus.Core.DependencyNode node, out bool success)
        {
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            System.Collections.Generic.List<MakeFileData> sourceFileDataArray = new System.Collections.Generic.List<MakeFileData>();
            MakeFileVariableDictionary dependentVariables = new MakeFileVariableDictionary();
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                if (sourceModule.Options.OutputPaths.Has(sourceOutputPaths))
                {
                    if (sourceModule.OwningNode.Data != null)
                    {
                        MakeFileData data = sourceModule.OwningNode.Data as MakeFileData;
                        sourceFileDataArray.Add(data);
                        dependentVariables.Append(data.VariableDictionary);
                    }
                }
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile path : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string toolExecutablePath = tool.Executable(node.Target);

            foreach (MakeFileData data in sourceFileDataArray)
            {
#if false
                if (!data.Included)
                {
                    recipe.Includes.Add(data.MakeFilePath);
                    data.Included = true;
                }
#endif

                Opus.Core.StringArray inputVariables = new Opus.Core.StringArray(data.VariableDictionary.Values);
                string target = System.String.Format("{0}{1}$(notdir $({2}))", destinationDirectory, System.IO.Path.DirectorySeparatorChar, data.VariableDictionary.Values);

                System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
                commandLineBuilder.AppendFormat("\"{0}\" ", toolExecutablePath);
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    commandLineBuilder.Append("/c COPY $< $@");
                }

                Opus.Core.StringArray commandLines = new Opus.Core.StringArray();
                commandLines.Add(commandLineBuilder.ToString());

#if true
                MakeFileRule rule = new MakeFileRule(copyFiles.Options.OutputPaths.Types, sourceOutputPaths, target, dependentVariables);
                rule.Recipes.Add(commandLineBuilder.ToString());
#else
                MakeFileRule rule = new MakeFileRule(target, inputVariables, commandLines);
#endif
                makeFile.RuleArray.Add(rule);
            }

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
