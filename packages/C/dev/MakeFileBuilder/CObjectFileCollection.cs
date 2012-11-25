// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, out bool success)
        {
            Opus.Core.BaseModule objectFileCollectionModule = objectFileCollection as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = objectFileCollectionModule.OwningNode;
            Opus.Core.Target target = node.Target;

            MakeFileVariableDictionary dependents = new MakeFileVariableDictionary();
            Opus.Core.Array<MakeFileData> childDataArray = new Opus.Core.Array<MakeFileData>();
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                MakeFileData data = childNode.Data as MakeFileData;
                if (!data.VariableDictionary.ContainsKey(C.OutputFileFlags.ObjectFile))
                {
                    throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName), false);
                }

                childDataArray.Add(data);
                dependents.Add(C.OutputFileFlags.ObjectFile, data.VariableDictionary[C.OutputFileFlags.ObjectFile]);
            }
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
                        foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> makeVariable in data.VariableDictionary)
                        {
                            dependents.Add(makeVariable.Key, makeVariable.Value);
                        }
                    }
                }
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            // no output paths because this rule has no recipe
            MakeFileRule rule = new MakeFileRule(null, C.OutputFileFlags.ObjectFileCollection, node.UniqueModuleName, null, dependents, null, null);
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

            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null, null);
            success = true;
            return returnData;
        }
    }
}