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

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFile.Include(baseLocation, pattern);
        }

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
            injectedFile.SourceFile.AbsoluteLocation = Opus.Core.FileLocation.Get(outputPath, Opus.Core.Location.EExists.WillExist);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}