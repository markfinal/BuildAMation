// <copyright file="File.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class File
    {
        static private readonly string DirectorySeparatorString = new string(System.IO.Path.DirectorySeparatorChar, 1);
        static private readonly string AltDirectorySeparatorString = new string(System.IO.Path.AltDirectorySeparatorChar, 1);

        private string absolutePath = null;

        private static bool ContainsDirectorySeparators(string pathSegment)
        {
            var containsSeparators = pathSegment.Contains(DirectorySeparatorString) || pathSegment.Contains(AltDirectorySeparatorString);
            return containsSeparators;
        }

        private static void ValidateFilePart(string pathSegment)
        {
            if (ContainsDirectorySeparators(pathSegment))
            {
                throw new Exception("Any file part cannot contain a directory separators; '{0}'", pathSegment);
            }
        }

        internal static string CombinePaths(ref string baseDirectory, params string[] pathSegments)
        {
            var combinedPath = baseDirectory;
            var canExtendBaseDirectoryWithUps = true;
            for (int i = 0; i < pathSegments.Length; ++i)
            {
                ValidateFilePart(pathSegments[i]);
                if (null != combinedPath)
                {
                    if (pathSegments[i] == "..")
                    {
                        if (canExtendBaseDirectoryWithUps)
                        {
                            baseDirectory = System.IO.Path.Combine(baseDirectory, pathSegments[i]);
                            continue;
                        }
                    }
                    else
                    {
                        canExtendBaseDirectoryWithUps = false;
                    }

                    combinedPath = System.IO.Path.Combine(combinedPath, pathSegments[i]);
                }
                else
                {
                    combinedPath = pathSegments[i];
                }
            }

            return combinedPath;
        }

        public static string CanonicalPath(string path)
        {
            var canonicalPath = System.IO.Path.GetFullPath(new System.Uri(path).LocalPath);
            return canonicalPath;
        }

        private void Initialize(string basePath, bool checkExists, params string[] pathSegments)
        {
            string absolutePath;
            if (0 == pathSegments.Length)
            {
                if (checkExists && !System.IO.File.Exists(basePath))
                {
                    throw new Exception("File '{0}' does not exist", basePath);
                }

                absolutePath = basePath;
            }
            else
            {
                if (checkExists && !System.IO.Directory.Exists(basePath))
                {
                    throw new Exception("Base directory '{0}' does not exist", basePath);
                }

                absolutePath = CombinePaths(ref basePath, pathSegments);
            }

            this.AbsolutePath = absolutePath;
        }

#if true
        private Location Root
        {
            get;
            set;
        }

        private StringArray PathSegments
        {
            get;
            set;
        }

        private string EvaluateExpression()
        {
            // TODO: yuck, ToArray()
            var files = GetFiles(this.Root.CachedPath, this.PathSegments.ToArray());
            if (0 == files.Count)
            {
                throw new Exception("Include expression does not relate to any existing files.\n\tRoot: {0}\n\tPath segments: {1}", this.Root.CachedPath, this.PathSegments.ToString('\n'));
            }
            else if (files.Count != 1)
            {
                throw new Exception("Include expression resolves to more than one file.\n\tRoot: {0}\n\tPath segments: {1}", this.Root.CachedPath, this.PathSegments.ToString('\n'));
            }

            var path = CanonicalPath(files[0]);
            return path;
        }

        // Note: this no longer includes the module's proxy path
        public void Include(Location root, params string[] pathSegments)
        {
#if true
            this.Root = root;
            this.PathSegments = new StringArray(pathSegments);
#else
            var files = GetFiles(root.CachedPath, pathSegments);
            if (0 == files.Count)
            {
                throw new Exception("Include expression does not relate to any existing files.\n\tRoot: {0}\n\tPath segments: {1}", root.CachedPath, new StringArray(pathSegments).ToString('\n'));
            }
            else if (files.Count != 1)
            {
                throw new Exception("Include expression resolves to more than one file.\n\tRoot: {0}\n\tPath segments: {1}", root.CachedPath, new StringArray(pathSegments).ToString('\n'));
            }

            this.AbsolutePath = CanonicalPath(files[0]);
#endif
        }
#else
        // deprecated
        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            var package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (owner as BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            this.SetGuaranteedAbsolutePath(CombinePaths(ref packagePath, pathSegments));
        }

        public void SetPackageRelativePath(PackageInformation package, params string[] pathSegments)
        {
            this.Initialize(package.Identifier.Path, true, pathSegments);
        }

        public void SetAbsolutePath(string absolutePath)
        {
            this.Initialize(absolutePath, true);
        }

        public void SetGuaranteedAbsolutePath(string absolutePath)
        {
            this.Initialize(absolutePath, false);
        }
#endif

        private static bool IsFileInHiddenHierarchy(System.IO.FileInfo file, string rootDir)
        {
            if (System.IO.FileAttributes.Hidden == (file.Attributes & System.IO.FileAttributes.Hidden))
            {
                return true;
            }

            var dirInfo = file.Directory;
            while (dirInfo.FullName != rootDir)
            {
                if (System.IO.FileAttributes.Hidden == (dirInfo.Attributes & System.IO.FileAttributes.Hidden))
                {
                    return true;
                }

                dirInfo = dirInfo.Parent;
            }

            return false;
        }

        public static StringArray GetFiles(out string combinedBaseDirectory, string baseDirectory, params string[] pathSegments)
        {
            // workaround for this Mono bug http://www.mail-archive.com/mono-bugs@lists.ximian.com/msg71506.html
            // cannot use GetFiles with a pattern containing directories
            // this is also useful for getting wildcarded recursive file searches from a directory

            combinedBaseDirectory = baseDirectory;
            int i = 0;
            for (; i < pathSegments.Length; ++i)
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

            var isDirectory = false;
            if (i < pathSegments.Length - 1)
            {
                throw new Exception("Unable to locate path, starting with '{0}' and ending in '{1}'", combinedBaseDirectory, pathSegments[i]);
            }
            else if (0 == pathSegments.Length)
            {
                isDirectory = System.IO.Directory.Exists(combinedBaseDirectory);
            }
            else if (i == pathSegments.Length)
            {
                isDirectory = true;
            }

            if (isDirectory)
            {
                combinedBaseDirectory = System.IO.Path.GetFullPath(combinedBaseDirectory);
                try
                {
                    var dirInfo = new System.IO.DirectoryInfo(combinedBaseDirectory);
                    var files = dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories);
                    var nonHiddenFiles = new StringArray();
                    foreach (var file in files)
                    {
                        if (!IsFileInHiddenHierarchy(file, combinedBaseDirectory))
                        {
                            nonHiddenFiles.Add(file.FullName);
                        }
                    }
                    return nonHiddenFiles;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Log.Detail("Warning: No files match the pattern {0}{1}{2} for all subdirectory", combinedBaseDirectory, System.IO.Path.DirectorySeparatorChar, "*");
                    return new StringArray();
                }
            }
            else
            {
                var isCombinedADirectory = System.IO.Directory.Exists(combinedBaseDirectory);
                var dirInfo = isCombinedADirectory ? new System.IO.DirectoryInfo(combinedBaseDirectory) : System.IO.Directory.GetParent(combinedBaseDirectory);
                var filename = isCombinedADirectory ? pathSegments[pathSegments.Length - 1] : combinedBaseDirectory.Replace(dirInfo.FullName, string.Empty).Trim(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
                try
                {
                    var files = dirInfo.GetFiles(filename, System.IO.SearchOption.TopDirectoryOnly);
                    var nonHiddenFiles = new StringArray();
                    foreach (var file in files)
                    {
                        if (0 == (file.Attributes & System.IO.FileAttributes.Hidden))
                        {
                            nonHiddenFiles.Add(file.FullName);
                        }
                    }
                    return nonHiddenFiles;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Log.Detail("Warning: No files match the pattern {0}{1}{2} for the top directory only", combinedBaseDirectory, System.IO.Path.DirectorySeparatorChar, pathSegments[pathSegments.Length - 1]);
                    return new StringArray();
                }
            }
        }

        public static StringArray GetFiles(string baseDirectory, params string[] pathSegments)
        {
            string commonBaseDirectory;
            return GetFiles(out commonBaseDirectory, baseDirectory, pathSegments);
        }

        public string AbsolutePath
        {
            get
            {
                if (null == this.absolutePath)
                {
                    if (null != this.Root)
                    {
                        this.absolutePath = this.EvaluateExpression();
                    }
                    else
                    {
                        throw new Exception("File path has not been set");
                    }
                }

                return this.absolutePath;
            }

            set
            {
                this.absolutePath = value;
            }
        }

        public override string ToString()
        {
            return this.AbsolutePath;
        }
    }
}