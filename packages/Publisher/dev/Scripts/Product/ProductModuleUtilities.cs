// <copyright file="ProductModuleUtilities.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public static class ProductModuleUtilities
    {
        // TODO: out of all the dependents, how do we determine the metadata that they have associated with them
        // from the Publisher.ProductModule module, that is beyond just the need for graph building?
        private static Opus.Core.Array<PublishNodeData>
        GetModulesDataWithAttribute(
            Publisher.ProductModule moduleToBuild,
            System.Type attributeType)
        {
            var moduleData = new Opus.Core.Array<PublishNodeData>();

            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(flags);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                if (attributes.Length != 1)
                {
                    throw new Opus.Core.Exception("Found {0} attributes on field {1} of module {2}. Should be just one",
                                                  attributes.Length, field.Name, moduleToBuild.OwningNode.ModuleName);
                }

                var currentAttr = attributes[0];
                if (currentAttr.GetType() == attributeType)
                {
                    var primaryTargetData = field.GetValue(moduleToBuild) as PublishNodeData;
                    if (null == primaryTargetData)
                    {
                        throw new Opus.Core.Exception("PrimaryTarget attribute field was not of type PublishNodeData");
                    }
                    moduleData.AddUnique(primaryTargetData);
                }
            }

            return moduleData;
        }

        public class PrimaryNodeData
        {
            public Opus.Core.DependencyNode Node
            {
                get;
                set;
            }

            public Opus.Core.LocationKey Key
            {
                get;
                set;
            }
        }

        public static PrimaryNodeData GetPrimaryNodeData(Publisher.ProductModule moduleToBuild)
        {
            PrimaryNodeData primaryNodeData = null;

            // TODO: why is this check necessary?
            var dependents = moduleToBuild.OwningNode.ExternalDependents;
            if ((null == dependents) || (dependents.Count == 0))
            {
                return primaryNodeData;
            }

            var primaryTargetData = GetModulesDataWithAttribute(moduleToBuild, typeof(Publisher.PrimaryTargetAttribute));
            if (primaryTargetData.Count == 0)
            {
                return primaryNodeData;
            }

            primaryNodeData = new PrimaryNodeData();
            primaryNodeData.Node = Opus.Core.ModuleUtilities.GetNode(primaryTargetData[0].ModuleType, (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);
            primaryNodeData.Key = primaryTargetData[0].Key;
            return primaryNodeData;
        }

        public static string
        GetPublishedKeyName(
            Opus.Core.BaseModule primaryModule,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(module.OwningNode.ModuleName);
            builder.Append(".");
            builder.Append(key.ToString());
            builder.Append(".PublishedFile");
            if (primaryModule != module)
            {
                builder.Append(".For.");
                builder.Append(primaryModule.OwningNode.ModuleName);
            }
            return builder.ToString();
        }

        public static string
        GenerateDestinationPath(
            string sourcePath,
            string destinationDirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var filename = System.IO.Path.GetFileName(sourcePath);
            var destPath = System.IO.Path.Combine(destinationDirectory, filename);
            if (key.IsFileKey)
            {
                module.Locations[key] = Opus.Core.FileLocation.Get(destPath, Opus.Core.Location.EExists.WillExist);
            }
            else if (key.IsSymlinkKey)
            {
                module.Locations[key] = Opus.Core.SymlinkLocation.Get(destPath, Opus.Core.Location.EExists.WillExist);
            }
            else if (key.IsDirectoryKey)
            {
                module.Locations[key] = Opus.Core.DirectoryLocation.Get(destPath, Opus.Core.Location.EExists.WillExist);
            }
            return destPath;
        }

        public static void
        CopyFileToLocation(
            Opus.Core.Location sourceFile,
            string destinationDirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceFile.GetSingleRawPath();
            var destPath = GenerateDestinationPath(sourcePath, destinationDirectory, module, key);
            Opus.Core.Log.Info("Copying file {0} to {1}", sourcePath, destPath);
            System.IO.File.Copy(sourcePath, destPath, true);
        }

        public static void
        CopySymlinkToLocation(
            Opus.Core.Location sourceSymlink,
            string destinationDirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceSymlink.GetSingleRawPath();
            var destPath = GenerateDestinationPath(sourcePath, destinationDirectory, module, key);
            Opus.Core.Log.Info("Copying symlink {0} to {1}", sourcePath, destPath);
#if __MonoCS__
            var buf = new Mono.Unix.Native.Stat();
            var statResult = Mono.Unix.Native.Syscall.lstat(sourcePath, out buf);
            if (0 != statResult)
            {
                throw new Opus.Core.Exception("Exception while stat'ing '{0}'", sourcePath);
            }
            var symLink = (buf.st_mode & Mono.Unix.Native.FilePermissions.S_IFLNK) == Mono.Unix.Native.FilePermissions.S_IFLNK;
            if (symLink)
            {
                var targetLink = new System.Text.StringBuilder(1024);
                Mono.Unix.Native.Syscall.readlink(sourcePath, targetLink, 1024);
                Mono.Unix.Native.Syscall.symlink(targetLink.ToString(), destPath);
            }
            else
            {
                System.IO.File.Copy(sourcePath, destPath, true);
            }
#else
            System.IO.File.Copy(sourcePath, destPath, true);
#endif
        }
    }
}
