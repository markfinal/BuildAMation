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
    [Opus.Core.ModuleToolAssignment(typeof(IMocTool))]
    public class MocFile : Opus.Core.BaseModule, Opus.Core.IInjectModules
    {
        public MocFile()
        {
            this.SourceFile = new Opus.Core.File();
        }

        public static string Prefix
        {
            get
            {
                return "moc_";
            }
        }

#if true
        public void Include(Opus.Core.Location root, params string[] pathSegments)
        {
            if (this.ProxyPath != null)
            {
                root = this.ProxyPath.Combine(root);
            }

            this.SourceFile.Include(root, pathSegments);
        }
#else
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
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            this.SourceFile = new Opus.Core.File();
            this.SourceFile.SetPackageRelativePath(package, pathSegments);
        }
#endif

        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            Opus.Core.IModule module = this as Opus.Core.IModule;
            IMocOptions options = module.Options as IMocOptions;
            string outputPath = options.MocOutputPath;
            C.Cxx.ObjectFile injectedFile = new C.Cxx.ObjectFile();
            injectedFile.SourceFile.AbsolutePath = outputPath;

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}