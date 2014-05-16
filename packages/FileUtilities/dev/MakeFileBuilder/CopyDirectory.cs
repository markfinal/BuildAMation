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
            var node = moduleToBuild.OwningNode;

            var dependents = new MakeFileVariableDictionary();
            foreach (var childNode in node.Children)
            {
                var data = childNode.Data as MakeFileData;
#if true
                if (!data.VariableDictionary.ContainsKey(FileUtilities.CopyFile.OutputFile))
                {
                    throw new Opus.Core.Exception("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName);
                }

                dependents.Add(FileUtilities.CopyFile.OutputFile, data.VariableDictionary[FileUtilities.CopyFile.OutputFile]);
#else
                if (!data.VariableDictionary.ContainsKey(FileUtilities.OutputFileFlags.CopiedFile))
                {
                    throw new Opus.Core.Exception("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName);
                }

                dependents.Add(FileUtilities.OutputFileFlags.CopiedFile, data.VariableDictionary[FileUtilities.OutputFileFlags.CopiedFile]);
#endif
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

#if true
            var rule = new MakeFileRule(
                moduleToBuild,
                FileUtilities.CopyDirectory.OutputDir,
                node.UniqueModuleName,
                null,
                dependents,
                null,
                null);
#else
            // no output paths because this rule has no recipe
            MakeFileRule rule = new MakeFileRule(null,
                                                 FileUtilities.OutputFileFlags.CopiedDirectory,
                                                 node.UniqueModuleName,
                                                 null,
                                                 dependents,
                                                 null,
                                                 null);
#endif
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
