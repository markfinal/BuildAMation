// <copyright file="ObjectFileCollectionBase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ObjectFileCollectionBase : Opus.Core.BaseModule, Opus.Core.IModuleCollection
    {
        protected Opus.Core.Array<ObjectFile> list = new Opus.Core.Array<ObjectFile>();

        protected Opus.Core.Array<Opus.Core.Location> Includes
        {
            get;
            set;
        }

        protected Opus.Core.Array<Opus.Core.Location> Excludes
        {
            get;
            set;
        }

#if true
        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            if (null == this.Includes)
            {
                this.Includes = new Opus.Core.Array<Opus.Core.Location>();
            }
            this.Includes.Add(new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File));
        }

        public void Exclude(Opus.Core.Location baseLocation, string pattern)
        {
            if (null == this.Excludes)
            {
                this.Excludes = new Opus.Core.Array<Opus.Core.Location>();
            }
            this.Excludes.Add(new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File));
        }
#else
        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null == this.Includes)
            {
                this.Includes = new System.Collections.Generic.List<Opus.Core.Location>();
            }

            this.Includes.Add(new Opus.Core.Location(root, pathSegments));
        }

        public void Exclude(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null == this.Excludes)
            {
                this.Excludes = new System.Collections.Generic.List<Opus.Core.Location>();
            }

            this.Excludes.Add(new Opus.Core.Location(root, pathSegments));
        }
#endif

#if true
        private Opus.Core.Array<Opus.Core.Location> EvaluatePaths()
        {
            if (null == this.Includes)
            {
                return null;
            }

            var includePathList = new Opus.Core.Array<Opus.Core.Location>();
            foreach (var include in this.Includes)
            {
                includePathList.AddRangeUnique(include.GetLocations());
            }
            if (null == this.Excludes)
            {
                return includePathList;
            }

            var excludePathList = new Opus.Core.Array<Opus.Core.Location>();
            foreach (var exclude in this.Excludes)
            {
                excludePathList.AddRangeUnique(exclude.GetLocations());
            }

            var complement = includePathList.Complement(excludePathList);
            return complement;
        }
#else
        private Opus.Core.Array<Opus.Core.FileLocation> EvaluatePaths()
        {
            if (null == this.Includes)
            {
                return null;
            }

            var includePathList = new Opus.Core.Array<Opus.Core.FileLocation>();
            foreach (var include in this.Includes)
            {
                var locationList = Opus.Core.File.GetFiles(include);
                includePathList.AddRange(locationList);
            }
            if (null == this.Excludes)
            {
                return includePathList;
            }

            var excludePathList = new Opus.Core.Array<Opus.Core.FileLocation>();
            foreach (var exclude in this.Excludes)
            {
                var locationList = Opus.Core.File.GetFiles(exclude);
                excludePathList.AddRange(locationList);
            }

            var complement = includePathList.Complement(excludePathList);
            // TODO: ToArray here is nasty, but couldn't spot the fix
            var remainingPathList = new Opus.Core.Array<Opus.Core.FileLocation>(complement.ToArray());
            return remainingPathList;
        }
#endif

        protected virtual System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.Array<Opus.Core.Location> locationList)
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

            var locationList = this.EvaluatePaths();
            if (null == locationList)
            {
                return collection;
            }

            var childModules = this.MakeChildModules(locationList);
            if (null != this.DeferredUpdates)
            {
                foreach (var objectFile in childModules)
                {
                    var objectFileDeferredLocation = (objectFile as ObjectFile).SourceFile.AbsoluteLocation;
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
        private System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

#if true
        public void RegisterUpdateOptions(Opus.Core.UpdateOptionCollectionDelegateArray delegateArray,
                                          Opus.Core.Location baseLocation,
                                          string pattern)
        {
            if (null == this.DeferredUpdates)
            {
                this.DeferredUpdates = new System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray>(new Opus.Core.LocationComparer());
            }

            this.DeferredUpdates[new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File)] = delegateArray;
        }
#else
        public void RegisterUpdateOptions(Opus.Core.UpdateOptionCollectionDelegateArray delegateArray,
                                          Opus.Core.Location root,
                                          params string[] pathSegments)
        {
            if (null == this.DeferredUpdates)
            {
                this.DeferredUpdates = new System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray>(new Opus.Core.LocationComparer());
            }

            this.DeferredUpdates[new Opus.Core.Location(root, pathSegments)] = delegateArray;
        }
#endif
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