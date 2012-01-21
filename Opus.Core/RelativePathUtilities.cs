// <copyright file="RelativePathUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class RelativePathUtilities
    {
        public static bool IsPathAbsolute(string path)
        {
            System.Uri pathUri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
            bool isAbsolute = pathUri.IsAbsoluteUri;
            return isAbsolute;
        }

        public static string GetPath(string path, System.Uri relativeToUri, string relativePrefix)
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

            System.Uri pathUri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
            System.Uri relativePathUri = pathUri.IsAbsoluteUri ? relativeToUri.MakeRelativeUri(pathUri) : pathUri;
            if (relativePathUri.IsAbsoluteUri || System.IO.Path.IsPathRooted(relativePathUri.ToString()))
            {
                return path;
            }
            else
            {
                string relativePath = relativePathUri.ToString();
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

        public static string GetPath(string path, System.Uri relativeToUri)
        {
            return GetPath(path, relativeToUri, null);
        }

        public static string GetPath(string path, string relativeToString, string relativePrefix)
        {
            System.Uri relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            string relativePath = GetPath(path, relativeToUri, relativePrefix);
            return relativePath;
        }

        public static string GetPath(string path, string relativeToString)
        {
            System.Uri relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            string relativePath = GetPath(path, relativeToUri);
            return relativePath;
        }

        public static string MakeRelativePathAbsoluteToWorkingDir(string relativePath)
        {
            System.Uri relativePathUri = new System.Uri(relativePath, System.UriKind.RelativeOrAbsolute);
            if (!relativePathUri.IsAbsoluteUri)
            {
                relativePathUri = new System.Uri(System.IO.Path.Combine(Core.State.WorkingDirectory, relativePath));
            }

            string absolutePath = relativePathUri.AbsolutePath;
            absolutePath = System.IO.Path.GetFullPath(absolutePath);
            absolutePath = System.Uri.UnescapeDataString(absolutePath);
            if (Core.OSUtilities.IsWindowsHosting)
            {
                absolutePath = absolutePath.Replace('/', '\\');
            }

            return absolutePath;
        }
    }
}