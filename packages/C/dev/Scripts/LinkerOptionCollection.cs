// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class LinkerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            Opus.Core.Target target = node.Target;

            ILinkerTool linkerTool = target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(linkerTool.BinaryOutputSubDirectory);
            if (linkerTool is IWinImportLibrary)
            {
                this.LibraryDirectoryPath = node.GetTargettedModuleBuildDirectory((linkerTool as IWinImportLibrary).ImportLibrarySubDirectory);
            }
            else
            {
                this.LibraryDirectoryPath = this.OutputDirectoryPath;
            }

            ILinkerOptions linkerOptions = this as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.Executable;
            linkerOptions.SubSystem = ESubsystem.NotSet;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                linkerOptions.DebugSymbols = true;
            }
            else
            {
                if (!target.HasConfiguration(Opus.Core.EConfiguration.Profile))
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
            linkerOptions.OSXApplicationBundle = false;
            linkerOptions.OSXFrameworks = new Opus.Core.StringArray();
            linkerOptions.AdditionalOptions = "";
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
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

        public string OutputFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.Executable];
            }
            set
            {
                this.OutputPaths[C.OutputFileFlags.Executable] = value;
            }
        }

        public string StaticImportLibraryFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.StaticImportLibrary];
            }
            set
            {
                this.OutputPaths[C.OutputFileFlags.StaticImportLibrary] = value;
            }
        }

        public string MapFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.MapFile];
            }
            set
            {
                this.OutputPaths[C.OutputFileFlags.MapFile] = value;
            }
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            Opus.Core.Target target = node.Target;
            ILinkerTool linkerTool = target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;
            ILinkerOptions options = this as ILinkerOptions;

            if (!this.OutputPaths.Has(C.OutputFileFlags.Executable))
            {
                string outputPrefix = string.Empty;
                string outputSuffix = string.Empty;
                if (options.OutputType == ELinkerOutput.Executable)
                {
                    outputSuffix = linkerTool.ExecutableSuffix;
                }
                else if (options.OutputType == ELinkerOutput.DynamicLibrary)
                {
                    outputPrefix = linkerTool.DynamicLibraryPrefix;
                    outputSuffix = linkerTool.DynamicLibrarySuffix;
                }

                string baseOutputPath = this.OutputDirectoryPath;
                if (target.HasPlatform(Opus.Core.EPlatform.OSX) && options.OSXApplicationBundle)
                {
                    baseOutputPath = System.IO.Path.Combine(baseOutputPath, this.OutputName + ".app");
                    baseOutputPath = System.IO.Path.Combine(baseOutputPath, "Contents");
                    baseOutputPath = System.IO.Path.Combine(baseOutputPath, "MacOS");
                }

                string outputPathName = System.IO.Path.Combine(baseOutputPath, outputPrefix + this.OutputName) + outputSuffix;
                this.OutputFilePath = outputPathName;
            }

            if (options.OutputType == ELinkerOutput.DynamicLibrary)
            {
                if (linkerTool is IWinImportLibrary)
                {
                    // explicit import library
                    IWinImportLibrary importLibrary = linkerTool as IWinImportLibrary;
                    string importLibraryPathName = System.IO.Path.Combine(this.LibraryDirectoryPath, importLibrary.ImportLibraryPrefix + this.OutputName) + importLibrary.ImportLibrarySuffix;
                    this.StaticImportLibraryFilePath = importLibraryPathName;
                }
                else
                {
                    // shared objects
                    this.StaticImportLibraryFilePath = this.OutputFilePath;
                }
            }

            if (options.GenerateMapFile && !this.OutputPaths.Has(C.OutputFileFlags.MapFile))
            {
                string mapPathName = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + linkerTool.MapFileSuffix;
                this.MapFilePath = mapPathName;
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}