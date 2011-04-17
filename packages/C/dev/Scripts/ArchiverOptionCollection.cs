// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ArchiverOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected virtual void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.LibraryOutputSubDirectory);

            IArchiverOptions archiverOptions = this as IArchiverOptions;

            archiverOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
        {
            this.InitializeDefaults(node);
        }

        public string OutputName
        {
            get;
            set;
        }

        public string OutputDirectoryPath
        {
            get;
            set;
        }

        public abstract string LibraryFilePath
        {
            get;
            set;
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}