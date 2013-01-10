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
            bool containsSeparators = pathSegment.Contains(DirectorySeparatorString) || pathSegment.Contains(AltDirectorySeparatorString);
            return containsSeparators;
        }

        private static void ValidateFilePart(string pathSegment)
        {
            if (ContainsDirectorySeparators(pathSegment))
            {
                throw new Exception("Individual file parts cannot contain directory separators; '{0}'", pathSegment);
            }
        }

        internal static string CombinePaths(ref string baseDirectory, params string[] pathSegments)
        {
            string combinedPath = baseDirectory;
            bool canExtendBaseDirectoryWithUps = true;
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
            string canonicalPath = System.IO.Path.GetFullPath(new System.Uri(path).LocalPath);
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

        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            string packagePath = package.Identifier.Path;
            ProxyModulePath proxyPath = (owner as BaseModule).ProxyPath;
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

        public static StringArray GetFiles(string baseDirectory, params string[] pathSegments)
        {
            if (State.RunningMono)
            {
                // workaround for this Mono bug http://www.mail-archive.com/mono-bugs@lists.ximian.com/msg71506.html
                // cannot use GetFiles with a pattern containing directories

                string combinedBaseDirectory = baseDirectory;
                int i = 0;
                for (; i < pathSegments.Length; ++i)
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
                    throw new Exception("Unable to locate path, starting with '{0}' and ending in '{1}'", combinedBaseDirectory, pathSegments[i]);
                }

                combinedBaseDirectory = System.IO.Path.GetFullPath(combinedBaseDirectory);

                string[] files = System.IO.Directory.GetFiles(combinedBaseDirectory, pathSegments[pathSegments.Length - 1], System.IO.SearchOption.TopDirectoryOnly);
                return new StringArray(files);
            }
            else
            {
                string baseDirChanges = System.String.Empty;
                string relativePath = CombinePaths(ref baseDirChanges, pathSegments);
                if (baseDirChanges != System.String.Empty)
                {
                    baseDirectory = System.IO.Path.Combine(baseDirectory, baseDirChanges);
                    baseDirectory = System.IO.Path.GetFullPath(baseDirectory);
                }
                try
                {
                    string[] files = System.IO.Directory.GetFiles(baseDirectory, relativePath, System.IO.SearchOption.TopDirectoryOnly);
                    return new StringArray(files);
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Log.Detail("Warning: No files match the pattern {0}{1}{2}", baseDirectory, System.IO.Path.DirectorySeparatorChar, relativePath);
                    return new StringArray();
                }
            }
        }

        public bool IsValid
        {
            get
            {
                bool isValid = (null != this.absolutePath);
                return isValid;
            }
        }

        public string AbsolutePath
        {
            get
            {
                if (null == this.absolutePath)
                {
                    throw new Exception("File path has not been set", false);
                }

                return this.absolutePath;
            }

            private set
            {
                this.absolutePath = value;
            }
        }
    }
}