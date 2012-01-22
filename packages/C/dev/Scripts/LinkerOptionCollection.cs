// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public enum ESubsystem
    {
        NotSet = 0,
        Console = 1,
        Windows = 2
    }

    public abstract class LinkerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        // TODO:  no reason why this can't be a static utility function
        protected virtual void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.BinaryOutputSubDirectory);
            this.LibraryDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.LibraryOutputSubDirectory);

            ILinkerOptions linkerOptions = this as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.Executable;

            linkerOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);

            Opus.Core.Target target = node.Target;

            Opus.Core.EConfiguration configuration = target.Configuration;

            linkerOptions.SubSystem = ESubsystem.NotSet;
            linkerOptions.DoNotAutoIncludeStandardLibraries = true;
            if (Opus.Core.EConfiguration.Debug == configuration)
            {
                linkerOptions.DebugSymbols = true;
            }
            else
            {
                if (Opus.Core.EConfiguration.Profile != configuration)
                {
                    linkerOptions.DebugSymbols = false;
                }
                else
                {
                    linkerOptions.DebugSymbols = true;
                }
            }
            linkerOptions.DynamicLibrary = false;
            linkerOptions.LibraryPaths = new Opus.Core.DirectoryCollection();
            linkerOptions.GenerateMapFile = true;
            linkerOptions.Libraries = new Opus.Core.FileCollection();
            linkerOptions.StandardLibraries = new Opus.Core.FileCollection();
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
        {
            this.InitializeDefaults(node);

            ILinkerOptions linkerOptions = this as ILinkerOptions;
            linkerOptions.AdditionalOptions = "";

            this.SetDelegates(node);
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

        public string LibraryDirectoryPath
        {
            get;
            set;
        }

        public abstract string OutputFilePath
        {
            get;
            set;
        }

        public abstract string StaticImportLibraryFilePath
        {
            get;
            set;
        }

        public abstract string MapFilePath
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