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
        public static string Prefix
        {
            get
            {
                return "moc_";
            }
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFileLocation = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File);
        }

        public Opus.Core.Location SourceFileLocation
        {
            get;
            set;
        }

        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            Opus.Core.IModule module = this as Opus.Core.IModule;
            IMocOptions options = module.Options as IMocOptions;
            string outputPath = options.MocOutputPath;
            C.Cxx.ObjectFile injectedFile = new C.Cxx.ObjectFile();
            injectedFile.SourceFileLocation = Opus.Core.FileLocation.Get(outputPath, Opus.Core.Location.EExists.WillExist);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}