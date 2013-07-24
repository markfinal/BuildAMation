// <copyright file="ProxyModulePath.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class ProxyModulePath
    {
        private string[] pathSegments;

        public ProxyModulePath()
        {
            this.pathSegments = null;
        }

#if true
        public void Assign(params string[] segments)
        {
            this.pathSegments = segments;
        }

        public void Assign(ProxyModulePath proxy)
        {
            this.pathSegments = new string[proxy.pathSegments.Length];
            var index = 0;
            foreach (var a in proxy.pathSegments)
            {
                this.pathSegments[index++] = a;
            }
        }
#else
        public ProxyModulePath(params string[] segments)
        {
            this.pathSegments = segments;
        }
#endif

        public Location Combine(Location root)
        {
            if (null == this.pathSegments)
            {
                return root;
            }

            var combinedRoot = new LocationDirectory(root, this.pathSegments);
            return combinedRoot;
        }
    }
}
