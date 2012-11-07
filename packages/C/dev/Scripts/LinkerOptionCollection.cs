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
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            Opus.Core.Target target = node.Target;

            // NEW STYLE
#if true
#if true
            Opus.Core.IToolset toolset = target.Toolset;
            ILinkerTool linkerTool = toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;
#else
            Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
            if (null == toolset)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            ILinkerInfo linkerTool = toolset as ILinkerInfo;
            if (null == linkerTool)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolset.GetType().ToString(), typeof(ILinkerInfo).ToString(), target.Toolchain), false);
            }
#endif

            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(linkerTool.BinaryOutputSubDirectory);
            this.LibraryDirectoryPath = node.GetTargettedModuleBuildDirectory(linkerTool.ImportLibrarySubDirectory);
#else
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.BinaryOutputSubDirectory);
            this.LibraryDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.LibraryOutputSubDirectory);
#endif

            ILinkerOptions linkerOptions = this as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.Executable;

#if false
            linkerOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);
#endif

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

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            Opus.Core.IToolset toolset = target.Toolset;

            ILinkerTool linkerTool = toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;

            ILinkerOptions options = this as ILinkerOptions;

            if (null == this.OutputFilePath)
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

                string outputPathName = System.IO.Path.Combine(this.OutputDirectoryPath, outputPrefix + this.OutputName) + outputSuffix;
                this.OutputFilePath = outputPathName;
            }

            if (options.DynamicLibrary && null == this.StaticImportLibraryFilePath)
            {
                if (target.HasPlatform(Opus.Core.EPlatform.Windows))
                {
                    // explicit import library
                    string importLibraryPathName = System.IO.Path.Combine(this.LibraryDirectoryPath, linkerTool.ImportLibraryPrefix + this.OutputName) + linkerTool.ImportLibrarySuffix;
                    this.StaticImportLibraryFilePath = importLibraryPathName;
                }
                else
                {
                    // shared objects
                    this.StaticImportLibraryFilePath = this.OutputFilePath;
                }
            }

            if (options.GenerateMapFile && null == this.MapFilePath)
            {
                string mapPathName = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + linkerTool.MapFileSuffix;
                this.MapFilePath = mapPathName;
            }
#else
            Toolchain toolchain = ToolchainFactory.GetTargetInstance(target);

            ILinkerOptions options = this as ILinkerOptions;

            if (null == this.OutputFilePath)
            {
                string outputPrefix = string.Empty;
                string outputSuffix = string.Empty;
                if (options.OutputType == ELinkerOutput.Executable)
                {
                    outputSuffix = toolchain.ExecutableSuffix;
                }
                else if (options.OutputType == ELinkerOutput.DynamicLibrary)
                {
                    outputPrefix = toolchain.DynamicLibraryPrefix;
                    outputSuffix = toolchain.DynamicLibrarySuffix;
                }

                string outputPathName = System.IO.Path.Combine(this.OutputDirectoryPath, outputPrefix + this.OutputName) + outputSuffix;
                this.OutputFilePath = outputPathName;
            }

            if (options.DynamicLibrary && null == this.StaticImportLibraryFilePath)
            {
                if (target.HasPlatform(Opus.Core.EPlatform.Windows))
                {
                    // explicit import library
                    string importLibraryPathName = System.IO.Path.Combine(this.LibraryDirectoryPath, toolchain.StaticImportLibraryPrefix + this.OutputName) + toolchain.StaticImportLibrarySuffix;
                    this.StaticImportLibraryFilePath = importLibraryPathName;
                }
                else
                {
                    // shared objects
                    this.StaticImportLibraryFilePath = this.OutputFilePath;
                }
            }

            if (options.GenerateMapFile && null == this.MapFilePath)
            {
                string mapPathName = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + toolchain.MapFileSuffix;
                this.MapFilePath = mapPathName;
            }
#endif

            base.FinalizeOptions(target);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}