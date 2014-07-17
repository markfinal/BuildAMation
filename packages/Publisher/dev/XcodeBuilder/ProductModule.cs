// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
#if true
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
#if false
                Publisher.ProductModuleUtilities.CopyFileToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
#endif
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Symlink);
#if false
                Publisher.ProductModuleUtilities.CopySymlinkToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
#endif
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
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Opus.Core.LocationKey(publishedKeyName, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
#if false
            Publisher.ProductModuleUtilities.CopyDirectoryToLocation(
                sourceLoc,
                publishDirectoryPath,
                subdirectory,
                directoryInfo.RenamedLeaf,
                moduleToBuild,
                publishedKey);
#endif
        }

        private void
        nativeCopyInfoPList(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.BaseModule primaryModule,
            Opus.Core.LocationArray directoriesToCreate,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.NamedModuleLocation namedLocation,
            string publishDirectoryPath,
            object context)
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

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            Publisher.DelegateProcessing.Process(
                moduleToBuild,
                nativeCopyNodeLocation,
                nativeCopyAdditionalDirectory,
                nativeCopyInfoPList,
                null);

            success = true;
            return null;
        }
#else
        private static void
        CopyNodes(
            Publisher.ProductModule moduleToBuild,
            PBXProject project,
            Opus.Core.DependencyNode toCopy,
            PBXNativeTarget nativeTarget,
            string subdirectory)
        {
            var nameOfBuildPhase = System.String.Format("Copy files for {0} to {1}", nativeTarget.Name, subdirectory);
            var copyFilesBuildPhase = project.CopyFilesBuildPhases.Get(nameOfBuildPhase, moduleToBuild.OwningNode.ModuleName);
            copyFilesBuildPhase.SubFolder = PBXCopyFilesBuildPhase.ESubFolder.Executables;
            if (!string.IsNullOrEmpty(subdirectory))
            {
                copyFilesBuildPhase.DestinationPath = subdirectory;
            }
            nativeTarget.BuildPhases.AddUnique(copyFilesBuildPhase);

            var copySourceNativeTarget = toCopy.Data as PBXNativeTarget;

            // need a different copy of the build file, to live in the CopyFiles build phase
            // but still referencing the same PBXFileReference
            var type = copySourceNativeTarget.ProductReference.Type;
            if (type == PBXFileReference.EType.DynamicLibrary)
            {
                type = PBXFileReference.EType.ReferencedDynamicLibrary;
            }
            var relativePath = Opus.Core.RelativePathUtilities.GetPath(copySourceNativeTarget.ProductReference.FullPath, project.RootUri);
            var dependentFileRef = project.FileReferences.Get(toCopy.UniqueModuleName, type, relativePath, project.RootUri);
            var buildFile = project.BuildFiles.Get(toCopy.UniqueModuleName, dependentFileRef, copyFilesBuildPhase);
            if (null == buildFile)
            {
                throw new Opus.Core.Exception("Build file not available");
            }

            project.MainGroup.Children.AddUnique(dependentFileRef);
        }

        private void
        PublishDependencies(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            PBXProject project,
            PBXNativeTarget primaryPBXNativeTarget)
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

                            CopyNodes(moduleToBuild, project, module.OwningNode, primaryPBXNativeTarget, string.Empty);
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

                            // take the common subdirectory by default, otherwise override on a per Location basis
                            var subDirectory = attribute.CommonSubDirectory;
                            if (!string.IsNullOrEmpty(dep.SubDirectory))
                            {
                                subDirectory = dep.SubDirectory;
                            }

                            CopyNodes(moduleToBuild, project, module.OwningNode, primaryPBXNativeTarget, subDirectory);
                        }
                    }
                }
            }
        }

        private void
        PublishAdditionalDirectories(
            Publisher.ProductModule moduleToBuild,
            Opus.Core.DependencyNode primaryNode,
            PBXProject project,
            PBXNativeTarget primaryPBXNativeTarget)
        {
            var additionalDirsData = Publisher.ProductModuleUtilities.GetAdditionalDirectoriesData(moduleToBuild);
            if (null != additionalDirsData)
            {
                var sourceLoc = additionalDirsData.SourceDirectory;
                var sourcePath = sourceLoc.GetSingleRawPath();
                var lastDir = System.IO.Path.GetFileName(sourcePath);
                var destDir = primaryNode.Module.Locations[C.Application.OutputDir].GetSingleRawPath();
                var dest = System.IO.Path.Combine(destDir, "$EXECUTABLE_FOLDER_PATH");
                dest = System.IO.Path.Combine(dest, lastDir);

                var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("Copy Directories", moduleToBuild.OwningNode.ModuleName);

                shellScriptBuildPhase.ShellScriptLines.Add("cp -Rf $SCRIPT_INPUT_FILE_0 $SCRIPT_OUTPUT_FILE_0");
                shellScriptBuildPhase.InputPaths.Add(sourcePath);
                shellScriptBuildPhase.OutputPaths.Add(dest);

                primaryPBXNativeTarget.BuildPhases.Add(shellScriptBuildPhase);
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
            var project = this.Workspace.GetProject(primaryNode);
            var primaryPBXNativeTarget = primaryNode.Data as PBXNativeTarget;

            if (options.OSXApplicationBundle)
            {
                primaryPBXNativeTarget.Type = PBXNativeTarget.EType.ApplicationBundle;
                primaryPBXNativeTarget.ProductReference.SetType(PBXFileReference.EType.ApplicationBundle);
            }

            this.PublishDependencies(moduleToBuild, primaryNode, project, primaryPBXNativeTarget);
            this.PublishAdditionalDirectories(moduleToBuild, primaryNode, project, primaryPBXNativeTarget);

            success = true;
            return null;
        }
#endif
    }
}
