namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalOptionsDelegateAttribute : System.Attribute
    {
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalOptionsDelegateAttribute), typeof(ExportOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("FileUtilities")]
    public interface ICopyFileTool : Opus.Core.ITool
    {
    }

    class CopyFileTool : ICopyFileTool
    {
        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

    [Opus.Core.ModuleToolAssignment(typeof(ICopyFileTool))]
    public class CopyFile : Opus.Core.BaseModule
    {
        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public CopyFile()
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

    public enum OutputFileFlags
    {
        CopiedFile = (1 << 0)
    }

    public partial class CopyFileOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ICopyFileOptions
    {
        public CopyFileOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void InitializeDefaults (Opus.Core.DependencyNode owningNode)
        {
            ICopyFileOptions options = this as ICopyFileOptions;
            options.DestinationDirectory = null;
        }
        #endregion

        public override void FinalizeOptions (Opus.Core.DependencyNode node)
        {
            string sourceFileName = System.IO.Path.GetFileName((node.Module as CopyFile).SourceFile.AbsolutePath);
            ICopyFileOptions options = this as ICopyFileOptions;
            if (options.DestinationDirectory != null)
            {
                this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(options.DestinationDirectory, sourceFileName);
            }
            else
            {
                this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(node.GetModuleBuildDirectory(), sourceFileName);
            }
            base.FinalizeOptions (node);
        }

        #region ICommandLineSupport implementation

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments (Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            throw new System.NotImplementedException ();
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate ()
        {
            throw new System.NotImplementedException ();
        }

        #endregion
    }
}
