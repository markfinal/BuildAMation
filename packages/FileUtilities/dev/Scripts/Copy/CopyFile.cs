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
    }

    public class CopyFileOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        public CopyFileOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void InitializeDefaults (Opus.Core.DependencyNode owningNode)
        {
            // TODO: stub
        }
        protected override void SetDelegates (Opus.Core.DependencyNode owningNode)
        {
            // TODO: stub
        }
        #endregion

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
