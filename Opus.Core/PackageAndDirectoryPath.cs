// <copyright file="PackageAndDirectoryPath.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class PackageAndDirectoryPath : System.Collections.IComparer
    {
        public PackageAndDirectoryPath(PackageInformation package, string relativePath, ProxyModulePath proxyPath)
        {
            this.Package = package;
            this.RelativePath = relativePath;
            this.ProxyPath = proxyPath;
        }

        public PackageInformation Package
        {
            get;
            private set;
        }

        public string RelativePath
        {
            get;
            private set;
        }

        public ProxyModulePath ProxyPath
        {
            get;
            private set;
        }

        public int Compare(object x, object y)
        {
            PackageAndDirectoryPath papX = x as PackageAndDirectoryPath;
            PackageAndDirectoryPath papY = y as PackageAndDirectoryPath;

            if (papX.Package.Equals(papY.Package) &&
                papX.RelativePath.Equals(papY.RelativePath) &&
                papX.ProxyPath.Equals(papY.ProxyPath))
            {
                return 0;
            }

            return 1;
        }

        public string GetAbsolutePath()
        {
            if (null == this.Package)
            {
                return this.RelativePath;
            }
            else
            {
                string packagePath = this.Package.Identifier.Path;
                if (null != this.ProxyPath)
                {
                    packagePath = this.ProxyPath.Combine(this.Package.Identifier);
                }

                string includePath = this.RelativePath;
                bool isAbsolutePath = System.IO.Path.IsPathRooted(includePath);
                if (!isAbsolutePath)
                {
                    includePath = System.IO.Path.Combine(packagePath, includePath);
                }
                return includePath;
            }
        }
    }
}