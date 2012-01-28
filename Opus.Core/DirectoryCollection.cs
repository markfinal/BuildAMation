// <copyright file="DirectoryCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class DirectoryCollection : System.ICloneable, System.Collections.IEnumerable
    {
#if NEWDIRECTORYCOLLECTION
        private System.Collections.Generic.List<string> directoryPaths = new System.Collections.Generic.List<string>();
#else
        private System.Collections.Generic.List<PackageAndDirectoryPath> directoryList = new System.Collections.Generic.List<PackageAndDirectoryPath>();
#endif

        public object Clone()
        {
            DirectoryCollection clone = new DirectoryCollection();
#if NEWDIRECTORYCOLLECTION
            clone.directoryPaths.AddRange(this.directoryPaths);
#else
            foreach (PackageAndDirectoryPath pap in this.directoryList)
            {
                clone.Add(pap.Package, pap.RelativePath.Clone() as string, pap.ProxyPath);
            }
#endif
            return clone;
        }

#if NEWDIRECTORYCOLLECTION
#else
        private bool Contains(PackageAndDirectoryPath pap)
        {
            foreach (PackageAndDirectoryPath listPap in this.directoryList)
            {
                if (listPap.Package == pap.Package &&
                    listPap.RelativePath == pap.RelativePath)
                {
                    return true;
                }
            }

            return false;
        }
#endif

        public void Add(string absoluteDirectoryPath, bool checkForExistence)
        {
            if (checkForExistence && !System.IO.Directory.Exists(absoluteDirectoryPath))
            {
                throw new Exception(System.String.Format("The directory '{0}' does not exist", absoluteDirectoryPath), false);
            }

#if NEWDIRECTORYCOLLECTION
            if (!this.directoryPaths.Contains(absoluteDirectoryPath))
            {
                this.directoryPaths.Add(absoluteDirectoryPath);
            }
            else
            {
                Log.DebugMessage("Absolute path '{0}' is already present in the list of directories", absoluteDirectoryPath);
            }
#else
            PackageAndDirectoryPath pap = new PackageAndDirectoryPath(null, absoluteDirectoryPath, null);
            if (this.Contains(pap))
            {
                Log.DebugMessage("Absolute path '{0}' is already present in the list of directories", absoluteDirectoryPath);
            }
            else
            {
                this.directoryList.Add(pap);
            }
#endif
        }

        public void AddAbsoluteDirectory(string absoluteDirectoryPath, bool checkForExistence)
        {
            this.Add(absoluteDirectoryPath, checkForExistence);
        }

#if NEWDIRECTORYCOLLECTION
#else
        public void Add(PackageInformation package, string relativePath, ProxyModulePath proxyPath)
        {
            PackageAndDirectoryPath pap = new PackageAndDirectoryPath(package, relativePath, proxyPath);
            if (this.Contains(pap))
            {
                Log.DebugMessage("Relative path '{0}' is already present for package '{1}'", relativePath, package.FullName);
            }
            else
            {
                this.directoryList.Add(pap);
            }
        }
#endif

        public void Add(object owner, string relativePath)
        {
            if (null == owner)
            {
#if NEWDIRECTORYCOLLECTION
                this.AddAbsoluteDirectory(relativePath, false);
#else
                this.Add(null, relativePath, null);
#endif
                return;
            }

#if NEWDIRECTORYCOLLECTION
            string[] pathSegments = relativePath.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
            this.Include(owner, pathSegments);
#else
            if (owner is PackageInformation)
            {
                this.Add(owner as PackageInformation, relativePath, null);
            }
            else
            {
                PackageInformation package = PackageUtilities.GetOwningPackage(owner);
                if (null == package)
                {
                    throw new Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
                }

                this.Add(package, relativePath, (owner as IModule).ProxyPath);
            }
#endif
        }

        private static StringArray GetDirectories(string baseDirectory, params string[] pathSegments)
        {
            if (State.RunningMono)
            {
                // workaround for this Mono bug http://www.mail-archive.com/mono-bugs@lists.ximian.com/msg71506.html
                // cannot use GetFiles with a pattern containing directories

                string combinedBaseDirectory = baseDirectory;
                int i = 0;
                for (; i < pathSegments.Length - 1; ++i)
                {
                    string baseDirTest = System.IO.Path.Combine(combinedBaseDirectory, pathSegments[i]);
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
                    throw new Exception(System.String.Format("Unable to locate path, starting with '{0}' and ending in '{1}'", combinedBaseDirectory, pathSegments[i]));
                }

                try
                {
                    StringArray directories = new StringArray(System.IO.Directory.GetDirectories(combinedBaseDirectory, pathSegments[pathSegments.Length - 1], System.IO.SearchOption.TopDirectoryOnly));
                    StringArray nonHiddenDirectories = new StringArray();
                    foreach (string dir in directories)
                    {
                        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dir);
                        if (0 == (info.Attributes & System.IO.FileAttributes.Hidden))
                        {
                            nonHiddenDirectories.Add(dir);
                        }
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
                string baseDirChanges = System.String.Empty;
                string relativePath = File.CombinePaths(ref baseDirChanges, pathSegments);
                if (baseDirChanges != System.String.Empty)
                {
                    baseDirectory = System.IO.Path.Combine(baseDirectory, baseDirChanges);
                }
                try
                {
                    StringArray directories = new StringArray(System.IO.Directory.GetDirectories(baseDirectory, relativePath, System.IO.SearchOption.TopDirectoryOnly));
                    StringArray nonHiddenDirectories = new StringArray();
                    foreach (string dir in directories)
                    {
                        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dir);
                        if (0 == (info.Attributes & System.IO.FileAttributes.Hidden))
                        {
                            nonHiddenDirectories.Add(dir);
                        }
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

        public void Include(object module, params string[] pathSegments)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", module.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            ProxyModulePath proxyPath = (module as IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            StringArray paths = GetDirectories(packagePath, pathSegments);
            foreach (string path in paths)
            {
                if (!this.directoryPaths.Contains(path))
                {
                    this.directoryPaths.Add(path);
                }
            }
        }

        public void AddRange(string[] absolutePaths)
        {
            foreach (string absolutePath in absolutePaths)
            {
                this.Add(absolutePath, false);
            }
        }

        public void AddRange(StringArray absolutePaths)
        {
            foreach (string absolutePath in absolutePaths)
            {
                this.Add(absolutePath, false);
            }
        }

        public void AddRange(PackageInformation package, string[] relativePaths)
        {
            if (null == package)
            {
                throw new Exception("Use AddRange without a package");
            }

            foreach (string path in relativePaths)
            {
                this.Add(package, path);
            }
        }

        public void AddRange(PackageInformation package, Opus.Core.StringArray relativePaths)
        {
            if (null == package)
            {
                throw new Exception("Use AddRange without a package");
            }

            foreach (string path in relativePaths)
            {
                this.Add(package, path);
            }
        }

        public void AddRange(object owner, string[] relativePaths)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            this.AddRange(package, relativePaths);
        }

        public void AddRange(object owner, StringArray relativePaths)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            this.AddRange(package, relativePaths);
        }

#if NEWDIRECTORYCOLLECTION
        public string this[int index]
        {
            get
            {
                return this.directoryPaths[index];
            }
        }
#else
        public PackageAndDirectoryPath this[int index]
        {
            get
            {
                return this.directoryList[index];
            }
        }
#endif

        public int Count
        {
            get
            {
#if NEWDIRECTORYCOLLECTION
                return this.directoryPaths.Count;
#else
                return this.directoryList.Count;
#endif
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return new DirectoryCollectionEnumerator(this);
        }
    }
}