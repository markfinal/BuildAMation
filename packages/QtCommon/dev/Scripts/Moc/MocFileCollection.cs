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
        private System.Collections.Generic.List<MocFile> list = new System.Collections.Generic.List<MocFile>();

#if true
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

        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            this.IncludeRoot = root;
            this.IncludePathSegments = new Opus.Core.StringArray(pathSegments);
        }

        public void Exclude(Opus.Core.Location root, params string[] pathSegments)
        {
            this.ExcludeRoot = root;
            this.ExcludePathSegments = new Opus.Core.StringArray(pathSegments);
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

        private System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.StringArray pathList)
        {
            var moduleCollection = new System.Collections.Generic.List<Opus.Core.IModule>();
            foreach (var path in pathList)
            {
                var mocFile = new MocFile();
                mocFile.ProxyPath.Assign(this.ProxyPath);
                mocFile.SourceFile.AbsolutePath = path;
                moduleCollection.Add(mocFile);
            }
            return moduleCollection;
        }

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();
            var pathList = this.EvaluatePaths();
            var childModules = this.MakeChildModules(pathList);
            if (null != this.DeferredUpdates)
            {
                foreach (var objectFile in childModules)
                {
                    var objectFileDeferredLocation = new Opus.Core.DeferredLocations((objectFile as MocFile).SourceFile.AbsolutePath);
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
        private System.Collections.Generic.Dictionary<Opus.Core.DeferredLocations, Opus.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

        public void RegisterUpdateOptions(Opus.Core.UpdateOptionCollectionDelegateArray delegateArray, Opus.Core.Location root, params string[] pathSegments)
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