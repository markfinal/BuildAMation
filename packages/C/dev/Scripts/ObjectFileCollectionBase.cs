// <copyright file="ObjectFileCollectionBase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ObjectFileCollectionBase : Opus.Core.IModuleCollection
    {
        protected System.Collections.Generic.List<ObjectFile> list = new System.Collections.Generic.List<ObjectFile>();

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        Opus.Core.BaseOptionCollection Opus.Core.IModule.Options
        {
            get;
            set;
        }

        Opus.Core.DependencyNode Opus.Core.IModule.OwningNode
        {
            get;
            set;
        }

        public Opus.Core.ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        void Opus.Core.IModule.ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (this.UpdateOptions != null)
            {
                this.UpdateOptions(this, target);
            }
        }

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();

            foreach (ObjectFile objectFile in this.list)
            {
                collection.Add(objectFile as Opus.Core.IModule);
            }

            return collection;
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

            foreach (ObjectFile objFile in this.list)
            {
                if (objFile.SourceFile.AbsolutePath == pathToFind)
                {
                    return objFile;
                }
            }

            return null;
        }

        Opus.Core.IToolset Opus.Core.IModule.GetToolset(Opus.Core.Target target)
        {
            Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
            if (null == toolset)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            return toolset;
        }
    }
}