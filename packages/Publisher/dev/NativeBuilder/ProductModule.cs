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
    public sealed class NativePackager :
        IPackagePolicy
    {
        static private void
        CopyFile(
            string sourcePath,
            string destinationDir)
        {
            if (!System.IO.Directory.Exists(destinationDir))
            {
                System.IO.Directory.CreateDirectory(destinationDir);
            }
            var destinationPath = System.IO.Path.Combine(destinationDir, System.IO.Path.GetFileName(sourcePath));
            System.IO.File.Copy(sourcePath, destinationPath, true);
        }

        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, PackageReference>> packageObjects)
        {
            var root = packageRoot.Parse();
            foreach (var module in packageObjects)
            {
                foreach (var path in module.Value)
                {
                    var sourcePath = path.Key.ToString();
                    if (path.Value.IsMarker)
                    {
                        var destinationDir = root;
                        if (null != path.Value.SubDirectory)
                        {
                            destinationDir = System.IO.Path.Combine(destinationDir, path.Value.SubDirectory);
                        }
                        CopyFile(sourcePath, destinationDir);
                        path.Value.DestinationDir = destinationDir;
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, path.Value.SubDirectory));
                            CopyFile(sourcePath, destinationDir);
                            path.Value.DestinationDir = destinationDir;
                        }
                    }
                }
            }
        }
    }
}
}
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
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
            foreach (var dir in directoriesToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

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
                var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
                Publisher.ProductModuleUtilities.CopyFileToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
                Publisher.ProductModuleUtilities.CopySymlinkToLocation(
                    sourceLoc,
                    publishDirectoryPath,
                    subDirectory,
                    moduleToBuild,
                    publishedKey);
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
            foreach (var dir in directoriesToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedAdditionalDirectoryKeyName(
                primaryModule,
                directoryInfo.Directory);
            var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Directory);
            var sourceLoc = directoryInfo.DirectoryLocation;
            var attribute = meta.Attribute as Publisher.AdditionalDirectoriesAttribute;
            var subdirectory = attribute.CommonSubDirectory;
            Publisher.ProductModuleUtilities.CopyDirectoryToLocation(
                sourceLoc,
                publishDirectoryPath,
                subdirectory,
                directoryInfo.RenamedLeaf,
                moduleToBuild,
                publishedKey);
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
            foreach (var dir in directoriesToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var plistNode = meta.Node;

            var moduleToCopy = plistNode.Module;
            var keyToCopy = nodeInfo.Key;

            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                keyToCopy);
            var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
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
                null,
                false);

            success = true;
            return null;
        }
    }
}
