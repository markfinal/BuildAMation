// <copyright file="CopyDirectoryModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ICopyFileTool))]
    public class CopyDirectory : Opus.Core.BaseModule, Opus.Core.IModuleCollection
    {
        private Opus.Core.Array<CopyFile> copyFiles = new Opus.Core.Array<CopyFile>();
        //private string commonBaseDirectory = null;

        public Opus.Core.Location CommonBaseDirectory
        {
#if true
            get;
            private set;
#else
            get
            {
                return this.commonBaseDirectory;
            }
#endif
        }

        public void Include(Opus.Core.Location baseLocation, string pattern, Opus.Core.Target target)
        {
            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);

#if true
            this.CommonBaseDirectory = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            // copy recursively
            var allFilesRecursiveScaffold = new Opus.Core.ScaffoldLocation(this.CommonBaseDirectory, "**", Opus.Core.ScaffoldLocation.ETypeHint.File);
            var locations = allFilesRecursiveScaffold.GetLocations();
            foreach (var location in locations)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                file.ProxyPath.Assign(this.ProxyPath);
                file.SourceFile.AbsoluteLocation = location;
                this.copyFiles.Add(file);
            }
#else
            string commonBaseDirectory;
            // TODO: replace with Location
            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(out commonBaseDirectory, root.CachedPath, pathSegments);
            foreach (string path in filePaths)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                file.ProxyPath.Assign(this.ProxyPath);
                file.SourceFile.AbsolutePath = path;
                this.copyFiles.Add(file);
            }

            if (null == this.commonBaseDirectory)
            {
                this.commonBaseDirectory = commonBaseDirectory;
            }
            else
            {
                var commonRoot = Opus.Core.RelativePathUtilities.GetCommonRoot(commonBaseDirectory, this.commonBaseDirectory);
                if (null == commonRoot)
                {
                    throw new Opus.Core.Exception("Unable to locate common path between '{0}' and '{1}'", commonBaseDirectory, this.commonBaseDirectory);
                }

                this.commonBaseDirectory = commonRoot;
            }
#endif
        }

        public void Exclude(Opus.Core.Location baseLocation, string pattern)
        {
#if true
            // copy recursively
            var dirs = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var allFilesRecursiveScaffold = new Opus.Core.ScaffoldLocation(dirs, "**", Opus.Core.ScaffoldLocation.ETypeHint.File);
            var locations = allFilesRecursiveScaffold.GetLocations();
            var toRemove = new Opus.Core.Array<CopyFile>();
            foreach (var location in locations)
            {
                foreach (var copyFile in this.copyFiles)
                {
                    if (copyFile.SourceFile.AbsoluteLocation == location)
                    {
                        toRemove.Add(copyFile);
                    }
                }
            }

            foreach (var file in toRemove)
            {
                this.copyFiles.Remove(file);
            }
#else
            // TODO: replace with Location
            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(root.CachedPath, pathSegments);
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
#endif
        }

#if false
        // deprecated
        public void Include(object owner, Opus.Core.Target target, params string[] pathSegments)
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

            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);

            string commonBaseDirectory;
            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(out commonBaseDirectory, packagePath, pathSegments);
            foreach (string path in filePaths)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                file.ProxyPath.Assign(this.ProxyPath);
                file.SourceFile.AbsolutePath = path;
                this.copyFiles.Add(file);
            }

            if (null == this.commonBaseDirectory)
            {
                this.commonBaseDirectory = commonBaseDirectory;
            }
            else
            {
                var commonRoot = Opus.Core.RelativePathUtilities.GetCommonRoot(commonBaseDirectory, this.commonBaseDirectory);
                if (null == commonRoot)
                {
                    throw new Opus.Core.Exception("Unable to locate common path between '{0}' and '{1}'", commonBaseDirectory, this.commonBaseDirectory);
                }

                this.commonBaseDirectory = commonRoot;
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
        public void RegisterUpdateOptions(Opus.Core.UpdateOptionCollectionDelegateArray delegateArray,
                                          Opus.Core.Location baseLocation,
                                          string pattern)
        {
            throw new System.NotImplementedException();
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
        
        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();
            foreach (CopyFile file in this.copyFiles)
            {
                collection.Add(file as Opus.Core.IModule);
            }
            return collection;
        }
        
        #endregion
    }
}
