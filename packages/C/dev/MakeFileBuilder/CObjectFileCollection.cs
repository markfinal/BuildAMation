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
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                MakeFileData data = childNode.Data as MakeFileData;
                // TODO: handle this better for more dependents
                if (null == data.Variable)
                {
                    throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is empty", childNode.UniqueModuleName), false);
                }
                dependents.Add(System.String.Format("$({0})", data.Variable));
            }
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
                        if (null == data.Variable)
                        {
                            throw new Opus.Core.Exception(System.String.Format("MakeFile Variable for '{0}' is empty", dependentNode.UniqueModuleName), false);
                        }
                        // TODO: handle this better for more dependents
                        dependents.Add(System.String.Format("$({0})", data.Variable));
                    }
                }
            }

            string makeFile = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFile));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFile);

            string uniqueModuleName = node.UniqueModuleName;

            string makeFileTargetName = null;
            string makeFileVariableName = null;
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFile))
            {
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
                if (null != node.ExternalDependents)
                {
                    foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
                        if (!data.Included)
                        {
                            string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.File, this.topLevelMakeFilePath, "$(CURDIR)");
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
                }
                else
                {
                    makeFileVariableName = System.String.Format("{0}_{1}_Output", uniqueModuleName, target.Key);
                    makeFileWriter.WriteLine("{0} = {1}", makeFileVariableName, dependents);
                }
            }

            success = true;

            MakeFileData returnData = new MakeFileData(makeFile, makeFileTargetName, makeFileVariableName, null);
            return returnData;
        }
    }
}