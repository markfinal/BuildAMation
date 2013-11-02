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

        public Location AbsoluteLocation
        {
            get;
            set;
        }

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

        public void Include(Location baseLocation, string pattern, ScaffoldLocation.ETypeHint typeHint)
        {
            var location = new ScaffoldLocation(baseLocation, pattern, typeHint);
            this.AbsoluteLocation = location;
        }

        public void Include(Location baseLocation, string pattern)
        {
            this.Include(baseLocation, pattern, ScaffoldLocation.ETypeHint.File);
        }

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

        public string AbsolutePath
        {
            get
            {
                var locations = this.AbsoluteLocation.GetLocations();
                if (locations.Count > 1)
                {
                    throw new Exception("Expands to more than one location");
                }
                return locations[0].AbsolutePath;
            }
        }

        public override string ToString()
        {
            return this.AbsoluteLocation.ToString();
        }
    }
}