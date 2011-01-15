// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.DynamicLibrary dynamicLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Linker linkerInstance = C.LinkerFactory.GetTargetInstance(target);
            Opus.Core.ITool linkerTool = linkerInstance as Opus.Core.ITool;
            C.ILinkerOptions linkerOptions = dynamicLibrary.Options as C.ILinkerOptions;

            // dependent files
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
                    else if (node1.Module is C.StaticLibrary)
                    {
                        C.ArchiverOptionCollection options = node1.Module.Options as C.ArchiverOptionCollection;
                        linkerOptions.Libraries.AddToFront(options.LibraryFilePath);
                    }
                    else if (node1.Module is C.ThirdPartyModule)
                    {
                        // do nothing
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
                    else if (node1.Module is C.StaticLibrary)
                    {
                        C.ArchiverOptionCollection options = node1.Module.Options as C.ArchiverOptionCollection;
                        linkerOptions.Libraries.AddToFront(options.LibraryFilePath);
                    }
                    else if (node1.Module is C.HeaderLibrary)
                    {
                        // do nothing
                    }
                    else if (node1.Module is C.ThirdPartyModule)
                    {
                        // do nothing
                    }
                    else
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unexpected type for dependent: '{0}'", node1.Module.GetType().ToString()));
                    }
                }
            }
            if (0 == dependentObjectFiles.Count)
            {
                throw new Opus.Core.Exception("There are no object files to link");
            }

            Opus.Core.StringArray inputFiles = dependentObjectFiles;
            Opus.Core.IOutputPaths outputPaths = dynamicLibrary.Options as Opus.Core.IOutputPaths;
            Opus.Core.StringArray outputFiles = new Opus.Core.StringArray(outputPaths.GetOutputPaths().Values);
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            string executablePath;
            C.IToolchainOptions toolchainOptions = (dynamicLibrary.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executablePath = linkerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executablePath = linkerTool.Executable(target);
            }

            // object files must come before everything else, for some compilers
            commandLineBuilder.Insert(0, dependentObjectFiles.ToString(' '));

            int exitCode = CommandLineProcessor.Processor.Execute(node, linkerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}