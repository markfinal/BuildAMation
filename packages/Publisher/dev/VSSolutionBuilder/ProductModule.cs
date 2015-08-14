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
    public sealed class VSSolutionPackager :
        IPackagePolicy
    {
        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, PackageReference>> packageObjects)
        {
            // instead of copying to the package root, modules are copied next to their dependees
            foreach (var module in packageObjects)
            {
                foreach (var path in module.Value)
                {
                    var sourcePath = path.Key.ToString();
                    if (path.Value.IsMarker)
                    {
                        // no copy is needed, but as we're copying other files relative to this, record where they have to go
                        // therefore ignore any subdirectory on this module
                        path.Value.DestinationDir = System.IO.Path.GetDirectoryName(sourcePath);
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var commands = new Bam.Core.StringArray();
                            if (null != module.Key.MetaData)
                            {
                                var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, subdir));
                                commands.Add(System.String.Format("IF NOT EXIST {0} MKDIR {0}", destinationDir));
                                commands.Add(System.String.Format(@"copy /V /Y $(OutputPath)$(TargetFileName) {0}\$(TargetFileName)", destinationDir));
                                (module.Key.MetaData as VSSolutionBuilder.V2.VSCommonProject).AddPostBuildCommands(commands);
                                path.Value.DestinationDir = destinationDir;
                            }
                            else
                            {
                                commands.Add(System.String.Format(@"copy /V /Y {0} $(OutDir)\{1}\{2}", sourcePath, subdir, System.IO.Path.GetFileName(sourcePath)));
                                (reference.Module.MetaData as VSSolutionBuilder.V2.VSCommonProject).AddPostBuildCommands(commands);
                            }
                        }
                    }
                }
            }
        }
    }
}
}
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        private static void
        CopyNodes(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDependency nodeInfo)
        {
            var toCopy = meta.Node;
            var keyToCopy = nodeInfo.Key;
            var toProject = primaryModule.OwningNode.Data as IProject;

            var configCollection = toProject.Configurations;
            var configurationName = configCollection.GetConfigurationNameForTarget((Bam.Core.BaseTarget)toCopy.Target); // TODO: not accurate
            var configuration = configCollection[configurationName];

            var toolName = "VCPostBuildEventTool";
            var vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            var sourceLoc = toCopy.Module.Locations[keyToCopy];
            if (!sourceLoc.IsValid)
            {
                return;
            }
            var sourcePath = sourceLoc.GetSingleRawPath();

            var destinationDir = configuration.OutputDirectory;
            var destinationDirPath = destinationDir.GetSingleRawPath();

            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;
            var nodeSpecificSubdirectory = nodeInfo.SubDirectory;
            if (!string.IsNullOrEmpty(nodeSpecificSubdirectory))
            {
                subDirectory = nodeSpecificSubdirectory;
            }

            var newKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(toCopy.Module, toCopy.Module, keyToCopy);
            var primaryKey = new Bam.Core.LocationKey(newKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                sourcePath,
                destinationDirPath,
                subDirectory,
                string.Empty,
                moduleToBuild,
                primaryKey);

            if (sourcePath == destPath)
            {
                Bam.Core.Log.DebugMessage("Skipping copying '{0}' as it would go to the same location", sourcePath);
                return;
            }

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("cmd.exe /c COPY /Y \"{0}\" \"{1}\"{2}", sourcePath, destPath, System.Environment.NewLine);

            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == toProject.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == toProject.VSTarget)
                {
                    attributeName = "Command";
                }

                lock (vcPostBuildEventTool)
                {
                    if (vcPostBuildEventTool.HasAttribute(attributeName))
                    {
                        var currentValue = vcPostBuildEventTool[attributeName];
                        currentValue += commandLine.ToString();
                        vcPostBuildEventTool[attributeName] = currentValue;
                    }
                    else
                    {
                        vcPostBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                    }
                }
            }
        }

        private static void
        CopyDirectory(
            Publisher.ProductModule moduleToBuild,
            Bam.Core.BaseModule primaryModule,
            Publisher.ProductModuleUtilities.MetaData meta,
            Publisher.PublishDirectory directoryInfo)
        {
            var toProject = primaryModule.OwningNode.Data as IProject;

            var configCollection = toProject.Configurations;
            var configurationName = configCollection.GetConfigurationNameForTarget((Bam.Core.BaseTarget)moduleToBuild.OwningNode.Target);
            var configuration = configCollection[configurationName];

            var toolName = "VCPostBuildEventTool";
            var vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            var sourceLoc = directoryInfo.DirectoryLocation;
            if (!sourceLoc.IsValid)
            {
                return;
            }
            var sourcePath = sourceLoc.GetSingleRawPath();
            var lastDir = System.IO.Path.GetFileName(sourcePath);

            var destinationDir = configuration.OutputDirectory;
            var destinationDirPath = System.IO.Path.Combine(destinationDir.GetSingleRawPath(), lastDir);

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("IF NOT EXIST {0} MKDIR {0}{1}", destinationDirPath, System.Environment.NewLine);
            commandLine.AppendFormat("cmd.exe /c XCOPY /E /Y \"{0}\" \"{1}\"{2}", sourcePath, destinationDirPath, System.Environment.NewLine);

            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == toProject.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == toProject.VSTarget)
                {
                    attributeName = "Command";
                }

                lock (vcPostBuildEventTool)
                {
                    if (vcPostBuildEventTool.HasAttribute(attributeName))
                    {
                        var currentValue = vcPostBuildEventTool[attributeName];
                        currentValue += commandLine.ToString();
                        vcPostBuildEventTool[attributeName] = currentValue;
                    }
                    else
                    {
                        vcPostBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                    }
                }
            }
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

            // TODO should really be using this
            /*
            // take the common subdirectory by default, otherwise override on a per Location basis
            var attribute = meta.Attribute as Publisher.CopyFileLocationsAttribute;
            var subDirectory = attribute.CommonSubDirectory;
            var nodeSpecificSubdirectory = nodeInfo.SubDirectory;
            if (!string.IsNullOrEmpty(nodeSpecificSubdirectory))
            {
                subDirectory = nodeSpecificSubdirectory;
            }
            */

            // TODO: should really be using this, in case we need to refer to published locations after this module
            /*
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                sourceKey);
                */

            if (sourceKey.IsFileKey)
            {
                //var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
                CopyNodes(
                    moduleToBuild,
                    primaryModule,
                    meta,
                    nodeInfo);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                //var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
                CopyNodes(
                    moduleToBuild,
                    primaryModule,
                    meta,
                    nodeInfo);
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
            // TODO: should be using this
            /*
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
            */
            CopyDirectory(
                moduleToBuild,
                primaryModule,
                meta,
                directoryInfo);
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
            throw new Bam.Core.Exception("Info.plists are OSX specific. Not supported with VisualStudio projects");
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
                null,
                true);

            success = true;
            return null;
        }
    }
}
