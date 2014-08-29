#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        private static void
        CopyNodes(
            Publisher.ProductModule moduleToBuild,
            PBXProject project,
            PBXNativeTarget nativeTarget,
            Bam.Core.DependencyNode toCopy,
            string subdirectory)
        {
            var nameOfBuildPhase = System.String.Format("Copy files for {0} to {1}", nativeTarget.Name, subdirectory);
            var copyFilesBuildPhase = project.CopyFilesBuildPhases.Get(nameOfBuildPhase, moduleToBuild.OwningNode.ModuleName);
            copyFilesBuildPhase.SubFolder = PBXCopyFilesBuildPhase.ESubFolder.Executables;
            if (!string.IsNullOrEmpty(subdirectory))
            {
                copyFilesBuildPhase.DestinationPath = subdirectory;
            }
            else
            {
                // check whether this is likely to be copying onto itself
                // native targets must match
                // and must be an empty subdirectory
                if (toCopy.Data == nativeTarget)
                {
                    Bam.Core.Log.DebugMessage("'{0}' would publish onto itself", toCopy.UniqueModuleName);
                    return;
                }
            }
            nativeTarget.BuildPhases.AddUnique(copyFilesBuildPhase);

            string pathOfFileToCopy;
            PBXFileReference.EType copiedFileType;
            if (toCopy.Data is PBXNativeTarget)
            {
                var copySourceNativeTarget = toCopy.Data as PBXNativeTarget;

                // need a different copy of the build file, to live in the CopyFiles build phase
                // but still referencing the same PBXFileReference
                copiedFileType = copySourceNativeTarget.ProductReference.Type;
                if (copiedFileType == PBXFileReference.EType.DynamicLibrary)
                {
                    copiedFileType = PBXFileReference.EType.ReferencedDynamicLibrary;
                }
                else if (copiedFileType == PBXFileReference.EType.Executable)
                {
                    copiedFileType = PBXFileReference.EType.ReferencedExecutable;
                }

                pathOfFileToCopy = copySourceNativeTarget.ProductReference.FullPath;
            }
            else if (toCopy.Data is PBXBuildFile)
            {
                var copyBuildFile = toCopy.Data as PBXBuildFile;
                copiedFileType = copyBuildFile.FileReference.Type;
                pathOfFileToCopy = copyBuildFile.FileReference.FullPath;
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported file to copy from '{0}'", toCopy.UniqueModuleName);
            }
            var relativePath = Bam.Core.RelativePathUtilities.GetPath(pathOfFileToCopy, project.RootUri);
            var dependentFileRef = project.FileReferences.Get(toCopy.UniqueModuleName, copiedFileType, relativePath, project.RootUri);
            var buildFile = project.BuildFiles.Get(toCopy.UniqueModuleName, dependentFileRef, copyFilesBuildPhase);
            if (null == buildFile)
            {
                throw new Bam.Core.Exception("Build file not available");
            }

            project.MainGroup.Children.AddUnique(dependentFileRef);
        }

        private void
        CopyAdditionalDirectory(
            Publisher.ProductModule moduleToBuild,
            Publisher.PublishDirectory additionalDirsData,
            Bam.Core.BaseModule primaryModule,
            string subdirectory,
            PBXProject project,
            PBXNativeTarget primaryPBXNativeTarget)
        {
            var sourceLoc = additionalDirsData.DirectoryLocation;
            var sourcePath = sourceLoc.GetSingleRawPath();
            var lastDir = additionalDirsData.RenamedLeaf != null ? additionalDirsData.RenamedLeaf : System.IO.Path.GetFileName(sourcePath);
            var destDir = primaryModule.Locations[C.Application.OutputDir].GetSingleRawPath();
            var dest = destDir.Clone() as string;
            if ((moduleToBuild.Options as Publisher.IPublishOptions).OSXApplicationBundle)
            {
                dest = System.IO.Path.Combine(dest, "$EXECUTABLE_FOLDER_PATH");
            }
            if (!System.String.IsNullOrEmpty(subdirectory))
            {
                dest = System.IO.Path.Combine(dest, subdirectory);
            }
            dest = System.IO.Path.Combine(dest, lastDir);

            var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("Copy Directories", moduleToBuild.OwningNode.ModuleName);

            var targetNode = moduleToBuild.OwningNode;
            var baseTarget = (Bam.Core.BaseTarget)targetNode.Target;
            var configuration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);

            shellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("if [ \\\"${{CONFIGURATION}}\\\" = \\\"{0}\\\" ]; then", configuration.Name));
            shellScriptBuildPhase.ShellScriptLines.Add("cp -Rf $SCRIPT_INPUT_FILE_0 $SCRIPT_OUTPUT_FILE_0");
            shellScriptBuildPhase.ShellScriptLines.Add("fi");
            shellScriptBuildPhase.InputPaths.Add(sourcePath);
            shellScriptBuildPhase.OutputPaths.Add(dest);

            primaryPBXNativeTarget.BuildPhases.Add(shellScriptBuildPhase);
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
            var project = this.Workspace.GetProject(primaryModule.OwningNode);
            var primaryTarget = primaryModule.OwningNode.Data as PBXNativeTarget;

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

            if (sourceKey.IsFileKey)
            {
                CopyNodes(
                    moduleToBuild,
                    project,
                    primaryTarget,
                    moduleToCopy.OwningNode,
                    subDirectory);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                throw new Bam.Core.Exception("Copying symlinks is not supported");
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
            var project = this.Workspace.GetProject(primaryModule.OwningNode);
            var primaryTarget = primaryModule.OwningNode.Data as PBXNativeTarget;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
            this.CopyAdditionalDirectory(
                moduleToBuild,
                directoryInfo,
                primaryModule,
                subdirectory,
                project,
                primaryTarget);
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
            // no special action here
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var primaryNode = Publisher.DelegateProcessing.Process(
                moduleToBuild,
                nativeCopyNodeLocation,
                nativeCopyAdditionalDirectory,
                nativeCopyInfoPList,
                null,
                true);

            var options = moduleToBuild.Options as Publisher.IPublishOptions;
            var primaryPBXNativeTarget = primaryNode.Data as PBXNativeTarget;
            if (options.OSXApplicationBundle)
            {
                primaryPBXNativeTarget.ChangeType(PBXNativeTarget.EType.ApplicationBundle);
                primaryPBXNativeTarget.ProductReference.ChangeType(PBXFileReference.EType.ApplicationBundle);
            }

            success = true;
            return null;
        }
    }
}
