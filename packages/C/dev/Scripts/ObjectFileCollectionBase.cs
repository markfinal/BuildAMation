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

        protected System.Collections.Generic.List<Opus.Core.DeferredLocations> Includes
        {
            get;
            set;
        }

        protected System.Collections.Generic.List<Opus.Core.DeferredLocations> Excludes
        {
            get;
            set;
        }

        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null == this.Includes)
            {
                this.Includes = new System.Collections.Generic.List<Opus.Core.DeferredLocations>();
            }

            this.Includes.Add(new Opus.Core.DeferredLocations(root, pathSegments));
        }

        public void Exclude(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null == this.Excludes)
            {
                this.Excludes = new System.Collections.Generic.List<Opus.Core.DeferredLocations>();
            }

            this.Excludes.Add(new Opus.Core.DeferredLocations(root, pathSegments));
        }

        private Opus.Core.StringArray EvaluatePaths()
        {
            if (null == this.Includes)
            {
                return null;
            }

            // TODO: Remove Cached Path and remove ToArray
            var includePathList = new Opus.Core.StringArray();
            foreach (var include in this.Includes)
            {
                var pathList = Opus.Core.File.GetFiles(include.Deferred.CachedPath);
                includePathList.AddRange(pathList);
            }
            if (null == this.Excludes)
            {
                return includePathList;
            }

            var excludePathList = new Opus.Core.StringArray();
            foreach (var exclude in this.Excludes)
            {
                var pathList = Opus.Core.File.GetFiles(exclude.Deferred.CachedPath);
                excludePathList.AddRange(pathList);
            }

            var remainingPathList = new Opus.Core.StringArray(includePathList.Complement(excludePathList));
            return remainingPathList;
        }

        protected virtual System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.StringArray pathList)
        {
            throw new Opus.Core.Exception("This needs to be implemented");
        }

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();

            // add in modules obtained through mechanisms other than paths
            foreach (var module in this.list)
            {
                collection.Add(module);
            }

            var pathList = this.EvaluatePaths();
            if (null == pathList)
            {
                return collection;
            }

            var childModules = this.MakeChildModules(pathList);
            if (null != this.DeferredUpdates)
            {
                foreach (var objectFile in childModules)
                {
                    var objectFileDeferredLocation = new Opus.Core.DeferredLocations((objectFile as ObjectFile).SourceFile.AbsolutePath);
                    if (this.DeferredUpdates.ContainsKey(objectFileDeferredLocation))
                    {
                        foreach (var updateDelegate in this.DeferredUpdates[objectFileDeferredLocation])
                        {
                            objectFile.UpdateOptions += updateDelegate;
                        }
                    }

                    collection.Add(objectFile);
                }
            }
            else
            {
                foreach (var objectFile in childModules)
                {
                    collection.Add(objectFile as Opus.Core.IModule);
                }
            }
            return collection;
        }

#if true
        private System.Collections.Generic.Dictionary<Opus.Core.DeferredLocations, Opus.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

        public void RegisterUpdateOptions(Opus.Core.UpdateOptionCollectionDelegateArray delegateArray,
                                          Opus.Core.Location root,
                                          params string[] pathSegments)
        {
            if (null == this.DeferredUpdates)
            {
                this.DeferredUpdates = new System.Collections.Generic.Dictionary<Opus.Core.DeferredLocations, Opus.Core.UpdateOptionCollectionDelegateArray>(new Opus.Core.DeferredLocationsComparer());
            }

            this.DeferredUpdates[new Opus.Core.DeferredLocations(root, new Opus.Core.StringArray(pathSegments))] = delegateArray;
        }
#else
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
#endif
    }
}