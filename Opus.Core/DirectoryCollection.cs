// <copyright file="DirectoryCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class DirectoryCollection : System.ICloneable, System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<string> directoryPaths = new System.Collections.Generic.List<string>();

        public object Clone()
        {
            var clone = new DirectoryCollection();
            clone.directoryPaths.AddRange(this.directoryPaths);
            return clone;
        }

        public void Add(string absoluteDirectoryPath, bool checkForExistence)
        {
            if (checkForExistence && !System.IO.Directory.Exists(absoluteDirectoryPath))
            {
                throw new Exception("The directory '{0}' does not exist", absoluteDirectoryPath);
            }

            if (!this.directoryPaths.Contains(absoluteDirectoryPath))
            {
                this.directoryPaths.Add(absoluteDirectoryPath);
            }
            else
            {
                Log.DebugMessage("Absolute path '{0}' is already present in the list of directories", absoluteDirectoryPath);
            }
        }

        public void AddAbsoluteDirectory(string absoluteDirectoryPath, bool checkForExistence)
        {
            this.Add(absoluteDirectoryPath, checkForExistence);
        }

        private static StringArray GetDirectories(string baseDirectory, params string[] pathSegments)
        {
            if (0 == pathSegments.Length)
            {
                return new StringArray(baseDirectory);
            }

            if (State.RunningMono)
            {
                // workaround for this Mono bug http://www.mail-archive.com/mono-bugs@lists.ximian.com/msg71506.html
                // cannot use GetFiles with a pattern containing directories

                var combinedBaseDirectory = baseDirectory;
                int i = 0;
                for (; i < pathSegments.Length - 1; ++i)
                {
                    var baseDirTest = System.IO.Path.Combine(combinedBaseDirectory, pathSegments[i]);
                    if (System.IO.Directory.Exists(baseDirTest))
                    {
                        combinedBaseDirectory = baseDirTest;
                    }
                    else
                    {
                        break;
                    }
                }

                if (i != pathSegments.Length - 1)
                {
                    throw new Exception("Unable to locate path, starting with '{0}' and ending in '{1}'", combinedBaseDirectory, pathSegments[i]);
                }

                combinedBaseDirectory = System.IO.Path.GetFullPath(combinedBaseDirectory);

                try
                {
                    var directories = new StringArray(System.IO.Directory.GetDirectories(combinedBaseDirectory, pathSegments[pathSegments.Length - 1], System.IO.SearchOption.TopDirectoryOnly));
                    var nonHiddenDirectories = new StringArray();
                    foreach (var dir in directories)
                    {
                        var info = new System.IO.DirectoryInfo(dir);
                        if (0 == (info.Attributes & System.IO.FileAttributes.Hidden))
                        {
                            nonHiddenDirectories.Add(dir);
                        }
                    }

                    if (0 == nonHiddenDirectories.Count)
                    {
                        Log.Detail("Warning: No directories match the pattern {0}{1}{2}", combinedBaseDirectory, System.IO.Path.DirectorySeparatorChar, pathSegments[pathSegments.Length - 1]);
                    }

                    return nonHiddenDirectories;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Log.Detail("Warning: No directories match the pattern {0}{1}{2}", combinedBaseDirectory, System.IO.Path.DirectorySeparatorChar, pathSegments[pathSegments.Length - 1]);
                    return new StringArray();
                }
            }
            else
            {
                var baseDirChanges = System.String.Empty;
                var relativePath = File.CombinePaths(ref baseDirChanges, pathSegments);
                if (baseDirChanges != System.String.Empty)
                {
                    baseDirectory = System.IO.Path.Combine(baseDirectory, baseDirChanges);
                    baseDirectory = System.IO.Path.GetFullPath(baseDirectory);
                }
                try
                {
                    var directories = new StringArray(System.IO.Directory.GetDirectories(baseDirectory, relativePath, System.IO.SearchOption.TopDirectoryOnly));
                    var nonHiddenDirectories = new StringArray();
                    foreach (var dir in directories)
                    {
                        var info = new System.IO.DirectoryInfo(dir);
                        if (0 == (info.Attributes & System.IO.FileAttributes.Hidden))
                        {
                            nonHiddenDirectories.Add(dir);
                        }
                    }

                    if (0 == nonHiddenDirectories.Count)
                    {
                        Log.Detail("Warning: No directories match the pattern {0}{1}{2}", baseDirectory, System.IO.Path.DirectorySeparatorChar, relativePath);
                    }

                    return nonHiddenDirectories;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Log.Detail("Warning: No directories match the pattern {0}{1}{2}", baseDirectory, System.IO.Path.DirectorySeparatorChar, relativePath);
                    return new StringArray();
                }
            }
        }

#if true
        // Note that this no longer applies a module's proxy path
        public void Include(Location root, params string[] pathSegments)
        {
            // TODO: replace with Location
            var paths = GetDirectories(root.CachedPath, pathSegments);
            foreach (var path in paths)
            {
                if (!this.directoryPaths.Contains(path))
                {
                    this.directoryPaths.Add(path);
                }
            }
        }

        // Note that this no longer applies a module's proxy path
        public void Exclude(Location root, params string[] pathSegments)
        {
            // TODO: replace with Location
            var paths = GetDirectories(root.CachedPath, pathSegments);
            foreach (var path in paths)
            {
                if (!this.directoryPaths.Contains(path))
                {
                    this.directoryPaths.Remove(path);
                }
            }
        }
#else
        // deprecated
        public void Include(object module, params string[] pathSegments)
        {
            var package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", module.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (module as BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            var paths = GetDirectories(packagePath, pathSegments);
            foreach (var path in paths)
            {
                if (!this.directoryPaths.Contains(path))
                {
                    this.directoryPaths.Add(path);
                }
            }
        }

        // deprecated
        public void Exclude(object module, params string[] pathSegments)
        {
            var package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", module.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (module as BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            var paths = GetDirectories(packagePath, pathSegments);
            foreach (var path in paths)
            {
                if (!this.directoryPaths.Contains(path))
                {
                    this.directoryPaths.Remove(path);
                }
            }
        }
#endif

        public void AddRange(string[] absolutePaths)
        {
            foreach (var absolutePath in absolutePaths)
            {
                this.Add(absolutePath, false);
            }
        }

        public void AddRange(StringArray absolutePaths)
        {
            foreach (var absolutePath in absolutePaths)
            {
                this.Add(absolutePath, false);
            }
        }

        // TODO: no longer possible?
#if false
        public void AddRange(PackageInformation package, string[] relativePaths)
        {
            if (null == package)
            {
                throw new Exception("Use AddRange without a package");
            }

            foreach (var path in relativePaths)
            {
                this.Include(package, path);
            }
        }

        public void AddRange(PackageInformation package, Opus.Core.StringArray relativePaths)
        {
            if (null == package)
            {
                throw new Exception("Use AddRange without a package");
            }

            foreach (var path in relativePaths)
            {
                this.Include(package, path);
            }
        }

        public void AddRange(object owner, string[] relativePaths)
        {
            var package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            this.AddRange(package, relativePaths);
        }

        public void AddRange(object owner, StringArray relativePaths)
        {
            var package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            this.AddRange(package, relativePaths);
        }
#endif

        public string this[int index]
        {
            get
            {
                return this.directoryPaths[index];
            }
        }

        public int Count
        {
            get
            {
                return this.directoryPaths.Count;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return new DirectoryCollectionEnumerator(this);
        }

        public StringArray ToStringArray()
        {
            var array = new StringArray(this.directoryPaths);
            return array;
        }
    }
}
