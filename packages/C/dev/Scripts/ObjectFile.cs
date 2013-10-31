// <copyright file="ObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFile : Opus.Core.BaseModule
    {
        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public ObjectFile()
        {
            this.SourceFile = new Opus.Core.File();
        }

#if true
        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFile.Include(baseLocation, pattern);
        }
#else
#if true
        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            // TODO: is this needed, since the proxy is incorporated into the root already in BaseModule?
            if (this.ProxyPath != null)
            {
                root = this.ProxyPath.Combine(root);
            }

            this.SourceFile.Include(root, pathSegments);
        }
#else
        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            this.SourceFile.SetRelativePath(owner, pathSegments);
        }

        public void SetPackageRelativePath(Opus.Core.PackageInformation package, params string[] pathSegments)
        {
            this.SourceFile.SetPackageRelativePath(package, pathSegments);
        }

        public void SetAbsolutePath(string absolutePath)
        {
            this.SourceFile.SetAbsolutePath(absolutePath);
        }

        public void SetGuaranteedAbsolutePath(string absolutePath)
        {
            this.SourceFile.SetGuaranteedAbsolutePath(absolutePath);
        }
#endif
#endif
    }
}