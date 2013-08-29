// <copyright file="CopyFileCollectionModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ICopyFileTool))]
    public class CopyFileCollection : Opus.Core.BaseModule, Opus.Core.IModuleCollection, Opus.Core.IIdentifyExternalDependencies
    {
        private System.Collections.Generic.List<Opus.Core.IModule> copyFiles = new System.Collections.Generic.List<Opus.Core.IModule>();

        public void Include(Opus.Core.Target target, object outputFileEnum, params System.Type[] moduleTypes)
        {
            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);

            foreach (var moduleType in moduleTypes)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                file.Set(moduleType, outputFileEnum);
                this.copyFiles.Add(file);
            }
        }

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
            if (this.Includes == null)
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

        private System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.StringArray pathList)
        {
            var moduleCollection = new System.Collections.Generic.List<Opus.Core.IModule>();
            foreach (var path in pathList)
            {
                var copyFile = new CopyFile();
                copyFile.ProxyPath.Assign(this.ProxyPath);
                copyFile.SourceFile.AbsolutePath = path;
                moduleCollection.Add(copyFile);
            }
            return moduleCollection;
        }

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
                CopyFile file = new CopyFile();
                file.ProxyPath.Assign(this.ProxyPath);
                file.SourceFile.AbsolutePath = path;
                this.copyFiles.Add(file);
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
            System.Collections.Generic.List<CopyFile> toRemove = new System.Collections.Generic.List<CopyFile>();
            foreach (string path in filePaths)
            {
                foreach (CopyFile file in this.copyFiles)
                {
                    if (file.SourceFile.AbsolutePath == path)
                    {
                        toRemove.Add(file);
                    }
                }
            }

            foreach (CopyFile file in toRemove)
            {
                this.copyFiles.Remove(file);
            }
        }
#endif

        #region IModuleCollection implementation

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
        Opus.Core.IModule Opus.Core.IModuleCollection.GetChildModule(object owner, params string[] pathSegments)
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
            foreach (CopyFile file in this.copyFiles)
            {
                if (file.SourceFile.AbsolutePath == pathToFind)
                {
                    return file;
                }
            }
            return null;
        }
#endif

        #endregion

        #region INestedDependents implementation

#if true
        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();

            // add in modules obtained through mechanisms other than paths
            foreach (var module in this.copyFiles)
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
                    var objectFileDeferredLocation = new Opus.Core.DeferredLocations((objectFile as CopyFile).SourceFile.AbsolutePath);
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
            foreach (CopyFile file in this.copyFiles)
            {
                collection.Add(file as Opus.Core.IModule);
            }
            return collection;
        }
#endif

        #endregion

        #region IIdentifyExternalDependencies implementation

        Opus.Core.TypeArray Opus.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(Opus.Core.Target target)
        {
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);
            if (null == besideModule)
            {
                return null;
            }

            // each nested file needs to know where it is being copied to
            foreach (CopyFile file in this.copyFiles)
            {
                file.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                    var options = module.Options as ICopyFileOptions;
                    options.DestinationModuleType = dependentModule;
                    options.DestinationModuleOutputEnum = besideModule.OutputFileFlag;
                };
            }

            return new Opus.Core.TypeArray(dependentModule);
        }

        #endregion
    }
}
