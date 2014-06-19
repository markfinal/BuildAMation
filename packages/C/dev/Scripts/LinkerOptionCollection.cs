// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class LinkerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            var target = node.Target;

            var linkerOptions = this as ILinkerOptions;
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

            var osxLinkerOptions = this as ILinkerOptionsOSX;
            if (osxLinkerOptions != null)
            {
                osxLinkerOptions.ApplicationBundle = false;
                osxLinkerOptions.Frameworks = new Opus.Core.StringArray();
                osxLinkerOptions.FrameworkSearchDirectories = new Opus.Core.DirectoryCollection();
                osxLinkerOptions.SuppressReadOnlyRelocations = false;
            }

            linkerOptions.MajorVersion = 1;
            linkerOptions.MinorVersion = 0;
            linkerOptions.PatchVersion = 0;
            linkerOptions.AdditionalOptions = "";
        }

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            var locationMap = this.OwningNode.Module.Locations;
            var moduleBuildDir = locationMap[Opus.Core.State.ModuleBuildDirLocationKey];

            var linkerTool = node.Target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;

            var outputDir = locationMap[C.Application.OutputDir];
            if (!outputDir.IsValid)
            {
                var linkerOutputDir = moduleBuildDir.SubDirectory(linkerTool.BinaryOutputSubDirectory);
                (outputDir as Opus.Core.ScaffoldLocation).SetReference(linkerOutputDir);
            }

            // special case here of the QMakeBuilder
            // QMake does not support writing import libraries to a separate location to the dll
            if (node.Module.Locations.Contains(C.DynamicLibrary.ImportLibraryDir))
            {
                var importLibDir = node.Module.Locations[C.DynamicLibrary.ImportLibraryDir];
                if (!importLibDir.IsValid)
                {
                    if (linkerTool is IWinImportLibrary && (Opus.Core.State.BuilderName != "QMake"))
                    {
                        var moduleDir = moduleBuildDir.SubDirectory((linkerTool as IWinImportLibrary).ImportLibrarySubDirectory);
                        (importLibDir as Opus.Core.ScaffoldLocation).SetReference(moduleDir);
                    }
                    else
                    {
                        (importLibDir as Opus.Core.ScaffoldLocation).SetReference(outputDir);
                    }
                }
            }

            base.SetNodeSpecificData(node);
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

#if true
#else
        public string OutputDirectoryPath
        {
            get;
            set;
        }
#endif

#if true
#else
        public string LibraryDirectoryPath
        {
            get;
            set;
        }
#endif

#if true
#else
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
#endif

#if true
#else
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
#endif

#if true
#else
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
#endif
        private static void GetBinaryPrefixAndSuffix(ILinkerOptions options, ILinkerTool tool, out string prefix, out string suffix)
        {
            switch (options.OutputType)
            {
                case ELinkerOutput.Executable:
                    prefix = string.Empty;
                    suffix = tool.ExecutableSuffix;
                    break;

                case ELinkerOutput.DynamicLibrary:
                    prefix = tool.DynamicLibraryPrefix;
                    suffix = tool.DynamicLibrarySuffix;
                    break;

                default:
                    throw new Opus.Core.Exception("Unknown output type");
            }
        }

        private string
        GetExecutableFilename(
            Opus.Core.Target target,
            ILinkerTool linkerTool,
            ILinkerOptions linkerOptions)
        {
            string prefix;
            string suffix;
            GetBinaryPrefixAndSuffix(linkerOptions, linkerTool, out prefix, out suffix);

            string filename = null;
            if (linkerOptions.OutputType == ELinkerOutput.DynamicLibrary)
            {
                var versionNumber = new System.Text.StringBuilder();
                versionNumber.AppendFormat(".{0}.{1}.{2}", linkerOptions.MajorVersion, linkerOptions.MinorVersion, linkerOptions.PatchVersion);
                if (target.HasPlatform(Opus.Core.EPlatform.Unix))
                {
                    // version number postfixes the filename
                    filename = prefix + this.OutputName + suffix + versionNumber.ToString();
                }
                else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
                {
                    // version number prefixes the extension
                    filename = prefix + this.OutputName + versionNumber.ToString() + suffix;
                }
            }
            if (null == filename)
            {
                filename = prefix + this.OutputName + suffix;
            }
            return filename;
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
#if true
            var target = node.Target;
            var linkerTool = target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;
            var options = this as ILinkerOptions;

            var locationMap = node.Module.Locations;
            var outputFile = locationMap[C.Application.OutputFile];
            if (!outputFile.IsValid)
            {
                var filename = this.GetExecutableFilename(target, linkerTool, options);
                (outputFile as Opus.Core.ScaffoldLocation).SpecifyStub(locationMap[C.Application.OutputDir], filename, Opus.Core.Location.EExists.WillExist);
            }

            if (node.Module is C.DynamicLibrary)
            {
                var importLibraryFile = locationMap[C.DynamicLibrary.ImportLibraryFile] as Opus.Core.ScaffoldLocation;
                if (!importLibraryFile.IsValid)
                {
                    if (linkerTool is IWinImportLibrary)
                    {
                        // explicit import library
                        var importLibrary = linkerTool as IWinImportLibrary;
                        var filename = importLibrary.ImportLibraryPrefix + this.OutputName + importLibrary.ImportLibrarySuffix;
                        importLibraryFile.SpecifyStub(locationMap[C.DynamicLibrary.ImportLibraryDir], filename, Opus.Core.Location.EExists.WillExist);
                    }
                    else
                    {
                        // shared objects
                        importLibraryFile.SetReference(locationMap[C.DynamicLibrary.OutputFile]);
                    }
                }
            }

            if (options.GenerateMapFile)
            {
                if (!locationMap[C.Application.MapFileDir].IsValid)
                {
                    (locationMap[C.Application.MapFileDir] as Opus.Core.ScaffoldLocation).SetReference(locationMap[C.Application.OutputDir]);
                }

                if (!locationMap[C.Application.MapFile].IsValid)
                {
                    (locationMap[C.Application.MapFile] as Opus.Core.ScaffoldLocation).SpecifyStub(locationMap[C.Application.MapFileDir], this.OutputName + linkerTool.MapFileSuffix, Opus.Core.Location.EExists.WillExist);
                }
            }

#else
            var target = node.Target;
            var linkerTool = target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;
            var options = this as ILinkerOptions;

            if (!this.OutputPaths.Has(C.OutputFileFlags.Executable))
            {
                var outputPrefix = string.Empty;
                var outputSuffix = string.Empty;
                if (options.OutputType == ELinkerOutput.Executable)
                {
                    outputSuffix = linkerTool.ExecutableSuffix;
                }
                else if (options.OutputType == ELinkerOutput.DynamicLibrary)
                {
                    outputPrefix = linkerTool.DynamicLibraryPrefix;
                    outputSuffix = linkerTool.DynamicLibrarySuffix;
                }

                var baseOutputPath = this.OutputDirectoryPath;
                var osxLinkerOptions = this as ILinkerOptionsOSX;
                if (osxLinkerOptions != null)
                {
                    // TODO: define more output paths for the application wrapper (.app), the Contents folder etc
                    if (target.HasPlatform(Opus.Core.EPlatform.OSX) && osxLinkerOptions.ApplicationBundle)
                    {
                        baseOutputPath = System.IO.Path.Combine(baseOutputPath, this.OutputName + ".app");
                        this.OutputPaths[OutputFileFlags.OSXBundle] = baseOutputPath;
                        baseOutputPath = System.IO.Path.Combine(baseOutputPath, "Contents");
                        this.OutputPaths[OutputFileFlags.OSXBundleContents] = baseOutputPath;
                        baseOutputPath = System.IO.Path.Combine(baseOutputPath, "MacOS");
                        this.OutputPaths[OutputFileFlags.OSXBundleMacOS] = baseOutputPath;
                    }
                }

                var outputPathName = System.IO.Path.Combine(baseOutputPath, outputPrefix + this.OutputName) + outputSuffix;
                this.OutputFilePath = outputPathName;
            }

            if (options.OutputType == ELinkerOutput.DynamicLibrary)
            {
                if (linkerTool is IWinImportLibrary)
                {
                    // explicit import library
                    var importLibrary = linkerTool as IWinImportLibrary;
                    var importLibraryPathName = System.IO.Path.Combine(this.LibraryDirectoryPath, importLibrary.ImportLibraryPrefix + this.OutputName) + importLibrary.ImportLibrarySuffix;
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
                var mapPathName = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + linkerTool.MapFileSuffix;
                this.MapFilePath = mapPathName;
            }

            base.FinalizeOptions(node);
#endif
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}