// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Archiver archiverInstance = C.ArchiverFactory.GetTargetInstance(target);
            Opus.Core.ITool archiverTool = archiverInstance as Opus.Core.ITool;

            // dependents
            Opus.Core.StringArray dependentObjectFiles = new Opus.Core.StringArray();
            if (node.Children != null)
            {
                foreach (Opus.Core.DependencyNode node1 in node.Children)
                {
                    if (node1.Module is C.ObjectFileCollection)
                    {
                        foreach (Opus.Core.DependencyNode node2 in node1.Children)
                        {
                            if (node2.Module is C.ObjectFile)
                            {
                                C.CompilerOptionCollection options = node2.Module.Options as C.CompilerOptionCollection;
                                dependentObjectFiles.Add(options.ObjectFilePath);
                            }
                            else
                            {
                                throw new Opus.Core.Exception(System.String.Format("Unexpected type for dependent: '{0}'. Should be of type C.ObjectFile", node2.Module.GetType().ToString()));
                            }
                        }
                    }
                    else if (node1.Module is C.CPlusPlus.ObjectFileCollection)
                    {
                        foreach (Opus.Core.DependencyNode node2 in node1.Children)
                        {
                            if (node2.Module is C.CPlusPlus.ObjectFile)
                            {
                                C.CompilerOptionCollection options = node2.Module.Options as C.CompilerOptionCollection;
                                dependentObjectFiles.Add(options.ObjectFilePath);
                            }
                            else
                            {
                                throw new Opus.Core.Exception(System.String.Format("Unexpected type for dependent: '{0}'. Should be of type CPlusPlus.ObjectFile", node2.Module.GetType().ToString()));
                            }
                        }
                    }
                    else if (node1.Module is C.ObjectFile)
                    {
                        C.CompilerOptionCollection options = node1.Module.Options as C.CompilerOptionCollection;
                        dependentObjectFiles.Add(options.ObjectFilePath);
                    }
                    else if (node1.Module is C.CPlusPlus.ObjectFile)
                    {
                        C.CompilerOptionCollection options = node1.Module.Options as C.CompilerOptionCollection;
                        dependentObjectFiles.Add(options.ObjectFilePath);
                    }
                    else
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unexpected type for dependent: '{0}'", node1.Module.GetType().ToString()));
                    }
                }
            }
            if (node.ExternalDependents != null)
            {
                foreach (Opus.Core.DependencyNode node1 in node.ExternalDependents)
                {
                    if (node1.Module is C.ObjectFileCollection)
                    {
                        foreach (Opus.Core.DependencyNode node2 in node1.ExternalDependents)
                        {
                            if (node2.Module is C.ObjectFile)
                            {
                                C.CompilerOptionCollection options = node2.Module.Options as C.CompilerOptionCollection;
                                dependentObjectFiles.Add(options.ObjectFilePath);
                            }
                            else
                            {
                                throw new Opus.Core.Exception("Unexpected type");
                            }
                        }
                    }
                    else if (node1.Module is C.ObjectFile)
                    {
                        C.CompilerOptionCollection options = node1.Module.Options as C.CompilerOptionCollection;
                        dependentObjectFiles.Add(options.ObjectFilePath);
                    }
                    else if (node1.Module is C.ThirdPartyModule)
                    {
                        // do nothing
                    }
                    else
                    {
                        throw new Opus.Core.Exception("Unexpected type");
                    }
                }
            }
            if (0 == dependentObjectFiles.Count)
            {
                throw new Opus.Core.Exception("There are no object files to link");
            }

            Opus.Core.StringArray inputFiles = dependentObjectFiles;
            Opus.Core.StringArray outputFiles = staticLibrary.Options.OutputPaths.Paths;
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (staticLibrary.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = staticLibrary.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            string executablePath = archiverTool.Executable(target);

            commandLineBuilder.Append(dependentObjectFiles.ToString(' '));

            int exitCode = CommandLineProcessor.Processor.Execute(node, archiverTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}