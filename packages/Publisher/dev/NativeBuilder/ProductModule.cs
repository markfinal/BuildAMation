// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
#if true
        private void
        nativeCopyNodeLocation(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath)
        {
            foreach (var dir in directoriesToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

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

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);

            if (sourceKey.IsFileKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                Publisher.ProductModuleUtilities.CopyFileToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                Publisher.ProductModuleUtilities.CopySymlinkToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
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
#else
        private void
        CopyNodeLocation(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule moduleToCopy,
            Opus.Core.LocationKey sourceKey,
            Publisher.CopyFileLocationsAttribute attribute,
            string metaSubDirectory,
            Opus.Core.BaseModule primaryModule,
            string publishDirPath)
        {
            var moduleLocations = moduleToCopy.Locations;

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
            var subDirectory = attribute.CommonSubDirectory;
            if (!string.IsNullOrEmpty(metaSubDirectory))
            {
                subDirectory = metaSubDirectory;
            }

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);

            if (sourceKey.IsFileKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                Publisher.ProductModuleUtilities.CopyFileToLocation(
                    sourceLoc,
                    publishDirPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                Publisher.ProductModuleUtilities.CopySymlinkToLocation(
                    sourceLoc,
                    publishDirPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
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
#endif

#if true
        private void
        nativeCopyAdditionalDirectory(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo,
            string publishDirectoryPath)
        {
            foreach (var dir in directoriesToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
            Publisher.ProductModuleUtilities.CopyDirectoryToLocation(
                sourceLoc,
                publishDirectoryPath,
                subdirectory,
                directoryInfo.RenamedLeaf,
                moduleToBuild,
                publishedKey);
        }
#else
        private void
        CopyAdditionalDirectory(
            Opus.Core.BaseModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Publisher.PublishDirectory directoryInfo,
            Publisher.AdditionalDirectoriesAttribute attribute,
            string publishDirPath)
        {
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var subdirectory = attribute.CommonSubDirectory;
            Publisher.ProductModuleUtilities.CopyDirectoryToLocation(
                sourceLoc,
                publishDirPath,
                subdirectory,
                directoryInfo.RenamedLeaf,
                moduleToBuild,
                publishedKey);
        }
#endif

#if true
        private void
        nativeCopyInfoPList(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.NamedModuleLocation namedLocation)
        {
            var plistNode = Opus.Core.ModuleUtilities.GetNode(
                namedLocation.ModuleType,
                (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);

            var moduleToCopy = plistNode.Module;
            var keyToCopy = namedLocation.Key;

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                keyToCopy);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var contentsLoc = moduleToBuild.Locations[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
            var plistSourceLoc = moduleToCopy.Locations[keyToCopy];
            Publisher.ProductModuleUtilities.CopyFileToLocation(
                plistSourceLoc,
                contentsLoc,
                string.Empty,
                moduleToBuild,
                publishedKey);
        }
#else
        private void
        CopyInfoPList(
            Opus.Core.BaseModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.BaseModule moduleToCopy,
            Opus.Core.LocationKey keyToCopy)
        {
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                keyToCopy);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var contentsLoc = moduleToBuild.Locations[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
            var plistSourceLoc = moduleToCopy.Locations[keyToCopy];
            Publisher.ProductModuleUtilities.CopyFileToLocation(
                plistSourceLoc,
                contentsLoc,
                string.Empty,
                moduleToBuild,
                publishedKey);
        }
#endif

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
#if true
            Publisher.DelegateProcessing.Process(
                moduleToBuild,
                nativeCopyNodeLocation,
                nativeCopyAdditionalDirectory,
                nativeCopyInfoPList);
#else
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var locationMap = moduleToBuild.Locations;

            // Native build publishes to a new location away from the existing module builds
            var publishDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            var primaryNode = Publisher.ProductModuleUtilities.GetPrimaryTarget(moduleToBuild);
            if (null == primaryNode)
            {
                throw new Opus.Core.Exception("Unable to locate the primary target for publishing");
            }
            var primaryModule = primaryNode.Module;

            // gather all nodes that will be considered for publication
            var nodesToPublish = new Opus.Core.DependencyNodeCollection();
            nodesToPublish.Add(primaryNode);
            nodesToPublish.Add(moduleToBuild.OwningNode);
            if (null != primaryNode.ExternalDependents)
            {
                nodesToPublish.AddRange(primaryNode.ExternalDependents);
            }
            if (null != primaryNode.RequiredDependents)
            {
                nodesToPublish.AddRange(primaryNode.RequiredDependents);
            }

            // gather up the publishing metadata for those nodes
            var metaData = Publisher.ProductModuleUtilities.GetPublishingMetaData(moduleToBuild.OwningNode.Target, nodesToPublish);

            // for built files to copy...
            var copyFiles = metaData.FilterByType<Publisher.CopyFileLocationsAttribute>();
            // TODO: convert to var
            foreach (Publisher.ProductModuleUtilities.MetaData meta in copyFiles)
            {
                var nodeData = meta.Data as Opus.Core.Array<Publisher.PublishDependency>;
                if (null == nodeData)
                {
                    throw new Opus.Core.Exception("Meta data '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                        meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(Opus.Core.Array<Publisher.PublishDependency>).ToString());
                }

                var module = meta.Node.Module;
                var moduleLocations = module.Locations;
                foreach (var node in nodeData)
                {
                    if (node.SubDirectory != null)
                    {
                        Opus.Core.Log.MessageAll("Copy file '{0}' : '{1}' -> {2}/{3}", meta.Node.UniqueModuleName, node.Key.ToString(), publishDirPath, node.SubDirectory);
                    }
                    else
                    {
                        Opus.Core.Log.MessageAll("Copy file '{0}' : '{1}' -> {2}", meta.Node.UniqueModuleName, node.Key.ToString(), publishDirPath);
                    }

                    this.CopyNodeLocation(
                        moduleToBuild,
                        module,
                        node.Key,
                        meta.Attribute as Publisher.CopyFileLocationsAttribute,
                        node.SubDirectory,
                        primaryModule,
                        publishDirPath);
                }
            }

            // for additional files already on disk...
            var additionalDirs = metaData.FilterByType<Publisher.AdditionalDirectoriesAttribute>();
            // TODO: convert to var
            foreach (Publisher.ProductModuleUtilities.MetaData meta in additionalDirs)
            {
                var dirData = meta.Data as Publisher.PublishDirectory;
                if (null == dirData)
                {
                    throw new Opus.Core.Exception("Meta data '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                        meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(Publisher.PublishDirectory).ToString());
                }

                Opus.Core.Log.MessageAll("Additional dir '{0}' : '{1}' -> '{2}'", meta.Node.UniqueModuleName, dirData.Directory, publishDirPath);
                this.CopyAdditionalDirectory(
                    moduleToBuild,
                    primaryModule,
                    dirData,
                    meta.Attribute as Publisher.AdditionalDirectoriesAttribute,
                    publishDirPath);
            }

            var options = moduleToBuild.Options as Publisher.IPublishOptions;
            if (options.OSXApplicationBundle)
            {
                // for built info.plists...
                var infoPLists = metaData.FilterByType<Publisher.OSXInfoPListAttribute>();
                // TODO: convert to var
                foreach (Publisher.ProductModuleUtilities.MetaData meta in infoPLists)
                {
                    var nodeData = meta.Data as Publisher.NamedModuleLocation;
                    if (null == nodeData)
                    {
                        throw new Opus.Core.Exception("Meta data '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                            meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(Publisher.NamedModuleLocation).ToString());
                    }

                    var plistNode = Opus.Core.ModuleUtilities.GetNode(nodeData.ModuleType, (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);

                    Opus.Core.Log.MessageAll("Copy Info.plist file '{0}' : '{1}' -> '{2}'", meta.Node.UniqueModuleName, nodeData.Key, publishDirPath);
                    this.CopyInfoPList(
                        moduleToBuild,
                        primaryModule,
                        plistNode.Module,
                        nodeData.Key);
                }
            }
#endif

            success = true;
            return null;
        }
    }
}
