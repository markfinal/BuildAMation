// <copyright file="ProxyModulePath.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class ProxyModulePath
    {
        private StringArray pathSegments;

        public ProxyModulePath()
        {
            this.pathSegments = null;
        }

        public ProxyModulePath(params string[] segments)
        {
            this.pathSegments = new StringArray(segments);
        }

        public string Combine(PackageIdentifier packageId)
        {
            if (null == this.pathSegments)
            {
                return packageId.Path;
            }

            string combinedPath = packageId.Path;
            foreach (string path in this.pathSegments)
            {
                combinedPath = System.IO.Path.Combine(combinedPath, path);
            }

            combinedPath = File.CanonicalPath(combinedPath);

            return combinedPath;
        }
    }
}
