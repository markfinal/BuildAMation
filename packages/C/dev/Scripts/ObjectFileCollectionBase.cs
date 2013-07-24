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

        protected Opus.Core.Location IncludeRoot
        {
            get;
            set;
        }

        protected Opus.Core.StringArray IncludePathSegments
        {
            get;
            set;
        }

        protected Opus.Core.Location ExcludeRoot
        {
            get;
            set;
        }

        protected Opus.Core.StringArray ExcludePathSegments
        {
            get;
            set;
        }

        private Opus.Core.StringArray EvaluatePaths()
        {
            // TODO: Remove Cached Path and remove ToArray
            var includePathList = Opus.Core.File.GetFiles(this.IncludeRoot.CachedPath, this.IncludePathSegments.ToArray());
            if (null == this.ExcludeRoot)
            {
                return includePathList;
            }

            var excludePathList = Opus.Core.File.GetFiles(this.ExcludeRoot.CachedPath, this.ExcludePathSegments.ToArray());
            var remainingPathList = new Opus.Core.StringArray(includePathList.Complement(excludePathList));
            return remainingPathList;
        }

        protected virtual System.Collections.Generic.List<ObjectFile> MakeChildModules(Opus.Core.StringArray pathList)
        {
            throw new Opus.Core.Exception("This needs to be implemented");
        }

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();
#if true
            var pathList = this.EvaluatePaths();
            var childModules = this.MakeChildModules(pathList);
            foreach (var objectFile in childModules)
            {
                collection.Add(objectFile as Opus.Core.IModule);
            }
#else
            foreach (var objectFile in this.list)
            {
                collection.Add(objectFile as Opus.Core.IModule);
            }
#endif
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
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
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