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
        MakeCopySymlinkRecipe(
            string sourcePath,
            string destPath,
            string workingDir)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                throw new Opus.Core.Exception("Cannot copy symlinks on Windows");
            }
            else
            {
                recipeBuilder.AppendFormat("cd {0} && ln -sf $(shell readlink {1}) $(notdir {2})", workingDir, sourcePath, destPath);
            }
            var recipe = recipeBuilder.ToString();
            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
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
            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(sourcePath, publishDirPath, moduleToBuild, primaryKey);

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
            makeFile.RuleArray.Add(primaryRule);

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
                    foreach (var key in candidateData)
                    {
                        var loc = module.Locations[key];
                        var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);

                        if (key.IsFileKey)
                        {
                            var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                            var depSourcePath = loc.GetSingleRawPath();
                            var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                depSourcePath,
                                publishDirPath,
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
                            makeFile.RuleArray.Add(rule);
                        }
                        else if (key.IsSymlinkKey)
                        {
                            var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                            var depSourcePath = loc.GetSingleRawPath();
                            var depDestPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                                depSourcePath,
                                publishDirPath,
                                moduleToBuild,
                                newKey);
                            var rule = new MakeFileRule(
                                moduleToBuild,
                                newKey,
                                node.UniqueModuleName,
                                dirsToCreate,
                                depInputVariables,
                                null,
                                MakeCopySymlinkRecipe(depSourcePath, depDestPath, publishDirPath));
                            makeFile.RuleArray.Add(rule);
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Unsupported location type");
                        }
                    }
                }
            }

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
