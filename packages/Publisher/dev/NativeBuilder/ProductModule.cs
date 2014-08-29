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
