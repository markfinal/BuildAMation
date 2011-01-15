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
            System.Uri relativePathUri = relativeToUri.MakeRelativeUri(pathUri);
            if (relativePathUri.IsAbsoluteUri || System.IO.Path.IsPathRooted(relativePathUri.ToString()))
            {
                return path;
            }
            else
            {
                string relativePath = relativePathUri.ToString();
                if (null != relativePrefix)
                {
                    relativePath = System.Uri.UnescapeDataString(relativePath);
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
    }
}