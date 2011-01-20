namespace Qt
{
    public class ExportMocOptionsDelegateAttribute : System.Attribute
    {
    }

    public class LocalMocOptionsDelegateAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(MocTool),
                                   typeof(ExportMocOptionsDelegateAttribute),
                                   typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(MocOptionCollection))]
    public abstract class MocFile : Opus.Core.IModule, Opus.Core.IInjectModules
    {
        public void SetPath(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            string[] splitBaseDir = package.Directory.Split(System.IO.Path.DirectorySeparatorChar);
            string[] newPathSegments = new string[splitBaseDir.Length + pathSegments.Length];
            System.Array.Copy(splitBaseDir, 0, newPathSegments, 0, splitBaseDir.Length);
            System.Array.Copy(pathSegments, 0, newPathSegments, splitBaseDir.Length, pathSegments.Length);
            newPathSegments[0] += System.IO.Path.DirectorySeparatorChar;
            this.SourceFile = new Opus.Core.File(newPathSegments);
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

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            return null;
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.ModuleCollection GetInjectedModules(Opus.Core.Target target)
        {
            MocOptionCollection options = this.Options as MocOptionCollection;
            string outputPath = options.MocOutputPath;
            C.CPlusPlus.ObjectFile injectedFile = new C.CPlusPlus.ObjectFile(outputPath);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}