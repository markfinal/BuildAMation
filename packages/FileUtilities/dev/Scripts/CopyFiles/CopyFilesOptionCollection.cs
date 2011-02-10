// <copyright file="CopyFilesOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public sealed class CopyFilesOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        private void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.DestinationDirectory = null;
        }

        public CopyFilesOptionCollection(Opus.Core.DependencyNode node)
        {
            this.InitializeDefaults(node);
        }

        public string DestinationDirectory
        {
            get;
            set;
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.DestinationDirectory)
            {
                directoriesToCreate.Add(null, this.DestinationDirectory);
            }

            return directoriesToCreate;
        }
    }
}