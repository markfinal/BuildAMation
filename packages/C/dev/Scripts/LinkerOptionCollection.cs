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
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            Opus.Core.Target target = node.Target;

            // NEW STYLE
#if true
            Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
            if (null == toolset)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            ILinkerInfo linkerInfo = toolset as ILinkerInfo;
            if (null == linkerInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolset.GetType().ToString(), typeof(ILinkerInfo).ToString(), target.Toolchain), false);
            }

            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(linkerInfo.BinaryOutputSubDirectory);
            this.LibraryDirectoryPath = node.GetTargettedModuleBuildDirectory(linkerInfo.ImportLibrarySubDirectory);
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
            Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
            if (null == toolset)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            ILinkerInfo linkerInfo = toolset as ILinkerInfo;
            if (null == linkerInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolset.GetType().ToString(), typeof(ILinkerInfo).ToString(), target.Toolchain), false);
            }

            ILinkerOptions options = this as ILinkerOptions;

            if (null == this.OutputFilePath)
            {
                string outputPrefix = string.Empty;
                string outputSuffix = string.Empty;
                if (options.OutputType == ELinkerOutput.Executable)
                {
                    outputSuffix = linkerInfo.ExecutableSuffix;
                }
                else if (options.OutputType == ELinkerOutput.DynamicLibrary)
                {
                    outputPrefix = linkerInfo.DynamicLibraryPrefix;
                    outputSuffix = linkerInfo.DynamicLibrarySuffix;
                }

                string outputPathName = System.IO.Path.Combine(this.OutputDirectoryPath, outputPrefix + this.OutputName) + outputSuffix;
                this.OutputFilePath = outputPathName;
            }

            if (options.DynamicLibrary && null == this.StaticImportLibraryFilePath)
            {
                if (target.HasPlatform(Opus.Core.EPlatform.Windows))
                {
                    // explicit import library
                    string importLibraryPathName = System.IO.Path.Combine(this.LibraryDirectoryPath, linkerInfo.ImportLibraryPrefix + this.OutputName) + linkerInfo.ImportLibrarySuffix;
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
                string mapPathName = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + linkerInfo.MapFileSuffix;
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