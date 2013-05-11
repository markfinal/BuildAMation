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
        private System.Collections.Generic.List<CopyFile> copyFiles = new System.Collections.Generic.List<CopyFile>();

        public void Include(Opus.Core.Target target, object outputFileEnum, params System.Type[] moduleTypes)
        {
            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            CopyFileUtilities.GetBesideModule(this, target, out besideModule, out dependentModule);

            foreach (var moduleType in moduleTypes)
            {
                CopyFile file = new CopyFile();
                file.Set(moduleType, outputFileEnum);

                if (null != dependentModule)
                {
                    file.AdditionalDependentModules.Add(dependentModule);
                }
                this.copyFiles.Add(file);
            }
        }

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
                packagePath = proxyPath.Combine(package.Identifier);
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            foreach (string path in filePaths)
            {
                CopyFile file = new CopyFile();
                (file as Opus.Core.BaseModule).ProxyPath = (this as Opus.Core.BaseModule).ProxyPath;
                file.SourceFile.SetAbsolutePath(path);
                this.copyFiles.Add(file);
            }
        }

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

        #region IIdentifyExternalDependencies implementation

        Opus.Core.TypeArray Opus.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(Opus.Core.Target target)
        {
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            CopyFileUtilities.GetBesideModule(this, target, out besideModule, out dependentModule);
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
