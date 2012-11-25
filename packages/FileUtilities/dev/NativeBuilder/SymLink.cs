// <copyright file="SymLink.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.SymLink symLink, out bool success)
        {
            Opus.Core.BaseModule symLinkModule = symLink as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = symLinkModule.OwningNode;
            Opus.Core.Target target = node.Target;

            // locate target
            string symlinkTarget = null;
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Instance;
            System.Reflection.FieldInfo[] fields = symLink.GetType().GetFields(bindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var sourceModuleAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                if (1 == sourceModuleAttributes.Length)
                {
                    if (null != symlinkTarget)
                    {
                        throw new Opus.Core.Exception("Can only specify one target for a symlink", false);
                    }

                    Opus.Core.File file = field.GetValue(symLink) as Opus.Core.File;
                    if (null == file)
                    {
                        throw new Opus.Core.Exception("Target can only be of type Opus.Core.File", false);
                    }

                    symlinkTarget = file.AbsolutePath;
                }
            }

            if (null == symlinkTarget)
            {
                throw new Opus.Core.Exception("No symlink target specified", false);
            }

            Opus.Core.BaseOptionCollection symLinkOptions = symLinkModule.Options;

            string link = symLinkOptions.OutputPaths[FileUtilities.SymLinkOutputFileFlags.Link];

            FileUtilities.ISymLinkOptions options = symLinkOptions as FileUtilities.ISymLinkOptions;
            bool requiresBuilding = true;
            if (options.Type == FileUtilities.EType.Directory)
            {
                if (!System.IO.Directory.Exists(symlinkTarget))
                {
                    throw new Opus.Core.Exception(System.String.Format("Symlink target directory '{0}' does not exist", symlinkTarget), false);
                }

                requiresBuilding = NativeBuilder.DirectoryUpToDate(link, symlinkTarget);
            }
            else
            {
                if (!System.IO.File.Exists(symlinkTarget))
                {
                    throw new Opus.Core.Exception(System.String.Format("Symlink target file '{0}' does not exist", symlinkTarget), false);
                }

                requiresBuilding = NativeBuilder.RequiresBuilding(link, symlinkTarget);
            }

            if (!requiresBuilding)
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (symLinkOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = symLinkOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Symlink options does not support command line translation");
            }

            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                commandLineBuilder.Add(symLinkOptions.OutputPaths[FileUtilities.SymLinkOutputFileFlags.Link]);
                commandLineBuilder.Add(symlinkTarget);
            }
            else
            {
                commandLineBuilder.Add(symlinkTarget);
                commandLineBuilder.Add(symLinkOptions.OutputPaths[FileUtilities.SymLinkOutputFileFlags.Link]);
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ISymLinkTool));
            int returnValue = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == returnValue);

            return null;
        }
    }
}
