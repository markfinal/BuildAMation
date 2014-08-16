// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.ObjectFileCollectionBase moduleToBuild,
            out bool success)
        {
            var objectFileCollectionModule = moduleToBuild as Bam.Core.BaseModule;
            var node = objectFileCollectionModule.OwningNode;

            var dependents = new MakeFileVariableDictionary();
            var childDataArray = new Bam.Core.Array<MakeFileData>();
            foreach (var childNode in node.Children)
            {
                var data = childNode.Data as MakeFileData;
                if (!data.VariableDictionary.ContainsKey(C.ObjectFile.OutputFile))
                {
                    throw new Bam.Core.Exception("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName);
                }

                childDataArray.Add(data);
                dependents.Add(C.ObjectFile.OutputFile, data.VariableDictionary[C.ObjectFile.OutputFile]);
            }
            if (null != node.ExternalDependents)
            {
                var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null == dependentNode.Data)
                    {
                        continue;
                    }
                    var data = dependentNode.Data as MakeFileData;
                    foreach (var makeVariable in data.VariableDictionary.Filter(keysToFilter))
                    {
                        dependents.Add(makeVariable.Key, makeVariable.Value);
                    }
                }
            }

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            // no output paths because this rule has no recipe
            var rule = new MakeFileRule(
                moduleToBuild,
                C.ObjectFile.OutputFile,
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

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            success = true;
            return returnData;
        }
    }
}
