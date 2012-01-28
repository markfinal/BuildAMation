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

        public ProxyModulePath()
        {
            this.combinedPathSegments = null;
        }

        public ProxyModulePath(params string[] segments)
        {
            string combinedPath = string.Empty;
            foreach (string path in segments)
            {
                combinedPath = System.IO.Path.Combine(combinedPath, path);
            }

            this.combinedPathSegments = combinedPath;
        }

        public string Combine(PackageIdentifier packageId)
        {
            if (null == this.combinedPathSegments)
            {
                return packageId.Path;
            }

            string combinedPath = System.IO.Path.Combine(packageId.Path, this.combinedPathSegments);
            combinedPath = File.CanonicalPath(combinedPath);

            return combinedPath;
        }
    }
}
