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
        private System.Collections.Generic.List<CopyFile> copyFiles = new System.Collections.Generic.List<CopyFile>();
        private string commonBaseDirectory;

        public string CommonBaseDirectory
        {
            get
            {
                return this.commonBaseDirectory;
            }
        }

        public void Include(Opus.Core.Location root, Opus.Core.Target target, params string[] pathSegments)
        {
            if (null != this.ProxyPath)
            {
                root = this.ProxyPath.Combine(root);
            }

            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);

            string commonBaseDirectory;
            // TODO: replace with Location
            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(out commonBaseDirectory, root.CachedPath, pathSegments);
            foreach (string path in filePaths)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                (file as Opus.Core.BaseModule).ProxyPath = (this as Opus.Core.BaseModule).ProxyPath;
                file.SourceFile.SetAbsolutePath(path);
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

        public void Exclude(Opus.Core.Location root, params string[] pathSegments)
        {
            if (null != this.ProxyPath)
            {
                root = this.ProxyPath.Combine(root);
            }
            
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
        }

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
                packagePath = proxyPath.Combine(package.Identifier);
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
                (file as Opus.Core.BaseModule).ProxyPath = (this as Opus.Core.BaseModule).ProxyPath;
                file.SourceFile.SetAbsolutePath(path);
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
                packagePath = proxyPath.Combine(package.Identifier);
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

        
        #region IModuleCollection implementation
        
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
