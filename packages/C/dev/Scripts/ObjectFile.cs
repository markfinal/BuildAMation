// <copyright file="ObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFile : Opus.Core.IModule
    {
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
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public ObjectFile()
        {
            this.SourceFile = new Opus.Core.File();
        }

        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            this.SourceFile.SetRelativePath(owner, pathSegments);
        }

        public void SetPackageRelativePath(Opus.Core.PackageInformation package, params string[] pathSegments)
        {
            this.SourceFile.SetPackageRelativePath(package, pathSegments);
        }

        public void SetAbsolutePath(string absolutePath)
        {
            this.SourceFile.SetAbsolutePath(absolutePath);
        }

        public void SetGuaranteedAbsolutePath(string absolutePath)
        {
            this.SourceFile.SetGuaranteedAbsolutePath(absolutePath);
        }
    }
}