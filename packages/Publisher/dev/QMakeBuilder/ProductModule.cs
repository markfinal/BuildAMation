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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        private void
        CopyFilesToDirectory(
            Bam.Core.BaseModule module,
            string destinationDirectory,
            string subdirectory,
            QMakeData proData)
        {
            // TODO: this is only temporary while I figure out prebuilt libraries
            if (null == proData)
            {
                throw new Bam.Core.Exception("No QMake pro file to append rules to");
            }

#if true
            // TOOD: if there is only one place to write to, use this
            var targetName = (module is C.DynamicLibrary) ? "dlltarget" : "target";
            var customRules = new Bam.Core.StringArray();
            var destDir = destinationDirectory.Clone() as string;
            if (!System.String.IsNullOrEmpty(subdirectory))
            {
                destDir = System.IO.Path.Combine(destDir, subdirectory);
            }
            destDir = destDir.Replace('\\', '/');
            customRules.Add(System.String.Format("{0}.path={1}", targetName, destDir));
            customRules.Add(System.String.Format("INSTALLS+={0}", targetName));
#else
            // otherwise, if there are multiple places to install to, use this
            // Note: don't use absolute paths, unless they exist already - cannot refer to files that are to be built
            var targetName = System.String.Format("copy_{0}_for_{1}", module.OwningNode.ModuleName, primaryNode.ModuleName);
            var customRules = new Bam.Core.StringArray();
            customRules.Add(System.String.Format("{0}.path={1}", targetName, destinationDirectory.Replace('\\', '/')));
            // TODO: this does not resolve to the final destination path
            customRules.Add(System.String.Format("{0}.files=$${{DESTDIR}}/$${{TARGET}}.$${{TARGET_EXT})", targetName));
            customRules.Add(System.String.Format("INSTALLS+={0}", targetName));
#endif
            if (null == proData.CustomRules)
            {
                proData.CustomRules = customRules;
            }
            else
            {
                proData.CustomRules.AddRangeUnique(customRules);
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
                Bam.Core.Log.DebugMessage("Location key '{0}' not in location map", sourceKey.ToString());
                return;
            }

            var sourceLoc = moduleLocations[sourceKey];
            if (!sourceLoc.IsValid)
            {
                Bam.Core.Log.DebugMessage("Location '{0} is invalid", sourceKey.ToString());
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

            var sourcePath = sourceLoc.GetSingleRawPath();
            if (sourceKey.IsFileKey)
            {
                var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
                var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                    sourcePath,
                    publishDirectoryPath,
                    string.Empty,
                    string.Empty,
                    moduleToBuild,
                    publishedKey);
                if (destPath == sourcePath)
                {
                    Bam.Core.Log.DebugMessage("Ignoring files to be published on top of themselves");
                    return;
                }
                var destDir = System.IO.Path.GetDirectoryName(destPath);

                // TODO: where do prebuilt copies go?
                var proData = meta.Node.Data as QMakeData;
                if (null == proData)
                {
                    Bam.Core.Log.MessageAll("Publishing prebuilt libraries is unsupported in QMake currently");
                    return;
                }

                this.CopyFilesToDirectory(
                    meta.Node.Module,
                    destDir,
                    subDirectory,
                    meta.Node.Data as QMakeData
                    );
            }
            else if (sourceKey.IsSymlinkKey)
            {
                var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
                var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(
                    sourcePath,
                    publishDirectoryPath,
                    string.Empty,
                    string.Empty,
                    moduleToBuild,
                    publishedKey);
                if (destPath == sourcePath)
                {
                    Bam.Core.Log.DebugMessage("Ignoring symlinks to be published on top of themselves");
                    return;
                }
                var destDir = System.IO.Path.GetDirectoryName(destPath);

                // TODO: where do prebuilt copies go?
                var proData = meta.Node.Data as QMakeData;
                if (null == proData)
                {
                    Bam.Core.Log.MessageAll("Publishing prebuilt library symlinks is unsupported in QMake currently");
                    return;
                }

                // TODO: not validated
                this.CopyFilesToDirectory(
                    meta.Node.Module,
                    destDir,
                    subDirectory,
                    meta.Node.Data as QMakeData
                    );
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
            Bam.Core.Log.MessageAll("Publishing directories is unsupported in QMake currently");
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
            var plistNode = meta.Node;

            var moduleToCopy = plistNode.Module;
            var keyToCopy = nodeInfo.Key;

            /*
            var publishedKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(
                primaryModule,
                moduleToCopy,
                keyToCopy);
            var publishedKey = new Bam.Core.LocationKey(publishedKeyName, Bam.Core.ScaffoldLocation.ETypeHint.File);
            */
            var contentsLoc = moduleToBuild.Locations[Publisher.ProductModule.OSXAppBundleContents].GetSingleRawPath();
            var plistSourceLoc = moduleToCopy.Locations[keyToCopy];

            // TODO: this probably won't work as it's copying an arbitrary file, but worth trying to exercise
            this.CopyFilesToDirectory(
                plistNode.Module,
                contentsLoc,
                null,
                meta.Node.Data as QMakeData
                );
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
