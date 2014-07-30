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
                if (string.IsNullOrEmpty(subdirectory))
                {
                    recipeBuilder.AppendFormat("cd {0} && ln -sf $(shell readlink {1}) $(notdir {2})", workingDir, sourcePath, destPath);
                }
                else
                {
                    recipeBuilder.AppendFormat("cd {0}/{2} && ln -sf $(shell readlink {1}) $(notdir {3})", workingDir, sourcePath, subdirectory, destPath);
                }
            }
            var recipe = recipeBuilder.ToString();
            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
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
            var makeFile = context as MakeFile;
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
            var sourcePath = sourceLoc.GetSingleRawPath();

            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;
            var nodeSpecificSubdirectory = nodeInfo.SubDirectory;
            if (!string.IsNullOrEmpty(nodeSpecificSubdirectory))
            {
                subDirectory = nodeSpecificSubdirectory;
            }
            if (!string.IsNullOrEmpty(subDirectory) &&
                subDirectory != ".")
            {
                var intendedSubDir = System.IO.Path.Combine(publishDirectoryPath, subDirectory);
                directoriesToCreate.AddUnique(
                    Opus.Core.DirectoryLocation.Get(intendedSubDir, Opus.Core.Location.EExists.WillExist));
            }

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);

            if (sourceKey.IsFileKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                var destinationPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                    sourcePath,
                    publishDirectoryPath,
                    subDirectory,
                    string.Empty,
                    moduleToBuild,
                    publishedKey);
                var rule = new MakeFileRule(
                    moduleToBuild,
                    publishedKey,
                    meta.Node.UniqueModuleName,
                    directoriesToCreate,
                    null, // depInputVariables, TODO: Might have to re-add this
                    null,
                    MakeCopyFileRecipe(sourcePath, destinationPath));
                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(publishedKey);
                makeFile.RuleArray.AddUnique(rule);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
                var destinationPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                    sourcePath,
                    publishDirectoryPath,
                    subDirectory,
                    string.Empty,
                    moduleToBuild,
                    publishedKey);
                var rule = new MakeFileRule(
                    moduleToBuild,
                    publishedKey,
                    meta.Node.UniqueModuleName,
                    directoriesToCreate,
                    null, // depInputVariables, TODO: Might have to re-add this
                    null,
                    MakeCopySymlinkRecipe(sourcePath, destinationPath, subDirectory, publishDirectoryPath));
                rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(publishedKey);
                makeFile.RuleArray.AddUnique(rule);
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
            var makeFile = context as MakeFile;
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var sourcePath = sourceLoc.GetSingleRawPath();
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;

            var destinationPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                sourcePath,
                publishDirectoryPath,
                subdirectory,
                directoryInfo.RenamedLeaf,
                moduleToBuild,
                publishedKey);
            var rule = new MakeFileRule(
                moduleToBuild,
                publishedKey,
                meta.Node.UniqueModuleName,
                directoriesToCreate,
                null, // depInputVariables, TODO: Might have to re-add this
                null,
                MakeCopyDirectoryRecipe(sourcePath, destinationPath));
            rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(publishedKey);
            makeFile.RuleArray.AddUnique(rule);
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
            var makeFile = context as MakeFile;
            var plistNode = meta.Node;

            var moduleToCopy = plistNode.Module;
            var keyToCopy = nodeInfo.Key;

            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                keyToCopy);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var contentsLoc = moduleToBuild.Locations[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
            var plistSourceLoc = moduleToCopy.Locations[keyToCopy];
            var plistSourcePath = plistSourceLoc.GetSingleRawPath();

            var destinationPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                plistSourcePath,
                contentsLoc,
                subDirectory,
                string.Empty,
                moduleToBuild,
                publishedKey);
            var rule = new MakeFileRule(
                moduleToBuild,
                publishedKey,
                meta.Node.UniqueModuleName,
                directoriesToCreate,
                null, // depInputVariables, TODO: Might have to re-add this
                null,
                MakeCopyFileRecipe(plistSourcePath, destinationPath));
            rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(publishedKey);
            makeFile.RuleArray.AddUnique(rule);
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            Publisher.DelegateProcessing.Process(
                moduleToBuild,
                nativeCopyNodeLocation,
                nativeCopyAdditionalDirectory,
                nativeCopyInfoPList,
                makeFile,
                false);

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
