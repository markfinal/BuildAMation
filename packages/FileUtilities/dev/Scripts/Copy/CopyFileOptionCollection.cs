// <copyright file="CopyFileOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public partial class CopyFileOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ICopyFileOptions
    {
        public CopyFileOptionCollection()
            : base()
        {
        }

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

        private Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (node.Module.GetType() == typeof(CopyFile))
            {
                string sourceFileName = System.IO.Path.GetFileName((node.Module as CopyFile).SourceFile.AbsolutePath);
                ICopyFileOptions options = this as ICopyFileOptions;

                string destinationDirectory;
                if (options.DestinationDirectory != null)
                {
                    destinationDirectory = options.DestinationDirectory;
                }
                else
                {
                    destinationDirectory = node.GetModuleBuildDirectory();
                }

                this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(destinationDirectory, sourceFileName);
                directoriesToCreate.AddAbsoluteDirectory(destinationDirectory, false);
            }
            base.FinalizeOptions (node);
        }

        #region ICommandLineSupport implementation

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            throw new System.NotImplementedException ();
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            return this.directoriesToCreate;
        }

        #endregion
    }
}
