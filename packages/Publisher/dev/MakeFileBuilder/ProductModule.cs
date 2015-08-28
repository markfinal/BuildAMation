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
namespace V2
{
    public sealed class MakeFilePackager :
        IPackagePolicy
    {
        private static void
        CopyFileRule(
            MakeFileBuilder.V2.MakeFileMeta meta,
            MakeFileBuilder.V2.MakeFileMeta sourceMeta,
            MakeFileBuilder.V2.Rule parentRule,
            string outputDirectory,
            Bam.Core.V2.TokenizedString sourcePath)
        {
            var copyRule = meta.AddRule();
            var target = copyRule.AddTarget(Bam.Core.V2.TokenizedString.Create(outputDirectory + "/" + System.IO.Path.GetFileName(sourcePath.Parse()), null));

            // TODO: there needs to be a mapping from this path to any existing targets so that the target variable names can be used
            copyRule.AddPrerequisite(sourcePath);

            var command = new System.Text.StringBuilder();
            command.AppendFormat("cp -fv $< $@");
            copyRule.AddShellCommand(command.ToString());

            parentRule.AddPrerequisite(target);

            meta.CommonMetaData.Directories.AddUnique(outputDirectory);
        }

        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module,
            System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString,
            PackageReference>> packageObjects)
        {
            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(Bam.Core.V2.TokenizedString.Create("publish", null, verbatim:true), isPhony:true);

            foreach (var module in packageObjects)
            {
                var moduleMeta = module.Key.MetaData as MakeFileBuilder.V2.MakeFileMeta;
                foreach (var path in module.Value)
                {
                    if (path.Value.IsMarker)
                    {
                        var outputDir = packageRoot.Parse();
                        if (null != path.Value.SubDirectory)
                        {
                            outputDir = System.IO.Path.Combine(outputDir, path.Value.SubDirectory);
                        }

                        CopyFileRule(meta, moduleMeta, rule, outputDir, path.Key);
                        path.Value.DestinationDir = outputDir;
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, path.Value.SubDirectory));
                            CopyFileRule(meta, moduleMeta, rule, destinationDir, path.Key);
                            path.Value.DestinationDir = destinationDir;
                        }
                    }
                }
            }
        }
    }
}
}
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        private static Bam.Core.StringArray
        MakeCopyFileRecipe(
            string sourcePath,
            string destPath)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Bam.Core.OSUtilities.IsWindowsHosting)
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

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
        }

        private static Bam.Core.StringArray
        MakeCopyDirectoryRecipe(
            string sourcePath,
            string destPath)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Bam.Core.OSUtilities.IsWindowsHosting)
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

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
        }

        private static Bam.Core.StringArray
        MakeCopySymlinkRecipe(
            string sourcePath,
            string destPath,
            string subdirectory,
            string workingDir)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                throw new Bam.Core.Exception("Cannot copy symlinks on Windows");
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
            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);
            return recipes;
        }

        private void
        nativeCopyNodeLocation(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Bam.Core.LocationArray directoriesToCreate,
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
                    Bam.Core.DirectoryLocation.Get(intendedSubDir, Bam.Core.Location.EExists.WillExist));
            }

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);

            if (sourceKey.IsFileKey)
            {
                var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
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
                rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(publishedKey);
                makeFile.RuleArray.AddUnique(rule);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
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
                rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(publishedKey);
                makeFile.RuleArray.AddUnique(rule);
            }
            else if (sourceKey.IsDirectoryKey)
            {
                throw new Bam.Core.Exception("Directories cannot be published yet");
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported Location type");
            }
        }

        private void
        nativeCopyAdditionalDirectory(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Bam.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo,
            string publishDirectoryPath,
            object context)
        {
            var makeFile = context as MakeFile;
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Directory);
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
            rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(publishedKey);
            makeFile.RuleArray.AddUnique(rule);
        }

        private void
        nativeCopyInfoPList(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Bam.Core.LocationArray directoriesToCreate,
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
            var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
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
            rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(publishedKey);
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
