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

            Opus.Core.StringArray dependents = new Opus.Core.StringArray();
            Opus.Core.Array<MakeFileData> childDataArray = new Opus.Core.Array<MakeFileData>();
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                MakeFileData data = childNode.Data as MakeFileData;
#if true
                if (!data.VariableDictionary.ContainsKey(C.OutputFileFlags.ObjectFile))
                {
                    throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName), false);
                }

                childDataArray.Add(data);
                dependents.Add(data.VariableDictionary[C.OutputFileFlags.ObjectFile]);
#else
                // TODO: handle this better for more dependents
                if (null == data.Variable)
                {
                    throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is empty", childNode.UniqueModuleName), false);
                }
                dependents.Add(System.String.Format("$({0})", data.Variable));
#endif
            }
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
#if true
                        bool hasObjectFile = data.VariableDictionary.ContainsKey(C.OutputFileFlags.ObjectFile);
                        bool hasStaticImportFile = data.VariableDictionary.ContainsKey(C.OutputFileFlags.StaticImportLibrary);
                        if (!hasObjectFile && !hasStaticImportFile)
                        {
                            throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is missing", dependentNode.UniqueModuleName), false);
                        }

                        if (hasObjectFile)
                        {
                            childDataArray.Add(data);
                            dependents.Add(System.String.Format("$({0})", data.VariableDictionary[C.OutputFileFlags.ObjectFile]));
                        }
                        else if (hasStaticImportFile)
                        {
                            childDataArray.Add(data);
                            dependents.Add(System.String.Format("$({0})", data.VariableDictionary[C.OutputFileFlags.StaticImportLibrary]));
                        }

#else
                        if (null == data.Variable)
                        {
                            throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is empty", dependentNode.UniqueModuleName), false);
                        }
                        // TODO: handle this better for more dependents
                        dependents.Add(System.String.Format("$({0})", data.Variable));
#endif
                    }
                }
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

#if true
            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            if (null == node.Parent)
            {
                // phony target
                MakeFileRule rule = new MakeFileRule(node.UniqueModuleName, dependents);
                makeFile.RuleArray.Add(rule);
            }
            else
            {
                // variable rule with no 
            }

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            success = true;
            return null;
#else
            string uniqueModuleName = node.UniqueModuleName;

            MakeFileTargetDictionary exportedTargets = new MakeFileTargetDictionary();
            MakeFileVariableDictionary exportedVariables = new MakeFileVariableDictionary();
            string makeFileTargetName = null;
            string makeFileVariableName = null;
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
#if false
                foreach (Opus.Core.DependencyNode childNode in node.Children)
                {
                    MakeFileData data = childNode.Data as MakeFileData;
                    if (!data.Included)
                    {
                        string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.File, this.topLevelMakeFilePath, "$(CURDIR)");
                        makeFileWriter.WriteLine("include {0}", relativeDataFile);
                        data.Included = true;
                    }
                }
#endif
                if (null != node.ExternalDependents)
                {
                    foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
                        if (!data.Included)
                        {
                            string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.MakeFilePath, this.topLevelMakeFilePath, "$(CURDIR)");
                            makeFileWriter.WriteLine("include {0}", relativeDataFile);
                            data.Included = true;
                        }
                    }
                }

                if (null == node.Parent)
                {
                    makeFileTargetName = System.String.Format("{0}_{1}", uniqueModuleName, target.Key);
                    makeFileWriter.WriteLine(".PHONY: {0}", makeFileTargetName);
                    makeFileWriter.WriteLine("{0}: {1}", makeFileTargetName, dependents.ToString(' '));

                    // TODO: this isn't quite right for the type
                    // there is no type really
                    exportedTargets.Add(C.OutputFileFlags.ObjectFile, makeFileTargetName);
                }
                else
                {
                    makeFileVariableName = System.String.Format("{0}_{1}_Output", uniqueModuleName, target.Key);
                    makeFileWriter.WriteLine("{0} = {1}", makeFileVariableName, dependents);
                    // TODO: the type of the target isn't right either
                    exportedVariables.Add(C.OutputFileFlags.ObjectFile, makeFileVariableName);
                }
            }

            success = true;

            MakeFileData returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, null);
            return returnData;
#endif
        }
    }
}