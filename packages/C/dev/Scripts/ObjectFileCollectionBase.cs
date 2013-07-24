// <copyright file="ObjectFileCollectionBase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ObjectFileCollectionBase : Opus.Core.BaseModule, Opus.Core.IModuleCollection
    {
        protected System.Collections.Generic.List<ObjectFile> list = new System.Collections.Generic.List<ObjectFile>();

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();

            foreach (var objectFile in this.list)
            {
                collection.Add(objectFile as Opus.Core.IModule);
            }

            return collection;
        }

        public Opus.Core.IModule GetChildModule(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null != this.ProxyPath)
            {
                root = this.ProxyPath.Combine(root);
            }

            // TODO: replace with Location
            var filePaths = Opus.Core.File.GetFiles(root.CachedPath, pathSegments);
            if (filePaths.Count != 1)
            {
                throw new Opus.Core.Exception("Path segments resolve to more than one file:\n{0}", filePaths.ToString('\n'));
            }

            var pathToFind = filePaths[0];
            foreach (var objFile in this.list)
            {
                if (objFile.SourceFile.AbsolutePath == pathToFind)
                {
                    return objFile;
                }
            }

            return null;
        }

        // deprecated
        public Opus.Core.IModule GetChildModule(object owner, params string[] pathSegments)
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
            if (filePaths.Count != 1)
            {
                throw new Opus.Core.Exception("Path segments resolve to more than one file:\n{0}", filePaths.ToString('\n'));
            }

            var pathToFind = filePaths[0];

            foreach (var objFile in this.list)
            {
                if (objFile.SourceFile.AbsolutePath == pathToFind)
                {
                    return objFile;
                }
            }

            return null;
        }
    }
}