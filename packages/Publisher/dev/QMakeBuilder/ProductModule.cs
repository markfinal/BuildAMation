// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        private static void
        InstallFile(
            string destinationDirectory,
            QMakeData proData)
        {
            var customRules = new Opus.Core.StringArray();
            customRules.Add(System.String.Format("target.path={0}", destinationDirectory));
            customRules.Add(System.String.Format("INSTALLS+=target"));
            if (null == proData.CustomRules)
            {
                proData.CustomRules = customRules;
            }
            else
            {
                proData.CustomRules.AddRangeUnique(customRules);
            }
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var options = moduleToBuild.Options as Publisher.IPublishOptions;

            var primaryNodeData = Publisher.ProductModuleUtilities.GetPrimaryNodeData(moduleToBuild);
            if (null == primaryNodeData)
            {
                success = true;
                return null;
            }

            var primaryNode = primaryNodeData.Node;
            if (options.OSXApplicationBundle)
            {
                var data = primaryNode.Data as QMakeData;
                data.OSXApplicationBundle = true;
            }

            var locationMap = primaryNode.Module.Locations;
            var publishDirLoc = (locationMap[primaryNodeData.Key] as Opus.Core.ScaffoldLocation).Base;
            var publishDirPath = publishDirLoc.GetSingleRawPath();

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
                var proData = dependency.Data as QMakeData;
                if (null == proData)
                {
                    continue;
                }

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

                            var sourceLoc = module.Locations[key];
                            if (!sourceLoc.IsValid)
                            {
                                continue;
                            }
                            var sourcePath = sourceLoc.GetSingleRawPath();

                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);
                            var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(sourcePath, publishDirPath, string.Empty, moduleToBuild, newKey);
                            var destDir = System.IO.Path.GetDirectoryName(destPath);

                            InstallFile(destDir, proData);
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

                            var sourceLoc = module.Locations[key];
                            if (!sourceLoc.IsValid)
                            {
                                continue;
                            }
                            var sourcePath = sourceLoc.GetSingleRawPath();

                            // take the common subdirectory by default, otherwise override on a per Location basis
                            var subDirectory = attribute.CommonSubDirectory;
                            if (!string.IsNullOrEmpty(dep.SubDirectory))
                            {
                                subDirectory = dep.SubDirectory;
                            }

                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);
                            var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(sourcePath, publishDirPath, subDirectory, moduleToBuild, newKey);
                            var destDir = System.IO.Path.GetDirectoryName(destPath);

                            InstallFile(destDir, proData);
                        }
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
