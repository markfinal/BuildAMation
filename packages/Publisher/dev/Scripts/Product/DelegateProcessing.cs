#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Publisher
{
    public static class DelegateProcessing
    {
        public delegate void
        CopyNodeLocationDelegate(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Bam.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context);

        public delegate void
        CopyAdditionalDirectoryDelegate(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Bam.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo,
            string publishDirectoryPath,
            object context);

        public delegate void
        CopyInfoPListDelegate(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Bam.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo,
            string publishDirectoryPath,
            object context);

        private static Bam.Core.DependencyNodeCollection
        RecursivelySearchDependencies(
            Bam.Core.DependencyNode node)
        {
            var newNodes = new Bam.Core.DependencyNodeCollection();
            if (null != node.ExternalDependents)
            {
                newNodes.AddRange(node.ExternalDependents);
            }
            if (null != node.RequiredDependents)
            {
                newNodes.AddRange(node.RequiredDependents);
            }

            var moreNewNodes = new Bam.Core.DependencyNodeCollection();
            foreach (var newNode in newNodes)
            {
                moreNewNodes.AddRange(RecursivelySearchDependencies(newNode));
            }
            newNodes.AddRange(moreNewNodes);

            return newNodes;
        }

        public static Bam.Core.DependencyNode
        Process(
            ProductModule moduleToBuild,
            CopyNodeLocationDelegate copyNode,
            CopyAdditionalDirectoryDelegate copyAdditionalDir,
            CopyInfoPListDelegate copyInfoPList,
            object context,
            bool publishBesidePrimary)
        {
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            var locationMap = moduleToBuild.Locations;

            // Native build publishes to a new location away from the existing module builds
            var publishDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            var primaryNode = Publisher.ProductModuleUtilities.GetPrimaryTarget(moduleToBuild);
            if (null == primaryNode)
            {
                throw new Bam.Core.Exception("Unable to locate the primary target for publishing");
            }
            var primaryModule = primaryNode.Module;

            // gather all nodes that will be considered for publication
            var nodesToPublish = new Bam.Core.DependencyNodeCollection();
            nodesToPublish.Add(primaryNode);
            nodesToPublish.Add(moduleToBuild.OwningNode);
            nodesToPublish.AddRange(RecursivelySearchDependencies(primaryNode));

            // gather up the publishing metadata for those nodes
            var metaData = Publisher.ProductModuleUtilities.GetPublishingMetaData(moduleToBuild.OwningNode.Target, nodesToPublish);

            if (publishBesidePrimary)
            {
                foreach (Publisher.ProductModuleUtilities.MetaData meta in metaData)
                {
                    if (meta.Node == primaryNode)
                    {
                        var data = meta.Data as Bam.Core.Array<Publisher.PublishDependency>;
                        if (null != data)
                        {
                            var mainDependency = data[0];
                            publishDirLoc = (primaryModule.Locations[mainDependency.Key] as Bam.Core.ScaffoldLocation).Base;
                            publishDirPath = publishDirLoc.GetSingleRawPath();
                            break;
                        }
                    }
                }
            }

            // publish static directories first, as future steps may copy files into them
            // for additional directories already on disk...
            var additionalDirs = metaData.FilterByType<Publisher.AdditionalDirectoriesAttribute>();
            // TODO: convert to var
            foreach (Publisher.ProductModuleUtilities.MetaData meta in additionalDirs)
            {
                var dirData = meta.Data as Publisher.PublishDirectory;
                if (null == dirData)
                {
                    throw new Bam.Core.Exception("Meta data '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                        meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(Publisher.PublishDirectory).ToString());
                }

                Bam.Core.Log.DebugMessage("Additional dir '{0}' : '{1}' -> '{2}'", meta.Node.UniqueModuleName, dirData.Directory, publishDirPath);
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
                var nodeData = meta.Data as Bam.Core.Array<Publisher.PublishDependency>;
                if (null == nodeData)
                {
                    throw new Bam.Core.Exception("Meta data '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                        meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(Bam.Core.Array<Publisher.PublishDependency>).ToString());
                }

                foreach (var node in nodeData)
                {
                    if (node.SubDirectory != null)
                    {
                        Bam.Core.Log.DebugMessage("Copy file '{0}' : '{1}' -> {2}/{3}", meta.Node.UniqueModuleName, node.Key.ToString(), publishDirPath, node.SubDirectory);
                    }
                    else
                    {
                        Bam.Core.Log.DebugMessage("Copy file '{0}' : '{1}' -> {2}", meta.Node.UniqueModuleName, node.Key.ToString(), publishDirPath);
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
                        Bam.Core.Log.DebugMessage("Ignoring Info.plist from '{0}' as it is not associated with the primary target", meta.Node.UniqueModuleName);
                        continue;
                    }

                    var nodeData = meta.Data as System.Type;
                    if (null == nodeData)
                    {
                        throw new Bam.Core.Exception("Meta data field called '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                            meta.Name, meta.Node.UniqueModuleName, meta.Data.GetType().ToString(), typeof(System.Type).ToString());
                    }

                    var plistNode = Bam.Core.ModuleUtilities.GetNode(nodeData, (Bam.Core.BaseTarget)moduleToBuild.OwningNode.Target);

                    var plistNodes = new Bam.Core.DependencyNodeCollection();
                    plistNodes.Add(plistNode);
                    var plistMetaData = Publisher.ProductModuleUtilities.GetPublishingMetaData(moduleToBuild.OwningNode.Target, plistNodes);

                    // TODO: convert to var
                    foreach (Publisher.ProductModuleUtilities.MetaData plistMeta in plistMetaData)
                    {
                        var plistNodeData = plistMeta.Data as Publisher.PublishDependency;
                        if (null == plistNodeData)
                        {
                            throw new Bam.Core.Exception("Meta data field called '{0}' in '{1}' was of unexpected type '{2}'. Expected '{3}'",
                                plistMeta.Name, plistMeta.Node.UniqueModuleName, plistMeta.Data.GetType().ToString(), typeof(Publisher.PublishDependency).ToString());
                        }

                        Bam.Core.Log.DebugMessage("Copy Info.plist file '{0}' : '{1}' -> '{2}'", plistMeta.Node.UniqueModuleName, plistNodeData.Key, publishDirPath);

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
