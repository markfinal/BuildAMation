// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
#if false
        private void
        PublishDependents(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            string publishDirPath)
        {
            var dependents = new Opus.Core.DependencyNodeCollection();
            if (null != primaryNode.ExternalDependents)
            {
                dependents.AddRange(primaryNode.ExternalDependents);
            }
            if (null != primaryNode.RequiredDependents)
            {
                dependents.AddRange(primaryNode.RequiredDependents);
            }

            foreach (var dependency in dependents)
            {
                var module = dependency.Module;
                var moduleType = module.GetType();
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic;
                var fields = moduleType.GetFields(flags);
                foreach (var field in fields)
                {
                    var candidates = field.GetCustomAttributes(typeof(Publisher.CopyFileLocationsAttribute), false);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }
                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish module dependency found");
                    }
                    var attribute = candidates[0] as Publisher.CopyFileLocationsAttribute;
                    var matchesTarget = Opus.Core.TargetUtilities.MatchFilters(moduleToBuild.OwningNode.Target, attribute);
                    if (!matchesTarget)
                    {
                        continue;
                    }
                    var candidateData = field.GetValue(module) as Opus.Core.Array<Opus.Core.LocationKey>;
                    if (null != candidateData)
                    {
                        foreach (var key in candidateData)
                        {
                            if (!module.Locations.Contains(key))
                            {
                                continue;
                            }

                            var loc = module.Locations[key];
                            if (!loc.IsValid)
                            {
                                continue;
                            }

                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);
                            if (key.IsFileKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                                Publisher.ProductModuleUtilities.CopyFileToLocation(loc, publishDirPath, string.Empty, moduleToBuild, newKey);
                            }
                            else if (key.IsSymlinkKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                                Publisher.ProductModuleUtilities.CopySymlinkToLocation(loc, publishDirPath, string.Empty, moduleToBuild, newKey);
                            }
                            else if (key.IsDirectoryKey)
                            {
                                throw new Opus.Core.Exception("Directories cannot be published yet");
                            }
                            else
                            {
                                throw new Opus.Core.Exception("Unsupported Location type");
                            }
                        }
                    }
                    else
                    {
                        var candidateData2 = field.GetValue(module) as Opus.Core.Array<Publisher.PublishDependency>;
                        if (null == candidateData2)
                        {
                            throw new Opus.Core.Exception("Unrecognized type for dependency data");
                        }

                        foreach (var dep in candidateData2)
                        {
                            var key = dep.Key;
                            if (!module.Locations.Contains(key))
                            {
                                continue;
                            }

                            var loc = module.Locations[key];
                            if (!loc.IsValid)
                            {
                                continue;
                            }

                            // take the common subdirectory by default, otherwise override on a per Location basis
                            var subDirectory = attribute.CommonSubDirectory;
                            if (!string.IsNullOrEmpty(dep.SubDirectory))
                            {
                                subDirectory = dep.SubDirectory;
                            }

                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);
                            if (key.IsFileKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                                Publisher.ProductModuleUtilities.CopyFileToLocation(loc, publishDirPath, subDirectory, moduleToBuild, newKey);
                            }
                            else if (key.IsSymlinkKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                                Publisher.ProductModuleUtilities.CopySymlinkToLocation(loc, publishDirPath, subDirectory, moduleToBuild, newKey);
                            }
                            else if (key.IsDirectoryKey)
                            {
                                throw new Opus.Core.Exception("Directories cannot be published yet");
                            }
                            else
                            {
                                throw new Opus.Core.Exception("Unsupported Location type");
                            }
                        }

                    }
                }
            }
        }

        private void
        PublishOSXPList(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            Opus.Core.LocationMap locationMap)
        {
            var options = moduleToBuild.Options as Publisher.IPublishOptions;
            if (options.OSXApplicationBundle)
            {
                var plistNodeData = Publisher.ProductModuleUtilities.GetOSXPListNodeData(moduleToBuild);
                if ((null != plistNodeData) && (plistNodeData.Node != null))
                {
                    var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, plistNodeData.Node.Module, plistNodeData.Key);
                    var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                    var contentsLoc = locationMap[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
                    var plistSourceLoc = plistNodeData.Node.Module.Locations[plistNodeData.Key];
                    Publisher.ProductModuleUtilities.CopyFileToLocation(plistSourceLoc, contentsLoc, string.Empty, moduleToBuild, newKey);
                }
            }
        }

        private void
        PublishAdditionalDirectories(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            string publishDirPath)
        {
            var additionalDirsData = Publisher.ProductModuleUtilities.GetAdditionalDirectoriesData(moduleToBuild);
            if (null != additionalDirsData)
            {
                var keyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(primaryNode.Module, additionalDirsData.DirectoryName);
                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
                var sourceLoc = additionalDirsData.SourceDirectory;
                Publisher.ProductModuleUtilities.CopyDirectoryToLocation(sourceLoc, publishDirPath, string.Empty, moduleToBuild, newKey);
            }
        }
#endif

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

        private void
        CopyAdditionalDirectory(
            Opus.Core.BaseModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Publisher.PublishDirectory directoryInfo,
            string publishDirPath)
        {
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            Publisher.ProductModuleUtilities.CopyDirectoryToLocation(
                sourceLoc,
                publishDirPath,
                string.Empty, // TODO: copy to subdirectory
                moduleToBuild,
                publishedKey);
        }

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

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
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
            if (null != primaryNode.ExternalDependents)
            {
                nodesToPublish.AddRange(primaryNode.ExternalDependents);
            }
            if (null != primaryNode.RequiredDependents)
            {
                nodesToPublish.AddRange(primaryNode.RequiredDependents);
            }

            // gather up the publishing metadata for those nodes
            var metaData = Publisher.ProductModuleUtilities.GetPublishingMetaData(nodesToPublish);

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
                    var nodeData = meta.Data as Opus.Core.Array<Publisher.PublishDependency>;
                    if (null == nodeData)
                    {
                        throw new Opus.Core.Exception("Meta data '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                            meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(Opus.Core.Array<Publisher.PublishDependency>).ToString());
                    }

                    foreach (var node in nodeData)
                    {
                        Opus.Core.Log.MessageAll("Copy Info.plist file '{0}' : '{1}' -> '{2}'", meta.Node.UniqueModuleName, node.Key.ToString(), publishDirPath);
                        this.CopyInfoPList(
                            moduleToBuild,
                            primaryModule,
                            meta.Node.Module,
                            node.Key);
                    }
                }
            }

#if false
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var locationMap = moduleToBuild.Locations;
            var publishDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            var sourceLoc = primaryNode.Module.Locations[primaryNodeData.Key];
            var publishedSourceKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, primaryNode.Module, primaryNodeData.Key);
            var publishedKey = new Opus.Core.LocationKey(publishedSourceKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            Publisher.ProductModuleUtilities.CopyFileToLocation(sourceLoc, publishDirPath, string.Empty, moduleToBuild, publishedKey);

            this.PublishDependents(moduleToBuild, primaryNode, publishDirPath);
            this.PublishOSXPList(moduleToBuild, primaryNode, locationMap);
            this.PublishAdditionalDirectories(moduleToBuild, primaryNode, publishDirPath);
#endif

            success = true;
            return null;
        }
    }
}
