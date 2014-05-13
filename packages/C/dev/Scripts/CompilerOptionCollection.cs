// <copyright file="CompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class CompilerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            var compilerOptions = this as ICCompilerOptions;

            var target = node.Target;

            // process character set early, as it sets #defines
            compilerOptions.CharacterSet = ECharacterSet.NotSet;

            compilerOptions.OutputType = ECompilerOutput.CompileOnly;
            compilerOptions.WarningsAsErrors = true;
            compilerOptions.IgnoreStandardIncludePaths = true;
            compilerOptions.TargetLanguage = ETargetLanguage.Default;

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerOptions.DebugSymbols = true;
                compilerOptions.Optimization = EOptimization.Off;
                compilerOptions.OmitFramePointer = false;
            }
            else
            {
                if (!target.HasConfiguration(Opus.Core.EConfiguration.Profile))
                {
                    compilerOptions.DebugSymbols = false;
                }
                else
                {
                    compilerOptions.DebugSymbols = true;
                }
                compilerOptions.Optimization = EOptimization.Speed;
                compilerOptions.OmitFramePointer = true;
            }
            compilerOptions.CustomOptimization = "";
            compilerOptions.ShowIncludes = false;

            compilerOptions.Defines = new DefineCollection();
            compilerOptions.Undefines = new DefineCollection();

            // TODO: deprecate this in 0.60
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_{0}", ((Opus.Core.BaseTarget)target).PlatformName('u')));

            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_WINDOWS"));
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_UNIX"));
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_OSX"));
            }

            {
                bool is64bit = Opus.Core.OSUtilities.Is64Bit(target);
                int bits = (is64bit) ? 64 : 32;
                compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_BITS={0}", bits.ToString()));
            }
            {
                bool isLittleEndian = Opus.Core.State.IsLittleEndian;
                if (isLittleEndian)
                {
                    compilerOptions.Defines.Add("D_OPUS_PLATFORM_LITTLEENDIAN");
                }
                else
                {
                    compilerOptions.Defines.Add("D_OPUS_PLATFORM_BIGENDIAN");
                }
            }
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_CONFIGURATION_{0}", ((Opus.Core.BaseTarget)target).ConfigurationName('u')));
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_TOOLCHAIN_{0}", target.ToolsetName('u')));

            compilerOptions.IncludePaths = new Opus.Core.DirectoryCollection();
            compilerOptions.IncludePaths.Add("."); // explicitly add the one that is assumed

            compilerOptions.SystemIncludePaths = new Opus.Core.DirectoryCollection();

            compilerOptions.DisableWarnings = new Opus.Core.StringArray();

            if (this is ICxxCompilerOptions)
            {
                compilerOptions.LanguageStandard = ELanguageStandard.Cxx98;
            }
            else
            {
                compilerOptions.LanguageStandard = ELanguageStandard.C89;
            }

            compilerOptions.AdditionalOptions = "";
        }

        public CompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            var locationMap = this.OwningNode.Module.Locations;
            var moduleBuildDir = locationMap[Opus.Core.State.ModuleBuildDirLocationKey];

            var outputFileDir = locationMap[C.ObjectFile.ObjectFileDirLocationKey];
            if (!outputFileDir.IsValid)
            {
                var target = node.Target;
                var compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                var objBuildDir = moduleBuildDir.SubDirectory(compilerTool.ObjectFileOutputSubDirectory);
                (outputFileDir as Opus.Core.ScaffoldLocation).SetReference(objBuildDir);
            }

            // don't operate on collections of modules
            var objectFileModule = node.Module as ObjectFile;
            if (null != objectFileModule)
            {
                // this only requires the end path - so grab it from the Location without resolving it
                var location = objectFileModule.SourceFileLocation;
                var sourcePathName = string.Empty;
                if (location is Opus.Core.FileLocation)
                {
                    sourcePathName = location.AbsolutePath;
                }
                else if (location is Opus.Core.DirectoryLocation)
                {
                    throw new Opus.Core.Exception("Cannot use a directory for compiler options");
                }
                else
                {
                    sourcePathName = (location as Opus.Core.ScaffoldLocation).Pattern;
                }
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }
            else
            {
                this.OutputName = null;
            }
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
        public string ObjectFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.ObjectFile];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.ObjectFile] = value;
            }
        }
#endif

#if true
#else
        public string PreprocessedFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.PreprocessedFile];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.PreprocessedFile] = value;
            }
        }
#endif

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
#if true
            // don't operate on collections of modules
            var objectFileModule = node.Module as ObjectFile;
            if (null != objectFileModule)
            {
                var outputFileLocation = node.Module.Locations[C.ObjectFile.ObjectFileLocationKey] as Opus.Core.ScaffoldLocation;
                if (!outputFileLocation.IsValid)
                {
                    var target = node.Target;
                    var tool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                    var options = this as ICCompilerOptions;
                    var suffix = (options.OutputType == ECompilerOutput.Preprocess) ?
                        tool.PreprocessedOutputSuffix :
                        tool.ObjectFileSuffix;
                    outputFileLocation.SpecifyStub(node.Module.Locations[C.ObjectFile.ObjectFileDirLocationKey], this.OutputName + suffix, Opus.Core.Location.EExists.WillExist);
                }
            }
#else
            if (null != this.OutputName)
            {
                var target = node.Target;
                var compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                var options = this as ICCompilerOptions;
                if ((options.OutputType == ECompilerOutput.CompileOnly) && !this.OutputPaths.Has(C.OutputFileFlags.ObjectFile))
                {
                    var objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + compilerTool.ObjectFileSuffix;
                    this.ObjectFilePath = objectPathname;
                }
                else if ((options.OutputType == ECompilerOutput.Preprocess) && !this.OutputPaths.Has(C.OutputFileFlags.PreprocessedFile))
                {
                    var preprocessedPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + compilerTool.PreprocessedOutputSuffix;
                    this.PreprocessedFilePath = preprocessedPathname;
                }
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