// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(Qt.MocFileCollection mocFileCollection, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;

            MakeFileVariableDictionary dependents = new MakeFileVariableDictionary();
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                MakeFileData data = childNode.Data as MakeFileData;
                // TODO: handle this better for more dependents
                dependents.Append(data.VariableDictionary);
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            string uniqueModuleName = node.UniqueModuleName;

#if true
            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);
#else
            string makeFileTargetName = null;
            string makeFileVariableName = null;
#endif
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
#if true
                makeFile.Write(makeFileWriter);        
#else
                foreach (Opus.Core.DependencyNode childNode in node.Children)
                {
                    MakeFileData data = childNode.Data as MakeFileData;
                    if (!data.Included)
                    {
                        string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.MakeFilePath, this.topLevelMakeFilePath, "$(CURDIR)");
                        makeFileWriter.WriteLine("include {0}", relativeDataFile);
                        data.Included = true;
                    }
                }

                if (null != node.Parent || null != node.ExternalDependentFor)
                {
                    makeFileVariableName = System.String.Format("{0}_{1}_Output", uniqueModuleName, target.Key);
                    makeFileWriter.WriteLine("{0} = {1}", makeFileVariableName, dependents);
                }
                else
                {
                    makeFileTargetName = System.String.Format("{0}_{1}", uniqueModuleName, target.Key);
                    makeFileWriter.WriteLine(".PHONY: {0}", makeFileTargetName);
                    makeFileWriter.WriteLine("{0}: {1}", makeFileTargetName, dependents.ToString(' '));
                }
#endif
            }

            success = true;

#if true
            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            return returnData;
#else
            MakeFileData returnData = new MakeFileData(makeFilePath, makeFileTargetName, makeFileVariableName, null);
            return returnData;
#endif
        }
    }
}