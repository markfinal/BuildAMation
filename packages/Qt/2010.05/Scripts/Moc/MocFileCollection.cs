// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    /// <summary>
    /// Create meta data from a C++ header or source file
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(MocTool),
                                   typeof(ExportMocOptionsDelegateAttribute),
                                   typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(MocOptionCollection))]
    public abstract class MocFileCollection : Opus.Core.IModule, Opus.Core.IInjectModules
    {
        private System.Collections.Generic.List<MocFile> list = new System.Collections.Generic.List<MocFile>();

        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();

            foreach (MocFile mocFile in this.list)
            {
                collection.Add(mocFile as Opus.Core.IModule);
            }

            return collection;
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            foreach (MocFile mocFile in this.list)
            {
                MocOptionCollection options = mocFile.Options as MocOptionCollection;
                string outputPath = options.MocOutputPath;
                C.CPlusPlus.ObjectFile injectedFile = new C.CPlusPlus.ObjectFile();
                injectedFile.SetGuaranteedAbsolutePath(outputPath);

                moduleCollection.Add(injectedFile);
            }

            return moduleCollection;
        }

        public void AddRelativePaths(object owner, params string[] pathSegments)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            Opus.Core.StringArray filePaths = Opus.Core.File.GetFiles(package.Directory, pathSegments);
            foreach (string path in filePaths)
            {
                MocFile mocFile = new MocFile();
                mocFile.SetAbsolutePath(path);
                this.list.Add(mocFile);
            }
        }
    }
}