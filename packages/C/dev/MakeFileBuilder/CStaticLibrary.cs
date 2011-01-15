// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Archiver archiverInstance = C.ArchiverFactory.GetTargetInstance(target);
            Opus.Core.ITool archiverTool = archiverInstance as Opus.Core.ITool;

            // dependents
            Opus.Core.StringArray inputs = new Opus.Core.StringArray();
            System.Collections.Generic.List<MakeFileData> dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                foreach (Opus.Core.DependencyNode node1 in node.Children)
                {
                    if ((node1.Module is C.ObjectFileCollectionBase) || (node1.Module is C.ObjectFile))
                    {
                        MakeFileData data = node1.Data as MakeFileData;
                        // TODO: handle this better for more dependents
                        inputs.Add(data.Variable);
                        dataArray.Add(data);
                    }
                    else
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unexpected type '{0}'", node1.Module.ToString()));
                    }
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode node1 in node.ExternalDependents)
                {
                    if ((node1.Module is C.ObjectFileCollectionBase) || (node1.Module is C.ObjectFile))
                    {
                        MakeFileData data = node1.Data as MakeFileData;
                        // TODO: handle this better for more dependents
                        inputs.Add(data.Target);
                        dataArray.Add(data);
                    }
                    else if (node1.Module is C.ThirdPartyModule)
                    {
                        // do nothing
                    }
                    else
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unexpected type '{0}'", node1.Module.ToString()));
                    }
                }
            }

            string executable = archiverTool.Executable(target);

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (staticLibrary.Options is CommandLineProcessor.ICommandLineSupport)
            {
                // TODO: pass in a map of path translations, e.g. outputfile > $@
                CommandLineProcessor.ICommandLineSupport commandLineOption = staticLibrary.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            Opus.Core.StringArray commandLines = new Opus.Core.StringArray();
            commandLines.Add(System.String.Format("\"{0}\" {1} $(filter %{2},$^)", executable, commandLineBuilder.ToString(), toolchain.ObjectFileExtension));

            MakeFileBuilderRecipe recipe = new MakeFileBuilderRecipe(node, inputs, commandLines, this.topLevelMakeFilePath);

            foreach (MakeFileData data in dataArray)
            {
                if (!data.Included)
                {
                    string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.File, this.topLevelMakeFilePath, "$(CURDIR)");
                    recipe.Includes.Add(relativeDataFile);
                    data.Included = true;
                }
            }

            string makeFile = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFile));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFile);

            string makeFileTargetName = null;
            string makeFileVariableName = null;
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFile))
            {
                recipe.Write(makeFileWriter, "LibraryFile");
                makeFileTargetName = recipe.TargetName;
                makeFileVariableName = recipe.VariableName;
            }

            success = true;
            MakeFileData returnData = new MakeFileData(makeFile, makeFileTargetName, makeFileVariableName, archiverTool.EnvironmentPaths(target));
            return returnData;
        }
    }
}