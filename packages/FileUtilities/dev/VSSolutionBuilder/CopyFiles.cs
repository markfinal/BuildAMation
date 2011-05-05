// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        private void PostBuildEventCopyFiles(Opus.Core.IModule moduleForPostEvents, string destinationDirectory, Opus.Core.FileCollection filesToCopy)
        {
            Opus.Core.StringArray fileArray = new Opus.Core.StringArray();
            foreach (string file in filesToCopy)
            {
                fileArray.Add(file);
            }

            PostBuildEventCopyFiles(moduleForPostEvents, destinationDirectory, fileArray);
        }

        private void PostBuildEventCopyFiles(Opus.Core.IModule moduleForPostEvents, string destinationDirectory, Opus.Core.StringArray filesToCopy)
        {
            Opus.Core.DependencyNode sourceModuleNode = moduleForPostEvents.OwningNode;
            IProject nodeProjectData = sourceModuleNode.Data as IProject;

            ProjectConfigurationCollection configCollection = nodeProjectData.Configurations;
            string configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(sourceModuleNode.Target);
            ProjectConfiguration configuration = configCollection[configurationName];

            string toolName = "VCPostBuildEventTool";
            ProjectTool vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            System.Text.StringBuilder commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("IF NOT EXIST \"{0}\" MKDIR \"{0}\"\n\r", destinationDirectory);
            foreach (string sourceFile in filesToCopy)
            {
                commandLine.AppendFormat("cmd.exe /c COPY \"{0}\" \"{1}\"\n\r", sourceFile, destinationDirectory);
            }

            lock (vcPostBuildEventTool)
            {
                const string attributeName = "CommandLine";
                if (vcPostBuildEventTool.HasAttribute(attributeName))
                {
                    string currentValue = vcPostBuildEventTool[attributeName];
                    currentValue += commandLine.ToString();
                    vcPostBuildEventTool[attributeName] = currentValue;
                }
                else
                {
                    vcPostBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                }
            }
        }

        public object Build(FileUtilities.CopyFiles copyFiles, out bool success)
        {
            string destinationDirectory = null;
            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
                Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
                sourceModule.Options.FilterOutputPaths(sourceOutputPaths, sourceFiles);
                if (0 == sourceFiles.Count)
                {
                    Opus.Core.Log.DebugMessage("No files to copy");
                    success = true;
                    return null;
                }

                PostBuildEventCopyFiles(sourceModule, destinationDirectory, sourceFiles);
            }

            if (null != copyFiles.SourceFiles)
            {
                if (null != destinationModule)
                {
                    PostBuildEventCopyFiles(destinationModule, destinationDirectory, copyFiles.SourceFiles);
                }
                else
                {
                    // TODO: this should probably be a Utility project
                    throw new Opus.Core.Exception("Unsupported copying explicit source files to an explicit directory");
                }
            }

            success = true;
            return null;
        }
    }
}
