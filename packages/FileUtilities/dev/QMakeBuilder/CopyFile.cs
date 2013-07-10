// <copyright file="CopyFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(FileUtilities.CopyFile moduleToBuild, out bool success)
        {
            var options = moduleToBuild.Options;
            var node = moduleToBuild.OwningNode;
            var target = node.Target;

            var tool = target.Toolset.Tool(typeof(FileUtilities.ICopyFileTool));
            var toolExecutablePath = tool.Executable((Opus.Core.BaseTarget)target);

            var commandLineBuilder = new Opus.Core.StringArray();
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }
            var copiedFilePath = options.OutputPaths[FileUtilities.OutputFileFlags.CopiedFile];
            var destinationDirectory = System.IO.Path.GetDirectoryName(copiedFilePath);

            var data = new QMakeData(node);

            var sourceFilePath = moduleToBuild.SourceFile.AbsolutePath;

            System.Text.StringBuilder postLinkCommand = new System.Text.StringBuilder();
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                postLinkCommand.AppendFormat("{0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), sourceFilePath, destinationDirectory);
            }
            else
            {
                postLinkCommand.AppendFormat("{0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), sourceFilePath, destinationDirectory);
            }
            data.PostLink.Add(postLinkCommand.ToString());

            success = true;
            return data;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(FileUtilities.CopyFile module, out bool success)
        {
            var owningNode = module.OwningNode;
            var besideModuleType = module.BesideModuleType;
            if (null == besideModuleType)
            {
                Opus.Core.Log.MessageAll("QMake support for copying to arbitrary locations is unavailable");
                success = true;
                return null;
            }

            var besideModuleNode = Opus.Core.ModuleUtilities.GetNode(besideModuleType, (Opus.Core.BaseTarget)owningNode.Target);

            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", module);

            success = true;
            return null;
        }
    }
}
