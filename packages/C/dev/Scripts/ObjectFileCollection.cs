// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file collection
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFileCollection : ObjectFileCollectionBase
    {
        public void Add(ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null != this.ProxyPath)
            {
                root = this.ProxyPath.Combine(root);
            }

            // TODO replace with Locations
            var filePaths = Opus.Core.File.GetFiles(root.CachedPath, pathSegments);
            foreach (var path in filePaths)
            {
                var objectFile = new ObjectFile();
                (objectFile as Opus.Core.BaseModule).ProxyPath = (this as Opus.Core.BaseModule).ProxyPath;
                objectFile.SourceFile.SetAbsolutePath(path);
                this.list.Add(objectFile);
            }
        }

        public void Exclude(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null != this.ProxyPath)
            {
                root = this.ProxyPath.Combine(root);
            }

            // TODO: replace with Location
            var filePaths = Opus.Core.File.GetFiles(root.CachedPath, pathSegments);
            var toRemove = new System.Collections.Generic.List<ObjectFile>();
            foreach (var path in filePaths)
            {
                foreach (var file in this.list)
                {
                    if (file.SourceFile.AbsolutePath == path)
                    {
                        toRemove.Add(file);
                    }
                }
            }

            foreach (var file in toRemove)
            {
                this.list.Remove(file);
            }
        }

        // deprecated
        public void Include(object owner, params string[] pathSegments)
        {
            var package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            var filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            foreach (var path in filePaths)
            {
                var objectFile = new ObjectFile();
                (objectFile as Opus.Core.BaseModule).ProxyPath = (this as Opus.Core.BaseModule).ProxyPath;
                objectFile.SourceFile.SetAbsolutePath(path);
                this.list.Add(objectFile);
            }
        }

        // deprecated
        public void Exclude(object owner, params string[] pathSegments)
        {
            var package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            var filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            var toRemove = new System.Collections.Generic.List<ObjectFile>();
            foreach (var path in filePaths)
            {
                foreach (var file in this.list)
                {
                    if (file.SourceFile.AbsolutePath == path)
                    {
                        toRemove.Add(file);
                    }
                }
            }

            foreach (var file in toRemove)
            {
                this.list.Remove(file);
            }
        }
    }
}