// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    /// <summary>
    /// Create meta data from a collection of C++ header or source files
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IMocTool))]
    public abstract class MocFileCollection : Opus.Core.BaseModule, Opus.Core.IModuleCollection
    {
        private Opus.Core.Array<MocFile> list = new Opus.Core.Array<MocFile>();

#if true
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

        private System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.Array<Opus.Core.Location> locationList)
        {
            var moduleCollection = new System.Collections.Generic.List<Opus.Core.IModule>();
            foreach (var location in locationList)
            {
                var copyFile = new MocFile();
                copyFile.ProxyPath.Assign(this.ProxyPath);
                copyFile.SourceFile.AbsoluteLocation = location;
                moduleCollection.Add(copyFile);
            }
            return moduleCollection;
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
                    var location = (objectFile as MocFile).SourceFile.AbsoluteLocation;
                    if (this.DeferredUpdates.ContainsKey(location))
                    {
                        foreach (var updateDelegate in this.DeferredUpdates[location])
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
#else
        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();

            foreach (MocFile mocFile in this.list)
            {
                collection.Add(mocFile as Opus.Core.IModule);
            }

            return collection;
        }
#endif

#if false
        // deprecated
        public void Include(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            string packagePath = package.Identifier.Path;
            Opus.Core.ProxyModulePath proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            foreach (string path in filePaths)
            {
                MocFile mocFile = new MocFile();
                mocFile.ProxyPath.Assign(this.ProxyPath);
                mocFile.SourceFile.AbsolutePath = path;
                this.list.Add(mocFile);
            }
        }

        // deprecated
        public void Exclude(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            string packagePath = package.Identifier.Path;
            Opus.Core.ProxyModulePath proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            System.Collections.Generic.List<MocFile> toRemove = new System.Collections.Generic.List<MocFile>();
            foreach (string path in filePaths)
            {
                foreach (MocFile file in this.list)
                {
                    if (file.SourceFile.AbsolutePath == path)
                    {
                        toRemove.Add(file);
                    }
                }
            }

            foreach (MocFile file in toRemove)
            {
                this.list.Remove(file);
            }
        }
#endif

        #region IModuleCollection Members

#if true
        private System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

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
        public Opus.Core.IModule GetChildModule(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            string packagePath = package.Identifier.Path;
            Opus.Core.ProxyModulePath proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            if (filePaths.Count != 1)
            {
                throw new Opus.Core.Exception("Path segments resolve to more than one file:\n{0}", filePaths.ToString('\n'));
            }

            string pathToFind = filePaths[0];

            foreach (MocFile mocFile in this.list)
            {
                if (mocFile.SourceFile.AbsolutePath == pathToFind)
                {
                    return mocFile;
                }
            }

            return null;
        }
#endif
        #endregion
    }
}