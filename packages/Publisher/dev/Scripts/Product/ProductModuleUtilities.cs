// <copyright file="ProductModuleUtilities.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public static class ProductModuleUtilities
    {
        public class MetaData
        {
            public Opus.Core.DependencyNode Node
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public object Data
            {
                get;
                set;
            }

            public Publisher.IPublishBaseAttribute Attribute
            {
                get;
                set;
            }
        }

        public class MetaDataCollection :
            System.Collections.IEnumerable
        {
            public
            MetaDataCollection()
            {
                this.List = new Opus.Core.Array<MetaData>();
            }

            public void
            Add(
                MetaData input)
            {
                this.List.AddUnique(input);
            }

            public MetaDataCollection
            FilterByType<T>() where T : class
            {
                var filtered = new MetaDataCollection();
                foreach (var item in this.List)
                {
                    if (item.Attribute is T)
                    {
                        filtered.Add(item);
                    }
                }
                return filtered;
            }

            public int Count
            {
                get
                {
                    return this.List.Count;
                }
            }

            private Opus.Core.Array<MetaData> List
            {
                get;
                set;
            }

#region IEnumerable Members

            System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator()
            {
                return this.List.GetEnumerator();
            }

#endregion
        }

        public static MetaDataCollection
        GetPublishingMetaData(
            Opus.Core.Target targetToMatch,
            Opus.Core.DependencyNodeCollection nodeCollection)
        {
            var results = new MetaDataCollection();
            foreach (var node in nodeCollection)
            {
                var module = node.Module;
                var moduleType = module.GetType();
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic;
                var fields = moduleType.GetFields(flags);
                foreach (var field in fields)
                {
                    var candidates = field.GetCustomAttributes(typeof(Publisher.IPublishBaseAttribute), true);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }

                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish attribute found on field '{0}'", field.Name);
                    }

                    var attribute = candidates[0] as Publisher.IPublishBaseAttribute;
                    var matchesTarget = Opus.Core.TargetUtilities.MatchFilters(targetToMatch, attribute as Opus.Core.ITargetFilters);
                    if (!matchesTarget)
                    {
                        continue;
                    }

                    var data = new MetaData();
                    data.Node = node;
                    data.Data = field.GetValue(module);
                    data.Attribute = attribute;
                    data.Name = field.Name;
                    results.Add(data);
                }
            }
            return results;
        }

        // TODO: out of all the dependents, how do we determine the metadata that they have associated with them
        // from the Publisher.ProductModule module, that is beyond just the need for graph building?
        private static Opus.Core.Array<T>
        GetModulesDataWithAttribute<T>(
            Publisher.ProductModule moduleToBuild,
            System.Type attributeType) where T : class
        {
            var moduleData = new Opus.Core.Array<T>();

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
                    var primaryTargetData = field.GetValue(moduleToBuild) as T;
                    if (null == primaryTargetData)
                    {
                        throw new Opus.Core.Exception("PrimaryTarget attribute field was not of type {0}", typeof(T).ToString());
                    }
                    moduleData.AddUnique(primaryTargetData);
                }
            }

            return moduleData;
        }

        public static Opus.Core.DependencyNode
        GetPrimaryTarget(
            Publisher.ProductModule moduleToBuild)
        {
            // TODO: why is this check necessary?
            var dependents = moduleToBuild.OwningNode.ExternalDependents;
            if ((null == dependents) || (dependents.Count == 0))
            {
                return null;
            }

            var matchingModules = GetModulesDataWithAttribute<System.Type>(moduleToBuild, typeof(Publisher.PrimaryTargetAttribute));
            if (matchingModules.Count == 0)
            {
                return null;
            }

            var node = Opus.Core.ModuleUtilities.GetNode(matchingModules[0], (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);
            return node;
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
        GetPublishedAdditionalDirectoryKeyName(
            Opus.Core.BaseModule primaryModule,
            string directoryName)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("Directory.");
            builder.Append(directoryName);
            builder.Append(".PublishedFor.");
            builder.Append(primaryModule.OwningNode.ModuleName);
            return builder.ToString();
        }

        public static string
        GenerateDestinationPath(
            string sourcePath,
            string destinationDirectory,
            string subdirectory,
            string renamedLeaf,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var filename = string.IsNullOrEmpty(renamedLeaf) ? System.IO.Path.GetFileName(sourcePath) : renamedLeaf;
            string destPath;
            if (string.IsNullOrEmpty(subdirectory))
            {
                destPath = System.IO.Path.Combine(destinationDirectory, filename);
            }
            else
            {
                destPath = System.IO.Path.Combine(destinationDirectory, subdirectory);
                if (subdirectory != ".")
                {
                    NativeBuilder.NativeBuilder.MakeDirectory(destPath);
                }
                destPath = System.IO.Path.Combine(destPath, filename);
            }
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
            string subdirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceFile.GetSingleRawPath();
            var destPath = GenerateDestinationPath(
                sourcePath,
                destinationDirectory,
                subdirectory,
                string.Empty,
                module,
                key);
            Opus.Core.Log.Info("Copying file {0} to {1}", sourcePath, destPath);
            System.IO.File.Copy(sourcePath, destPath, true);
        }

        public static void
        CopyDirectoryToLocation(
            Opus.Core.Location sourceDirectory,
            string destinationDirectory,
            string subdirectory,
            string renamedLeaf,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceDirectory.GetSingleRawPath();
            var destPath = GenerateDestinationPath(sourcePath, destinationDirectory, subdirectory, renamedLeaf, module, key);
            Opus.Core.Log.Info("Copying directory {0} to {1}", sourcePath, destPath);

            if (!System.IO.Directory.Exists(destPath))
            {
                System.IO.Directory.CreateDirectory(destPath);
            }

            foreach (string dir in System.IO.Directory.GetDirectories(sourcePath, "*", System.IO.SearchOption.AllDirectories))
            {
                var dirToCreate = destPath + dir.Substring(sourcePath.Length);
                if (!System.IO.Directory.Exists(dirToCreate))
                {
                    System.IO.Directory.CreateDirectory(dirToCreate);
                }
            }

            foreach (string file_name in System.IO.Directory.GetFiles(sourcePath, "*.*", System.IO.SearchOption.AllDirectories))
            {
                var fileCopiedTo = destPath + file_name.Substring(sourcePath.Length);
                System.IO.File.Copy(file_name, fileCopiedTo, true);
            }
        }

        public static void
        CopySymlinkToLocation(
            Opus.Core.Location sourceSymlink,
            string destinationDirectory,
            string subdirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceSymlink.GetSingleRawPath();
            var destPath = GenerateDestinationPath(
                sourcePath,
                destinationDirectory,
                subdirectory,
                string.Empty,
                module,
                key);
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
