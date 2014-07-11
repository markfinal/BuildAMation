// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        private static Opus.Core.StringArray
        MakeCopyFileRecipe(
            string sourcePath,
            string destPath)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                recipeBuilder.AppendFormat("cmd.exe /c COPY {0} {1}", sourcePath, destPath);
            }
            else
            {
                recipeBuilder.AppendFormat("cp {0} {1}", sourcePath, destPath);
            }
            var recipe = recipeBuilder.ToString();
            // replace primary target with $@
            recipe = recipe.Replace(destPath, "$@");
            // TODO: too many inputs for some modules (map files, pdbs, etc) to just replace source with $<

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
        }

        private static Opus.Core.StringArray
        MakeCopyDirectoryRecipe(
            string sourcePath,
            string destPath)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                recipeBuilder.AppendFormat("cmd.exe /c XCOPY {0} {1} /E", sourcePath, destPath);
            }
            else
            {
                recipeBuilder.AppendFormat("cp -R {0} {1}", sourcePath, destPath);
            }
            var recipe = recipeBuilder.ToString();
            // replace primary target with $@
            recipe = recipe.Replace(destPath, "$@");
            // TODO: too many inputs for some modules (map files, pdbs, etc) to just replace source with $<

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
        }

        private static Opus.Core.StringArray
        MakeCopySymlinkRecipe(
            string sourcePath,
            string destPath,
            string subdirectory,
            string workingDir)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                throw new Opus.Core.Exception("Cannot copy symlinks on Windows");
            }
            else
            {
                recipeBuilder.AppendFormat("cd {0} && ln -sf $(shell readlink {1}) {2}/$(notdir {3})", workingDir, sourcePath, subdirectory, destPath);
            }
            var recipe = recipeBuilder.ToString();
            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
        }

        private void
        PublishDependents(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            string publishDirPath,
            Opus.Core.LocationArray dirsToCreate,
            MakeFile makeFile)
        {
            var node = moduleToBuild.OwningNode;

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
                var depNodeData = dependency.Data as MakeFileData;
                var depInputVariables = new MakeFileVariableDictionary();
                if (null != depNodeData)
                {
                    depInputVariables.Append(depNodeData.VariableDictionary);
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

                            var loc = module.Locations[key];
                            if (!loc.IsValid)
                            {
                                continue;
                            }
                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);

                            if (key.IsFileKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                                var depSourcePath = loc.GetSingleRawPath();
                                var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                    depSourcePath,
                                    publishDirPath,
                                    string.Empty,
                                    moduleToBuild,
                                    newKey);
                                var rule = new MakeFileRule(
                                    moduleToBuild,
                                    newKey,
                                    node.UniqueModuleName,
                                    dirsToCreate,
                                    depInputVariables,
                                    null,
                                    MakeCopyFileRecipe(depSourcePath, depDestPath));
                                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(newKey);
                                makeFile.RuleArray.Add(rule);
                            }
                            else if (key.IsSymlinkKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                                var depSourcePath = loc.GetSingleRawPath();
                                var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                    depSourcePath,
                                    publishDirPath,
                                    string.Empty,
                                    moduleToBuild,
                                    newKey);
                                var rule = new MakeFileRule(
                                    moduleToBuild,
                                    newKey,
                                    node.UniqueModuleName,
                                    dirsToCreate,
                                    depInputVariables,
                                    null,
                                    MakeCopySymlinkRecipe(depSourcePath, depDestPath, ".", publishDirPath));
                                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(newKey);
                                makeFile.RuleArray.Add(rule);
                            }
                            else
                            {
                                throw new Opus.Core.Exception("Unsupported location type");
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
                            var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);

                            // take the common subdirectory by default, otherwise override on a per Location basis
                            var subDirectory = attribute.CommonSubDirectory;
                            if (!string.IsNullOrEmpty(dep.SubDirectory))
                            {
                                subDirectory = dep.SubDirectory;
                            }

                            if (key.IsFileKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                                var depSourcePath = loc.GetSingleRawPath();
                                var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                    depSourcePath,
                                    publishDirPath,
                                    subDirectory,
                                    moduleToBuild,
                                    newKey);
                                var rule = new MakeFileRule(
                                    moduleToBuild,
                                    newKey,
                                    node.UniqueModuleName,
                                    dirsToCreate,
                                    depInputVariables,
                                    null,
                                    MakeCopyFileRecipe(depSourcePath, depDestPath));
                                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(newKey);
                                makeFile.RuleArray.Add(rule);
                            }
                            else if (key.IsSymlinkKey)
                            {
                                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                                var depSourcePath = loc.GetSingleRawPath();
                                var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                    depSourcePath,
                                    publishDirPath,
                                    subDirectory,
                                    moduleToBuild,
                                    newKey);
                                var rule = new MakeFileRule(
                                    moduleToBuild,
                                    newKey,
                                    node.UniqueModuleName,
                                    dirsToCreate,
                                    depInputVariables,
                                    null,
                                    MakeCopySymlinkRecipe(depSourcePath, depDestPath, subDirectory, publishDirPath));
                                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(newKey);
                                makeFile.RuleArray.Add(rule);
                            }
                            else
                            {
                                throw new Opus.Core.Exception("Unsupported location type");
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
            Opus.Core.LocationArray dirsToCreate,
            MakeFile makeFile)
        {
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as Publisher.IPublishOptions;
            if (options.OSXApplicationBundle)
            {
                var locationMap = moduleToBuild.Locations;
                var plistNodeData = Publisher.ProductModuleUtilities.GetOSXPListNodeData(moduleToBuild);
                if ((null != plistNodeData) && (plistNodeData.Node != null))
                {
                    var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, plistNodeData.Node.Module, plistNodeData.Key);
                    var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                    var contentsLoc = locationMap[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
                    var plistSourceLoc = plistNodeData.Node.Module.Locations[plistNodeData.Key];
                    var plistSourcePath = plistSourceLoc.GetSingleRawPath();
                    var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                        plistSourcePath,
                        contentsLoc,
                        string.Empty,
                        moduleToBuild,
                        newKey);
                    var rule = new MakeFileRule(
                        moduleToBuild,
                        newKey,
                        node.UniqueModuleName,
                        dirsToCreate,
                        null,
                        null,
                        MakeCopyFileRecipe(plistSourcePath, depDestPath));
                    rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(newKey);
                    makeFile.RuleArray.Add(rule);
                }
            }
        }

        private void
        PublishAdditionalDirectories(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            string publishDirPath,
            Opus.Core.LocationArray dirsToCreate,
            MakeFile makeFile)
        {
            var node = moduleToBuild.OwningNode;
            var additionalDirsData = Publisher.ProductModuleUtilities.GetAdditionalDirectoriesData(moduleToBuild);
            if (null != additionalDirsData)
            {
                var keyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(primaryNode.Module, additionalDirsData.DirectoryName);
                var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
                var sourceLoc = additionalDirsData.SourceDirectory;
                var sourcePath = sourceLoc.GetSingleRawPath();
                var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                    sourcePath,
                    publishDirPath,
                    string.Empty,
                    moduleToBuild,
                    newKey);
                var rule = new MakeFileRule(
                    moduleToBuild,
                    newKey,
                    node.UniqueModuleName,
                    dirsToCreate,
                    null,
                    null,
                    MakeCopyDirectoryRecipe(sourcePath, destPath));
                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(newKey);
                makeFile.RuleArray.Add(rule);
            }
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;

            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            var primaryNodeData = Publisher.ProductModuleUtilities.GetPrimaryNodeData(moduleToBuild);
            if (null == primaryNodeData)
            {
                success = true;
                return null;
            }

            var primaryNode = primaryNodeData.Node;
            var locationMap = moduleToBuild.Locations;
            var publishDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            var sourceLoc = primaryNode.Module.Locations[primaryNodeData.Key];
            var sourcePath = sourceLoc.GetSingleRawPath();
            var newKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, primaryNode.Module, primaryNodeData.Key);
            var primaryKey = new Opus.Core.LocationKey(newKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(sourcePath, publishDirPath, string.Empty, moduleToBuild, primaryKey);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var primaryNodeMakeData = primaryNode.Data as MakeFileData;
            var primaryInputVariables = new MakeFileVariableDictionary();
            primaryInputVariables.Append(primaryNodeMakeData.VariableDictionary);

            var primaryRule = new MakeFileRule(
                moduleToBuild,
                primaryKey,
                node.UniqueModuleName,
                dirsToCreate,
                primaryInputVariables,
                null,
                MakeCopyFileRecipe(sourcePath, destPath));
            primaryRule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>();
            primaryRule.OutputLocationKeys.AddUnique(primaryKey);
            makeFile.RuleArray.Add(primaryRule);

            this.PublishDependents(moduleToBuild, primaryNode, publishDirPath, dirsToCreate, makeFile);
            this.PublishOSXPList(moduleToBuild, primaryNode, dirsToCreate, makeFile);
            this.PublishAdditionalDirectories(moduleToBuild, primaryNode, publishDirPath, dirsToCreate, makeFile);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargets = makeFile.ExportedTargets;
            var exportedVariables = makeFile.ExportedVariables;
            var returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, null);
            success = true;
            return returnData;
        }
    }
}
