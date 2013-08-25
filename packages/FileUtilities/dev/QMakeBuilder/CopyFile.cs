// <copyright file="CopyFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
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
            {
                var besideModuleType = moduleToBuild.BesideModuleType;
                if (null != besideModuleType)
                {
                    var besideModuleNode = Opus.Core.ModuleUtilities.GetNode(besideModuleType, (Opus.Core.BaseTarget)target);
                    data.Merge(besideModuleNode.Data as QMakeData);
                }
                else
                {
                    var copyOptions = options as FileUtilities.ICopyFileOptions;
                    if (null == copyOptions.SourceModuleType)
                    {
                        Opus.Core.Log.MessageAll("QMake support for copying to arbitrary locations is unavailable");
                        success = true;
                        return null;
                    }

                    var copySourceNode = Opus.Core.ModuleUtilities.GetNode(copyOptions.SourceModuleType, (Opus.Core.BaseTarget)target);
                    data.Merge(copySourceNode.Data as QMakeData);
                }
            }

            var sourceFilePath = moduleToBuild.SourceFile.AbsolutePath;

            var makeDirCommand = new System.Text.StringBuilder();
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                makeDirCommand.AppendFormat("IF NOT EXIST {0} MKDIR {0}", destinationDirectory);
            }
            else
            {
                makeDirCommand.AppendFormat("mkdir -p {0}", destinationDirectory);
            }
            data.PostLink.Add(makeDirCommand.ToString());

            var copyCommand = new System.Text.StringBuilder();
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                copyCommand.AppendFormat("{0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), sourceFilePath, destinationDirectory);
            }
            else
            {
                copyCommand.AppendFormat("{0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), sourceFilePath, destinationDirectory);
            }
            data.PostLink.Add(copyCommand.ToString());

            success = true;
            return data;
        }
    }
}
