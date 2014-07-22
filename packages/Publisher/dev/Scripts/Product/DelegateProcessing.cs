// <copyright file="DelegateProcessing.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public static class DelegateProcessing
    {
        public delegate void
        CopyNodeLocationDelegate(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context);

        public delegate void
        CopyAdditionalDirectoryDelegate(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo,
            string publishDirectoryPath,
            object context);

        public delegate void
        CopyInfoPListDelegate(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context);

        public static Opus.Core.DependencyNode
        Process(
            ProductModule moduleToBuild,
            CopyNodeLocationDelegate copyNode,
            CopyAdditionalDirectoryDelegate copyAdditionalDir,
            CopyInfoPListDelegate copyInfoPList,
            object context)
        {
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
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

            // publish static directories first, as future steps may copy files into them
            // for additional directories already on disk...
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
                copyAdditionalDir(
                    moduleToBuild,
                    primaryModule,
                    dirsToCreate,
                    meta,
                    dirData,
                    publishDirPath,
                    context);
            }

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

                    copyNode(
                        moduleToBuild,
                        primaryModule,
                        dirsToCreate,
                        meta,
                        node,
                        publishDirPath,
                        context);
                }
            }

            var options = moduleToBuild.Options as Publisher.IPublishOptions;
            if (options.OSXApplicationBundle)
            {
                // for built info.plists...
                var infoPLists = metaData.FilterByType<Publisher.OSXInfoPListAttribute>();
                // TODO: convert to var
                foreach (Publisher.ProductModuleUtilities.MetaData meta in infoPLists)
                {
                    if (meta.Node != moduleToBuild.OwningNode)
                    {
                        Opus.Core.Log.DebugMessage("Ignoring Info.plist from '{0}' as it is not associated with the primary target", meta.Node.UniqueModuleName);
                        continue;
                    }

                    var nodeData = meta.Data as System.Type;
                    if (null == nodeData)
                    {
                        throw new Opus.Core.Exception("Meta data field called '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                            meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(System.Type).ToString());
                    }

                    var plistNode = Opus.Core.ModuleUtilities.GetNode(nodeData, (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);

                    var plistNodes = new Opus.Core.DependencyNodeCollection();
                    plistNodes.Add(plistNode);
                    var plistMetaData = Publisher.ProductModuleUtilities.GetPublishingMetaData(moduleToBuild.OwningNode.Target, plistNodes);

                    // TODO: convert to var
                    foreach (Publisher.ProductModuleUtilities.MetaData plistMeta in plistMetaData)
                    {
                        var plistNodeData = plistMeta.Data as Publisher.PublishDependency;
                        if (null == plistNodeData)
                        {
                            throw new Opus.Core.Exception("Meta data field called '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                                plistMeta.Name, plistMeta.Node.UniqueModuleName, plistMeta.Data.GetType().ToString(), typeof(Publisher.PublishDependency).ToString());
                        }

                        Opus.Core.Log.MessageAll("Copy Info.plist file '{0}' : '{1}' -> '{2}'", plistMeta.Node.UniqueModuleName, plistNodeData.Key, publishDirPath);

                        copyInfoPList(
                            moduleToBuild,
                            primaryModule,
                            dirsToCreate,
                            plistMeta,
                            plistNodeData,
                            publishDirPath,
                            context);
                    }
                }
            }

            return primaryNode;
        }
    }
}
