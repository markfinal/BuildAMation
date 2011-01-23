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
                throw new Exception(System.String.Format("Individual file parts cannot contain directory separators; '{0}'", pathSegment));
            }
        }

        private static string CombinePaths(string baseDirectory, params string[] pathSegments)
        {
            string combinedPath = baseDirectory;
            for (int i = 0; i < pathSegments.Length; ++i)
            {
                ValidateFilePart(pathSegments[i]);
                if (null == combinedPath)
                {
                    combinedPath = pathSegments[i];
                }
                else
                {
                    combinedPath = System.IO.Path.Combine(combinedPath, pathSegments[i]);
                }
            }

            return combinedPath;
        }

        private void Initialize(string basePath, bool checkExists, params string[] pathSegments)
        {
            string absolutePath;
            if (0 == pathSegments.Length)
            {
                if (checkExists && !System.IO.File.Exists(basePath))
                {
                    throw new Exception(System.String.Format("File '{0}' does not exist", basePath));
                }

                absolutePath = basePath;
            }
            else
            {
                if (checkExists && !System.IO.Directory.Exists(basePath))
                {
                    throw new Exception(System.String.Format("Base directory '{0}' does not exist", basePath));
                }

                absolutePath = CombinePaths(basePath, pathSegments);
            }

            this.AbsolutePath = absolutePath;
        }

        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            this.SetPackageRelativePath(package, pathSegments);
        }

        public void SetPackageRelativePath(Opus.Core.PackageInformation package, params string[] pathSegments)
        {
            this.Initialize(package.Directory, true, pathSegments);
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

                string baseDir = baseDirectory;
                int i = 0;
                for (; i < pathSegments.Length; ++i)
                {
                    string baseDirTest = System.IO.Path.Combine(baseDir, pathSegments[i]);
                    if (System.IO.Directory.Exists(baseDirTest))
                    {
                        baseDir = baseDirTest;
                    }
                    else
                    {
                        break;
                    }
                }

                if (i != pathSegments.Length - 1)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to locate path, starting with '{0}' and ending in '{1}'", baseDir, pathSegments[i]));
                }

                string[] files = System.IO.Directory.GetFiles(baseDir, pathSegments[pathSegments.Length - 1], System.IO.SearchOption.AllDirectories);
                return new StringArray(files);
            }
            else
            {
                string relativePath = CombinePaths(null, pathSegments);
                string[] files = System.IO.Directory.GetFiles(baseDirectory, relativePath, System.IO.SearchOption.AllDirectories);
                return new StringArray(files);
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