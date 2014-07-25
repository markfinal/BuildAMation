// <copyright file="RelativePathUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class RelativePathUtilities
    {
        public static bool
        IsPathAbsolute(
            string path)
        {
            var pathUri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
            var isAbsolute = pathUri.IsAbsoluteUri;
            return isAbsolute;
        }

        public static string
        GetPath(
            System.Uri pathUri,
            System.Uri relativeToUri)
        {
            return GetPath(pathUri, relativeToUri, null);
        }

        public static string
        GetPath(
            System.Uri pathUri,
            System.Uri relativeToUri,
            string relativePrefix)
        {
            var relativePathUri = pathUri.IsAbsoluteUri ? relativeToUri.MakeRelativeUri(pathUri) : pathUri;
            if (relativePathUri.IsAbsoluteUri || System.IO.Path.IsPathRooted(relativePathUri.ToString()))
            {
                // should return the path that pathUri was constructed with
                return pathUri.LocalPath;
            }
            else
            {
                var relativePath = relativePathUri.ToString();
                relativePath = System.Uri.UnescapeDataString(relativePath);
                if (null != relativePrefix)
                {
                    relativePath = System.IO.Path.Combine(relativePrefix, relativePath);
                }
                if (OSUtilities.IsWindowsHosting)
                {
                    relativePath = relativePath.Replace('/', '\\');
                }
                return relativePath;
            }
        }

        public static string
        GetPath(
            string path,
            System.Uri relativeToUri,
            string relativePrefix)
        {
            if (null == path)
            {
                Opus.Core.Log.DebugMessage("Null relative path requested");
                return path;
            }

            // special cases
            if ("." == path || ".." == path)
            {
                return path;
            }

            var pathUri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
            return GetPath(pathUri, relativeToUri, relativePrefix);
        }

        public static string
        GetPath(
            string path,
            System.Uri relativeToUri)
        {
            return GetPath(path, relativeToUri, null);
        }

        public static string
        GetPath(
            Location location,
            System.Uri relativeToUri)
        {
            return GetPath(location.GetSinglePath(), relativeToUri, null);
        }

        public static string
        GetPath(
            string path,
            string relativeToString,
            string relativePrefix)
        {
            var relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            var relativePath = GetPath(path, relativeToUri, relativePrefix);
            return relativePath;
        }

        public static string
        GetPath(
            string path,
            string relativeToString)
        {
            var relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            var relativePath = GetPath(path, relativeToUri);
            return relativePath;
        }

        public static string
        GetPath(
            Location location,
            string relativeToString)
        {
            var relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            var relativePath = GetPath(location, relativeToUri);
            return relativePath;
        }

        public static string
        MakeRelativePathAbsoluteToWorkingDir(
            string relativePath)
        {
            var relativePathUri = new System.Uri(relativePath, System.UriKind.RelativeOrAbsolute);
            if (!relativePathUri.IsAbsoluteUri)
            {
                relativePathUri = new System.Uri(System.IO.Path.Combine(Core.State.WorkingDirectory, relativePath));
            }

            var absolutePath = relativePathUri.AbsolutePath;
            absolutePath = System.IO.Path.GetFullPath(absolutePath);
            absolutePath = System.Uri.UnescapeDataString(absolutePath);
            if (Core.OSUtilities.IsWindowsHosting)
            {
                absolutePath = absolutePath.Replace('/', '\\');
            }

            return absolutePath;
        }

        public static string
        GetCommonRoot(
            string path1,
            string path2)
        {
            var path1Parts = path1.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
            var path2Parts = path2.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });

            string commonRoot = null;
            for (int i = 0; i < path1Parts.Length; ++i)
            {
                if (i >= path2Parts.Length)
                {
                    break;
                }

                if (!path1Parts[i].Equals(path2Parts[i]))
                {
                    break;
                }

                if (null != commonRoot)
                {
                    commonRoot = System.IO.Path.Combine(commonRoot, path1Parts[i]);
                }
                else
                {
                    commonRoot = path1Parts[i];
                    if (commonRoot.EndsWith(System.IO.Path.VolumeSeparatorChar.ToString()))
                    {
                        commonRoot += System.IO.Path.DirectorySeparatorChar;
                    }
                }
            }

            return commonRoot;
        }
    }
}
