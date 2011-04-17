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

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                commandLineBuilder.Add("/c");
                commandLineBuilder.Add("COPY");
            }
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
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