// <copyright file="CopyFileOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
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
