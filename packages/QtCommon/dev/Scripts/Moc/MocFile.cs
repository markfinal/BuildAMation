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
    public class MocFile :
        Opus.Core.BaseModule,
        Opus.Core.IInjectModules,
        Opus.Core.ICommonOptionCollection
    {
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("MocdSource", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("MocdSourceDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        public static string Prefix
        {
            get
            {
                return "moc_";
            }
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFileLocation = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.Exists);
        }

        public Opus.Core.Location SourceFileLocation
        {
            get;
            set;
        }

        #region IInjectModules Members

#if false
        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            var module = this as Opus.Core.IModule;
            var options = module.Options as IMocOptions;
            var outputPath = options.MocOutputPath;
            var injectedFile = new C.Cxx.ObjectFile();
            injectedFile.SourceFileLocation = Opus.Core.FileLocation.Get(outputPath, Opus.Core.Location.EExists.WillExist);

            var moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
#endif

        string Opus.Core.IInjectModules.GetInjectedModuleNameSuffix(Opus.Core.BaseTarget baseTarget)
        {
            return "Qt4MocSourceFile";
        }

        System.Type Opus.Core.IInjectModules.GetInjectedModuleType(Opus.Core.BaseTarget baseTarget)
        {
            return typeof(C.Cxx.ObjectFile);
        }

        Opus.Core.DependencyNode Opus.Core.IInjectModules.GetInjectedParentNode(Opus.Core.DependencyNode node)
        {
            var dependentFor = node.ExternalDependentFor;
            var firstDependentFor = dependentFor[0];
            return firstDependentFor;
        }

        void Opus.Core.IInjectModules.ModuleCreationFixup(Opus.Core.DependencyNode node)
        {
            var dependent = node.ExternalDependents;
            var firstDependent = dependent[0];
            var dependentModule = firstDependent.Module;
            var module = node.Module as C.ObjectFile;
            var sourceFile = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.File);
            sourceFile.SetReference(dependentModule.Locations[MocFile.OutputFile]);
            module.SourceFileLocation = sourceFile;
        }

        #endregion

        #region ICommonOptionCollection implementation

        Opus.Core.BaseOptionCollection Opus.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }

        #endregion
    }
}