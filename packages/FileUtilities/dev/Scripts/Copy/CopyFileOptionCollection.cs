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
        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            ICopyFileOptions options = this as ICopyFileOptions;
            options.DestinationDirectory = null;
            if (typeof(CopyDirectory).IsInstanceOfType(owningNode.Module))
            {
                options.CommonBaseDirectory = (owningNode.Module as CopyDirectory).CommonBaseDirectory;
            }
            else
            {
                options.CommonBaseDirectory = null;
            }
        }
        #endregion

        private Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (typeof(CopyFile).IsInstanceOfType(node.Module))
            {
                string sourcePath = (node.Module as CopyFile).SourceFile.AbsolutePath;
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

                if (null != options.CommonBaseDirectory)
                {
                    string relPath = Opus.Core.RelativePathUtilities.GetPath(sourcePath, options.CommonBaseDirectory);
                    this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(destinationDirectory, relPath);
                }
                else
                {
                    string sourceFileName = System.IO.Path.GetFileName(sourcePath);
                    this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(destinationDirectory, sourceFileName);
                }

                string parentDir = System.IO.Path.GetDirectoryName(this.OutputPaths[OutputFileFlags.CopiedFile]);
                directoriesToCreate.AddAbsoluteDirectory(parentDir, false);
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
