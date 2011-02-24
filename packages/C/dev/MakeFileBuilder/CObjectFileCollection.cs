// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, Opus.Core.DependencyNode node, out bool success)
        {
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
                        bool hasObjectFile = data.VariableDictionary.ContainsKey(C.OutputFileFlags.ObjectFile);
                        bool hasStaticImportFile = data.VariableDictionary.ContainsKey(C.OutputFileFlags.StaticImportLibrary);
                        if (!hasObjectFile && !hasStaticImportFile)
                        {
                            throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is missing", dependentNode.UniqueModuleName), false);
                        }

                        if (hasObjectFile)
                        {
                            childDataArray.Add(data);
                            dependents.Add(C.OutputFileFlags.ObjectFile, data.VariableDictionary[C.OutputFileFlags.ObjectFile]);
                        }
                        else if (hasStaticImportFile)
                        {
                            childDataArray.Add(data);
                            dependents.Add(C.OutputFileFlags.StaticImportLibrary, data.VariableDictionary[C.OutputFileFlags.StaticImportLibrary]);
                        }
                    }
                }
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(objectFileCollection.Options.OutputPaths, C.OutputFileFlags.ObjectFileCollection, node.UniqueModuleName, null, dependents, null);
            if (null == node.Parent)
            {
                // phony target
                rule.ExportTarget = true;
                rule.ExportVariable = false;
                rule.TargetIsPhony = true;
            }
            else
            {
                // variable rule with no target
                rule.ExportTarget = false;
                rule.ExportVariable = true;
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