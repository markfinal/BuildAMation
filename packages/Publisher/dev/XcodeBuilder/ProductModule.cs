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
    public sealed class XcodePackager :
        IPackagePolicy
    {
        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, string>> packageObjects)
        {
            // instead of copying to the package root, modules are copied next to their dependees
            foreach (var module in packageObjects)
            {
                foreach (var dependee in module.Key.Dependees)
                {
                    if (!packageObjects.ContainsKey(dependee))
                    {
                        // the dependee wasn't being packaged, so the dependent isn't needed to be either
                        Bam.Core.Log.DebugMessage("Module {0} is packaged, but {1} is dependent upon it but isn't packaged, and thus ignored", module.ToString(), dependee.ToString());
                        continue;
                    }
                    if (dependee.Package == module.Key.Package)
                    {
                        // same package has the same output folder, so don't bother copying
                        continue;
                    }
                    foreach (var path in packageObjects[dependee].Keys)
                    {
                        // this has to be the path that Xcode writes to
                        var dir = Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(config)", dependee).Parse();
                        // the subdir on the dependee is ignored here, as it was never copied anywhere
                        foreach (var modulePath in module.Value)
                        {
                            // the dependent's subdir must be honoured, as the runtime might expect it
                            var dependentSubDir = modulePath.Value;

                            var commands = new Bam.Core.StringArray();
                            if (null != module.Key.MetaData)
                            {
                                var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, dependentSubDir));
                                commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}", destinationDir));
                                commands.Add(System.String.Format("cp -v $CONFIGURATION_BUILD_DIR/$EXECUTABLE_NAME {0}/$EXECUTABLE_NAME", destinationDir));
                                (module.Key.MetaData as XcodeBuilder.V2.XcodeCommonProject).AddPostBuildCommands(commands);
                            }
                            else
                            {
                                var sourcePath = modulePath.Key.Parse();
                                commands.Add(System.String.Format("cp -v {0} $CONFIGURATION_BUILD_DIR/{1}/{2}", sourcePath, dependentSubDir, System.IO.Path.GetFileName(sourcePath)));
                                (dependee.MetaData as VSSolutionBuilder.V2.VSCommonProject).AddPostBuildCommands(commands);
                            }
                        }
                    }
                }
            }
        }
    }
}
}
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
