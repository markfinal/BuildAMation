// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        private static void
        CopyNodes(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo)
        {
            var toCopy = meta.Node;
            var keyToCopy = nodeInfo.Key;
            var toProject = primaryModule.OwningNode.Data as IProject;

            var configCollection = toProject.Configurations;
            var configurationName = configCollection.GetConfigurationNameForTarget((Opus.Core.BaseTarget)toCopy.Target); // TODO: not accurate
            var configuration = configCollection[configurationName];

            var toolName = "VCPostBuildEventTool";
            var vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            var sourceLoc = toCopy.Module.Locations[keyToCopy];
            if (!sourceLoc.IsValid)
            {
                return;
            }
            var sourcePath = sourceLoc.GetSingleRawPath();

            var destinationDir = configuration.OutputDirectory;
            var destinationDirPath = destinationDir.GetSingleRawPath();

            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;
            var nodeSpecificSubdirectory = nodeInfo.SubDirectory;
            if (!string.IsNullOrEmpty(nodeSpecificSubdirectory))
            {
                subDirectory = nodeSpecificSubdirectory;
            }

            var newKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(toCopy.Module, toCopy.Module, keyToCopy);
            var primaryKey = new Opus.Core.LocationKey(newKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                sourcePath,
                destinationDirPath,
                subDirectory,
                string.Empty,
                moduleToBuild,
                primaryKey);

            if (sourcePath == destPath)
            {
                Opus.Core.Log.DebugMessage("Skipping copying '{0}' as it would go to the same location", sourcePath);
                return;
            }

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("cmd.exe /c COPY /Y \"{0}\" \"{1}\"{2}", sourcePath, destPath, System.Environment.NewLine);

            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == toProject.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == toProject.VSTarget)
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

        private static void
        CopyDirectory(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo)
        {
            var toProject = primaryModule.OwningNode.Data as IProject;

            var configCollection = toProject.Configurations;
            var configurationName = configCollection.GetConfigurationNameForTarget((Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);
            var configuration = configCollection[configurationName];

            var toolName = "VCPostBuildEventTool";
            var vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            var sourceLoc = directoryInfo.DirectoryLocation;
            if (!sourceLoc.IsValid)
            {
                return;
            }
            var sourcePath = sourceLoc.GetSingleRawPath();
            var lastDir = System.IO.Path.GetFileName(sourcePath);

            var destinationDir = configuration.OutputDirectory;
            var destinationDirPath = System.IO.Path.Combine(destinationDir.GetSingleRawPath(), lastDir);

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("IF NOT EXIST {0} MKDIR {0}{1}", destinationDirPath, System.Environment.NewLine);
            commandLine.AppendFormat("cmd.exe /c XCOPY /E /Y \"{0}\" \"{1}\"{2}", sourcePath, destinationDirPath, System.Environment.NewLine);

            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == toProject.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == toProject.VSTarget)
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

        private void
        nativeCopyNodeLocation(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context)
        {
            var moduleToCopy = meta.Node.Module;
            var moduleLocations = moduleToCopy.Locations;

            var sourceKey = nodeInfo.Key;
            if (!moduleLocations.Contains(sourceKey))
            {
                return;
            }

            var sourceLoc = moduleLocations[sourceKey];
            if (!sourceLoc.IsValid)
            {
                return;
            }

            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;
            var nodeSpecificSubdirectory = nodeInfo.SubDirectory;
            if (!string.IsNullOrEmpty(nodeSpecificSubdirectory))
            {
                subDirectory = nodeSpecificSubdirectory;
            }

            // TODO: should really be using this, in case we need to refer to published locations after this module
            /*
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);
                */

            if (sourceKey.IsFileKey)
            {
                //var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                CopyNodes(
                    moduleToBuild,
                    primaryModule,
                    meta,
                    nodeInfo);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                //var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                CopyNodes(
                    moduleToBuild,
                    primaryModule,
                    meta,
                    nodeInfo);
            }
            else if (sourceKey.IsDirectoryKey)
            {
                throw new Opus.Core.Exception("Directories cannot be published yet");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported Location type");
            }
        }

        private void
        nativeCopyAdditionalDirectory(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo,
            string publishDirectoryPath,
            object context)
        {
            // TODO: should be using this
            /*
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            */
            var sourceLoc = directoryInfo.DirectoryLocation;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
            CopyDirectory(
                moduleToBuild,
                primaryModule,
                meta,
                directoryInfo);
        }

        private void
        nativeCopyInfoPList(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context)
        {
            throw new Opus.Core.Exception("Info.plists are OSX specific. Not supported with VisualStudio projects");
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            Publisher.DelegateProcessing.Process(
                moduleToBuild,
                nativeCopyNodeLocation,
                nativeCopyAdditionalDirectory,
                nativeCopyInfoPList,
                null,
                true);

            success = true;
            return null;
        }
    }
}
