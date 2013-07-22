// <copyright file="ProxyModulePath.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class ProxyModulePath
    {
        private string combinedPathSegments;
        private string[] pathSegments;

        public ProxyModulePath()
        {
            this.combinedPathSegments = null;
            this.pathSegments = null;
        }

        public ProxyModulePath(params string[] segments)
        {
            this.pathSegments = segments;
            var combinedPath = string.Empty;
            foreach (var path in segments)
            {
                combinedPath = System.IO.Path.Combine(combinedPath, path);
            }

            this.combinedPathSegments = combinedPath;
        }

        public Location Combine(Location root)
        {
            if (null == this.pathSegments)
            {
                return root;
            }

            var combinedRoot = new LocationDirectory(root, this.pathSegments);
            return combinedRoot;
        }

        // deprecated
        public string Combine(PackageIdentifier packageId)
        {
            if (null == this.combinedPathSegments)
            {
                return packageId.Path;
            }

            var combinedPath = System.IO.Path.Combine(packageId.Path, this.combinedPathSegments);
            combinedPath = File.CanonicalPath(combinedPath);

            return combinedPath;
        }
    }
}
