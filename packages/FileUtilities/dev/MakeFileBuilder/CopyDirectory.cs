// <copyright file="CopyDirectory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(FileUtilities.CopyDirectory moduleToBuild, out bool success)
        {
            Opus.Core.DependencyNode node = moduleToBuild.OwningNode;

            MakeFileVariableDictionary dependents = new MakeFileVariableDictionary();
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                MakeFileData data = childNode.Data as MakeFileData;
                if (!data.VariableDictionary.ContainsKey(FileUtilities.OutputFileFlags.CopiedFile))
                {
                    throw new Opus.Core.Exception("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName);
                }

                dependents.Add(FileUtilities.OutputFileFlags.CopiedFile, data.VariableDictionary[FileUtilities.OutputFileFlags.CopiedFile]);
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            // no output paths because this rule has no recipe
            MakeFileRule rule = new MakeFileRule(null,
                                                 FileUtilities.OutputFileFlags.CopiedDirectory,
                                                 node.UniqueModuleName,
                                                 null,
                                                 dependents,
                                                 null,
                                                 null);
            if (null == node.Parent)
            {
                // phony target
                rule.TargetIsPhony = true;
            }
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            success = true;
            return returnData;
        }
    }
}
