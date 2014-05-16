// <copyright file="CopyFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        private void PostBuildEventCopyFiles(Opus.Core.IModule moduleForPostEvents,
                                             string destinationDirectory,
                                             string sourceFile)
        {
            var sourceModuleNode = (moduleForPostEvents as Opus.Core.BaseModule).OwningNode;
            var nodeProjectData = sourceModuleNode.Data as IProject;
            if (null == nodeProjectData)
            {
                // there is no VCProj data to know which project to write the build event onto
                // try looking what this is a dependent for
                foreach (var dependeeNodes in sourceModuleNode.ExternalDependentFor)
                {
                    nodeProjectData = dependeeNodes.Data as IProject;
                    if (null != nodeProjectData)
                    {
                        break;
                    }
                }

                if ((null == nodeProjectData) && (null != sourceModuleNode.RequiredDependentFor))
                {
                    // try looking what this a requirement for
                    foreach (var requireeNodes in sourceModuleNode.RequiredDependentFor)
                    {
                        nodeProjectData = requireeNodes.Data as IProject;
                        if (null != nodeProjectData)
                        {
                            break;
                        }
                    }
                }

                if (null == nodeProjectData)
                {
                    throw new Opus.Core.Exception("Cannot locate any vcproj data to write the post-build event for");
                }
            }

            var configCollection = nodeProjectData.Configurations;
            var configurationName = configCollection.GetConfigurationNameForTarget(sourceModuleNode.Target);
            var configuration = configCollection[configurationName];

            var toolName = "VCPostBuildEventTool";
            var vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("IF NOT EXIST \"{0}\" MKDIR \"{0}\"{1}", destinationDirectory, System.Environment.NewLine);
            commandLine.AppendFormat("cmd.exe /c COPY \"{0}\" \"{1}\"{2}", sourceFile, destinationDirectory, System.Environment.NewLine);

            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == nodeProjectData.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == nodeProjectData.VSTarget)
                {
                    attributeName = "Command";
                }

                lock (vcPostBuildEventTool)
                {
                    if (vcPostBuildEventTool.HasAttribute(attributeName))
                    {
                        var currentValue = vcPostBuildEventTool[attributeName];
                        currentValue += commandLine.ToString();
                        vcPostBuildEventTool[attributeName] = currentValue;
                    }
                    else
                    {
                        vcPostBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                    }
                }
            }
        }

        public object Build(FileUtilities.CopyFile moduleToBuild, out bool success)
        {
            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            var baseOptions = moduleToBuild.Options;
#if true
            var copiedFilePath = moduleToBuild.Locations[FileUtilities.CopyFile.OutputFile].GetSinglePath();
#else
            var copiedFilePath = baseOptions.OutputPaths[FileUtilities.OutputFileFlags.CopiedFile];
#endif
            var destinationDirectory = System.IO.Path.GetDirectoryName(copiedFilePath);
            var node = moduleToBuild.OwningNode;
            var target = node.Target;

            var besideModuleType = moduleToBuild.BesideModuleType;
            if (null == besideModuleType)
            {
                var copyOptions = baseOptions as FileUtilities.ICopyFileOptions;
                if (null == copyOptions.SourceModuleType)
                {
                    Opus.Core.Log.MessageAll("VSSolution support for copying to arbitrary locations is unavailable");
                    success = true;
                    return null;
                }

                var copySourceModule = Opus.Core.ModuleUtilities.GetModule(copyOptions.SourceModuleType, (Opus.Core.BaseTarget)target);

                PostBuildEventCopyFiles(copySourceModule, destinationDirectory, sourceFilePath);
                success = true;
                return null;
            }

            var besideModule = Opus.Core.ModuleUtilities.GetModule(besideModuleType, (Opus.Core.BaseTarget)target);

            PostBuildEventCopyFiles(besideModule, destinationDirectory, sourceFilePath);

            success = true;
            return null;
        }
    }
}
