// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
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
                    var candidates = field.GetCustomAttributes(typeof(Publisher.PublishModuleDependencyAttribute), false);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }
                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish module dependency found");
                    }
                    var attribute = candidates[0] as Publisher.PublishModuleDependencyAttribute;
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

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var primaryNodeData = Publisher.ProductModuleUtilities.GetPrimaryNodeData(moduleToBuild);
            if (null == primaryNodeData)
            {
                success = true;
                return null;
            }

            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var primaryNode = primaryNodeData.Node;
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

            success = true;
            return null;
        }
    }
}
