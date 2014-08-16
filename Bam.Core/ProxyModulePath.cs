// <copyright file="ProxyModulePath.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class ProxyModulePath
    {
        private StringArray pathSegments;

        public
        ProxyModulePath()
        {}

        public
        ProxyModulePath(
            params string[] segments)
        {
            this.pathSegments = new StringArray(segments);
        }

        public bool Empty
        {
            get
            {
                return this.pathSegments == null;
            }
        }

        public void
        Assign(
            params string[] segments)
        {
            this.pathSegments = new StringArray(segments);
        }

        public void
        Assign(
            ProxyModulePath proxy)
        {
            if (null == proxy.pathSegments)
            {
                return;
            }

            this.pathSegments = new StringArray(proxy.pathSegments);
        }

        public DirectoryLocation
        Combine(
            Location baseLocation)
        {
            if (null == this.pathSegments)
            {
                return baseLocation as DirectoryLocation;
            }

            var offset = this.pathSegments.ToString(System.IO.Path.DirectorySeparatorChar);
            var basePath = baseLocation.AbsolutePath;
            var combined = System.IO.Path.Combine(basePath, offset);
            return DirectoryLocation.Get(System.IO.Path.GetFullPath(combined));
        }
    }
}
