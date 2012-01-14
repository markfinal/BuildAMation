// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public class ExportMocOptionsDelegateAttribute : System.Attribute
    {
    }

    public class LocalMocOptionsDelegateAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Create meta data from a C++ header or source file
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(MocTool),
                                   typeof(ExportMocOptionsDelegateAttribute),
                                   typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(MocOptionCollection))]
    public class MocFile : Opus.Core.IModule, Opus.Core.IInjectModules
    {
        public static string Prefix
        {
            get
            {
                return "moc_";
            }
        }

        public void SetAbsolutePath(string absolutePath)
        {
            this.SourceFile = new Opus.Core.File();
            this.SourceFile.SetAbsolutePath(absolutePath);
        }

        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            this.SourceFile = new Opus.Core.File();
            this.SourceFile.SetPackageRelativePath(package, pathSegments);
        }

        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public Opus.Core.DependencyNode OwningNode
        {
            get;
            set;
        }

        public Opus.Core.ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.ModuleCollection GetInjectedModules(Opus.Core.Target target)
        {
            MocOptionCollection options = this.Options as MocOptionCollection;
            string outputPath = options.MocOutputPath;
            C.CPlusPlus.ObjectFile injectedFile = new C.CPlusPlus.ObjectFile();
            injectedFile.SetGuaranteedAbsolutePath(outputPath);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}