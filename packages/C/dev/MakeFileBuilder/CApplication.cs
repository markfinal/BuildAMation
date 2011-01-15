// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.Application application, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Linker linkerInstance = C.LinkerFactory.GetTargetInstance(target);
            Opus.Core.ITool linkerTool = linkerInstance as Opus.Core.ITool;

            // dependents
            Opus.Core.StringArray inputs = new Opus.Core.StringArray();
            System.Collections.Generic.List<MakeFileData> dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                foreach (Opus.Core.DependencyNode node1 in node.Children)
                {
                    if ((node1.Module is C.ObjectFileCollectionBase) || (node1.Module is C.ObjectFile) || (node1.Module is C.StaticLibrary) || (node1.Module is C.DynamicLibrary))
                    {
                        MakeFileData data = node1.Data as MakeFileData;
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
                        inputs.Add(data.Target);
                        dataArray.Add(data);
                    }
                    else if ((node1.Module is C.StaticLibrary) || (node1.Module is C.DynamicLibrary))
                    {
                        MakeFileData data = node1.Data as MakeFileData;
                        inputs.Add(data.Variable);
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

            string executable;
            C.IToolchainOptions toolchainOptions = (application.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executable = linkerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executable = linkerTool.Executable(target);
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (application.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = application.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            Opus.Core.StringArray commandLines = new Opus.Core.StringArray();
            commandLines.Add(System.String.Format("\"{0}\" {1} $(filter %{2},$^) $(filter %{3},$^)", executable, commandLineBuilder.ToString(), toolchain.ObjectFileExtension, toolchain.StaticLibraryExtension));

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
                recipe.Write(makeFileWriter, "OutputFile");
                makeFileTargetName = recipe.TargetName;
                makeFileVariableName = recipe.VariableName;
            }

            success = true;
            MakeFileData returnData = new MakeFileData(makeFile, makeFileTargetName, makeFileVariableName, linkerTool.EnvironmentPaths(target));
            return returnData;
        }
    }
}