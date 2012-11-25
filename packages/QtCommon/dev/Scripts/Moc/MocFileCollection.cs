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

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();

            foreach (MocFile mocFile in this.list)
            {
                collection.Add(mocFile as Opus.Core.IModule);
            }

            return collection;
        }

        [System.Obsolete("Please use the Include method")]
        public void AddRelativePaths(object owner, params string[] pathSegments)
        {
            this.Include(owner, pathSegments);
        }

        public void Include(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            Opus.Core.ProxyModulePath proxyPath = (owner as Opus.Core.IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            foreach (string path in filePaths)
            {
                MocFile mocFile = new MocFile();
                (mocFile as Opus.Core.IModule).ProxyPath = (this as Opus.Core.IModule).ProxyPath;
                mocFile.SetAbsolutePath(path);
                this.list.Add(mocFile);
            }
        }

        public void Exclude(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            Opus.Core.ProxyModulePath proxyPath = (owner as Opus.Core.IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
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

        public Opus.Core.IModule GetChildModule(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            Opus.Core.ProxyModulePath proxyPath = (owner as Opus.Core.IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            if (filePaths.Count != 1)
            {
                throw new Opus.Core.Exception(System.String.Format("Path segments resolve to more than one file:\n{0}", filePaths.ToString('\n')), false);
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
    }
}