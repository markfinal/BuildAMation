// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions, ICCompilerOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common compiler options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(ToolchainOptionCollectionCommandLine, ToolchainOptionCollectionVisualStudio);
            this["SystemIncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLine, IncludePathsVisualStudio);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLine, IncludePathsVisualStudio);
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLine, DefinesVisualStudio);
            this["DebugSymbols"].PrivateData = new PrivateData(null, null);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine, OutputTypeVisualStudio);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLine, OptimizationVisualStudio);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLine, CustomOptimizationVisualStudio);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLine, WarningsAsErrorsVisualStudio);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLine, IgnoreStandardIncludePathsVisualStudio);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLine, TargetLanguageVisualStudio);
            this["DebugType"].PrivateData = new PrivateData(DebugTypeCommandLine, DebugTypeVisualStudio);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLine, ShowIncludesVisualStudio);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLine, AdditionalOptionsVisualStudio);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLine, OmitFramePointerVisualStudio);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLine, DisableWarningsVisualStudio);

            // compiler specific options
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine, NoLogoVisualStudio);
            this["MinimalRebuild"].PrivateData = new PrivateData(MinimalRebuildCommandLine, MinimalRebuildVisualStudio);
            this["WarningLevel"].PrivateData = new PrivateData(WarningLevelCommandLine, WarningLevelVisualStudio);
            this["BrowseInformation"].PrivateData = new PrivateData(BrowseInformationCommandLine, BrowseInformationVisualStudio);
            this["StringPooling"].PrivateData = new PrivateData(StringPoolingCommandLine, StringPoolingVisualStudio);
            this["DisableLanguageExtensions"].PrivateData = new PrivateData(DisableLanguageExtensionsCommandLine, DisableLanguageExtensionsVisualStudio);
            this["ForceConformanceInForLoopScope"].PrivateData = new PrivateData(ForceConformanceInForLoopScopeCommandLine, ForceConformanceInForLoopScopeVisualStudio);
            this["UseFullPaths"].PrivateData = new PrivateData(UseFullPathsCommandLine, UseFullPathsVisualStudio);
            this["CompileAsManaged"].PrivateData = new PrivateData(CompileAsManagedCommandLine, CompileAsManagedVisualStudio);
            this["BasicRuntimeChecks"].PrivateData = new PrivateData(BasicRuntimeChecksCommandLine, BasicRuntimeChecksVisualStudio);
            this["SmallerTypeConversionRuntimeCheck"].PrivateData = new PrivateData(SmallerTypeConversionRuntimeCheckCommandLine, SmallerTypeConversionRuntimeCheckVisualStudio);
            this["InlineFunctionExpansion"].PrivateData = new PrivateData(InlineFunctionExpansionCommandLine, InlineFunctionExpansionVisualStudio);
            this["EnableIntrinsicFunctions"].PrivateData = new PrivateData(EnableIntrinsicFunctionsCommandLine, EnableIntrinsicFunctionsVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;

            this.NoLogo = true;

            if (Opus.Core.EConfiguration.Debug == target.Configuration)
            {
                this.MinimalRebuild = true;
                this.BasicRuntimeChecks = EBasicRuntimeChecks.StackFrameAndUninitializedVariables;
                this.SmallerTypeConversionRuntimeCheck = true;
                this.InlineFunctionExpansion = EInlineFunctionExpansion.None;
                this.OmitFramePointer = false;
                this.EnableIntrinsicFunctions = false;
            }
            else
            {
                this.MinimalRebuild = false;
                this.BasicRuntimeChecks = EBasicRuntimeChecks.None;
                this.SmallerTypeConversionRuntimeCheck = false;
                this.InlineFunctionExpansion = EInlineFunctionExpansion.AnySuitable;
                this.OmitFramePointer = true;
                this.EnableIntrinsicFunctions = true;
            }

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;
            this.SystemIncludePaths.AddRange(null, compilerInstance.IncludeDirectoryPaths(target));

            this.TargetLanguage = C.ETargetLanguage.C;

            this.WarningLevel = EWarningLevel.Level4;

            this.DebugType = EDebugType.Embedded;

            // disable browse information to improve build speed
            this.BrowseInformation = EBrowseInformation.None;

            this.StringPooling = true;
            this.DisableLanguageExtensions = false;
            this.ForceConformanceInForLoopScope = true;
            this.UseFullPaths = true;
            this.CompileAsManaged = EManagedCompilation.NoCLR;
        }

        public CCompilerOptionCollection()
            : base()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            base.SetNodeOwnership(node);

            this.ProgramDatabaseName = node.ModuleName;
        }

        public string ProgramDatabaseName
        {
            get;
            set;
        }

        public override string ObjectFilePath
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

        public override string PreprocessedFilePath
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

        public string ProgramDatabaseFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.CompilerProgramDatabase];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.CompilerProgramDatabase] = value;
            }
        }

        protected static void ToolchainOptionCollectionSetHandler(object sender, Opus.Core.Option option)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            toolchainOptions.Value.CCompilerOptionsInterface = sender as C.ICCompilerOptions;
        }

        private static void ToolchainOptionCollectionCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            CommandLineProcessor.ICommandLineSupport commandLineSupport = toolchainOptions.Value as CommandLineProcessor.ICommandLineSupport;
            commandLineSupport.ToCommandLineArguments(commandLineBuilder, target);
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ToolchainOptionCollectionVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            VisualStudioProcessor.IVisualStudioSupport visualStudioSupport = toolchainOptions.Value as VisualStudioProcessor.IVisualStudioSupport;
            return visualStudioSupport.ToVisualStudioProjectAttributes(target);
        }

        private static void IncludeSystemPathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            if (!optionCollection.IgnoreStandardIncludePaths)
            {
                Opus.Core.Log.Full("System include paths not explicitly added to the build");
                return;
            }

            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            string switchPrefix = compilerInstance.IncludePathCompilerSwitches[0];

            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IncludeSystemPathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();

            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            if (!optionCollection.IgnoreStandardIncludePaths)
            {
                Opus.Core.Log.Full("System include paths not explicitly added to the build");
                return dictionary;
            }

            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            System.Text.StringBuilder includePaths = new System.Text.StringBuilder();
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    includePaths.AppendFormat("\"{0}\";", includePath);
                }
                else
                {
                    includePaths.AppendFormat("{0};", includePath);
                }
            }
            dictionary.Add("AdditionalIncludeDirectories", includePaths.ToString());
            return dictionary;
        }

        private static void IncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            string switchPrefix = compilerInstance.IncludePathCompilerSwitches[0];

            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IncludePathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            System.Text.StringBuilder includePaths = new System.Text.StringBuilder();
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    includePaths.AppendFormat("\"{0}\";", includePath);
                }
                else
                {
                    includePaths.AppendFormat("{0};", includePath);
                }
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalIncludeDirectories", includePaths.ToString());
            return dictionary;
        }

        private static void DefinesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string define in definesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("/D{0}", define));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DefinesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            System.Text.StringBuilder defines = new System.Text.StringBuilder();
            foreach (string define in definesOption.Value)
            {
                defines.AppendFormat("{0};", define);
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("PreprocessorDefinitions", defines.ToString());
            return dictionary;
        }

        protected static void OutputTypeSetHandler(object sender, Opus.Core.Option option)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (null == options.OutputName)
            {
                options.ObjectFilePath = null;
                return;
            }

            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    {
                        string objectPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".obj";
                        options.ObjectFilePath = objectPathname;
                    }
                    break;

                case C.ECompilerOutput.Preprocess:
                    {
                        string preprocessedPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".i";
                        options.PreprocessedFilePath = preprocessedPathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ECompilerOutput");
            }
        }

        private static void OutputTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (null == options.ObjectFilePath)
            {
                return;
            }
            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLineBuilder.Add("/c");
                    if (options.ObjectFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/Fo\"{0}\"", options.ObjectFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/Fo{0}", options.ObjectFilePath));
                    }
                    break;

                case C.ECompilerOutput.Preprocess: // with line numbers
                    commandLineBuilder.Add("/P");
                    if (options.ObjectFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/Fo\"{0}\"", options.ObjectFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/Fo{0}", options.ObjectFilePath));
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.ECompilerOutput> processOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (null == options.ObjectFilePath)
            {
                return null;
            }
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (processOption.Value)
                {
                    case C.ECompilerOutput.CompileOnly:
                    case C.ECompilerOutput.Preprocess:
                        {
                            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                            dictionary.Add("GeneratePreprocessedFile", processOption.Value.ToString("D"));
                            dictionary.Add("ObjectFile", options.ObjectFilePath);
                            return dictionary;
                        }

                    default:
                        throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (processOption.Value)
                {
                    case C.ECompilerOutput.CompileOnly:
                        {
                            dictionary.Add("PreprocessToFile", "false");
                            dictionary.Add("ObjectFileName", options.ObjectFilePath);
                        }
                        break;

                    case C.ECompilerOutput.Preprocess:
                        {
                            dictionary.Add("PreprocessToFile", "true");
                            dictionary.Add("ObjectFileName", options.ObjectFilePath);
                        }
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
                }
                return dictionary;
            }
            return null;
        }

        private static void OptimizationCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EOptimization> optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            switch (optimizationOption.Value)
            {
                case C.EOptimization.Off:
                    commandLineBuilder.Add("/Od");
                    break;

                case C.EOptimization.Size:
                    commandLineBuilder.Add("/Os");
                    break;

                case C.EOptimization.Speed:
                    commandLineBuilder.Add("/O1");
                    break;

                case C.EOptimization.Full:
                    commandLineBuilder.Add("/Ox");
                    break;

                case C.EOptimization.Custom:
                    // do nothing
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized optimization option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OptimizationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.EOptimization> optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (optimizationOption.Value)
                {
                    case C.EOptimization.Off:
                    case C.EOptimization.Size:
                    case C.EOptimization.Speed:
                    case C.EOptimization.Full:
                    case C.EOptimization.Custom:
                        {
                            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                            dictionary.Add("Optimization", optimizationOption.Value.ToString("D"));
                            return dictionary;
                        }

                    default:
                        throw new Opus.Core.Exception("Unrecognized optimization option");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (optimizationOption.Value)
                {
                    case C.EOptimization.Off:
                        dictionary.Add("Optimization", "Disabled");
                        break;

                    case C.EOptimization.Size:
                        dictionary.Add("Optimization", "MinSpace");
                        break;

                    case C.EOptimization.Speed:
                        dictionary.Add("Optimization", "MaxSpeed");
                        break;

                    case C.EOptimization.Full:
                        dictionary.Add("Optimization", "Full");
                        break;

                    case C.EOptimization.Custom:
                        // TODO: does this need something?
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized optimization option");
                }
                return dictionary;
            }
            return null;
        }

        private static void CustomOptimizationCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (!System.String.IsNullOrEmpty(customOptimizationOption.Value))
            {
                commandLineBuilder.Add(customOptimizationOption.Value);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary CustomOptimizationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            // TODO;
            //Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            return null;
        }

        private static void WarningsAsErrorsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Add("/WX");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary WarningsAsErrorsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("WarnAsError", warningsAsErrorsOption.Value.ToString().ToLower());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                dictionary.Add("TreatWarningAsError", warningsAsErrorsOption.Value.ToString().ToLower());
            }
            return dictionary;
        }

        private static void IgnoreStandardIncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> includeStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (includeStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Add("/X");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IgnoreStandardIncludePathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> includeStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("IgnoreStandardIncludePath", includeStandardIncludePathsOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void TargetLanguageCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ETargetLanguage> targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            switch (targetLanguageOption.Value)
            {
                case C.ETargetLanguage.Default:
                    // do nothing
                    break;

                case C.ETargetLanguage.C:
                    commandLineBuilder.Add("/TC");
                    break;

                case C.ETargetLanguage.CPlusPlus:
                    commandLineBuilder.Add("/TP");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized target language option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary TargetLanguageVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.ETargetLanguage> targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (targetLanguageOption.Value)
                {
                    case C.ETargetLanguage.Default:
                    case C.ETargetLanguage.C:
                    case C.ETargetLanguage.CPlusPlus:
                        {
                            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                            dictionary.Add("CompileAs", targetLanguageOption.Value.ToString("D"));
                            return dictionary;
                        }

                    default:
                        throw new Opus.Core.Exception("Unrecognized target language option");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (targetLanguageOption.Value)
                {
                    case C.ETargetLanguage.Default:
                        dictionary.Add("CompileAs", "Default");
                        break;

                    case C.ETargetLanguage.C:
                        dictionary.Add("CompileAs", "CompileAsC");
                        break;

                    case C.ETargetLanguage.CPlusPlus:
                        dictionary.Add("CompileAs", "CompileAsCpp");
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized target language option");
                }
                return dictionary;
            }
            return null;
        }

        private static void NoLogoCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("/nologo");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void MinimalRebuildCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            Opus.Core.ValueTypeOption<bool> minimalRebuildOption = option as Opus.Core.ValueTypeOption<bool>;
            if (minimalRebuildOption.Value &&
                optionCollection.DebugSymbols &&
                (EManagedCompilation.NoCLR == optionCollection.CompileAsManaged) &&
                ((EDebugType.ProgramDatabase == optionCollection.DebugType) || (EDebugType.ProgramDatabaseEditAndContinue == optionCollection.DebugType)))
            {
                commandLineBuilder.Add("/Gm");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary MinimalRebuildVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            Opus.Core.ValueTypeOption<bool> minimalRebuildOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            string attributeName = "MinimalRebuild";
            if (minimalRebuildOption.Value &&
                optionCollection.DebugSymbols &&
                (EManagedCompilation.NoCLR == optionCollection.CompileAsManaged) &&
                ((EDebugType.ProgramDatabase == optionCollection.DebugType) || (EDebugType.ProgramDatabaseEditAndContinue == optionCollection.DebugType)))
            {
                dictionary.Add(attributeName, "true");
            }
            else
            {
                dictionary.Add(attributeName, "false");
            }
            return dictionary;
        }

        private static void WarningLevelCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            commandLineBuilder.Add(System.String.Format("/W{0}", (int)enumOption.Value));
        }

        private static VisualStudioProcessor.ToolAttributeDictionary WarningLevelVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("WarningLevel", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                dictionary.Add("WarningLevel", System.String.Format("Level{0}", enumOption.Value.ToString("D")));
            }
            return dictionary;
        }

        private static void BrowseInformationCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.CompilerOptionCollection options = sender as C.CompilerOptionCollection;
            Opus.Core.ValueTypeOption<EBrowseInformation> enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            string browseDir = options.OutputDirectoryPath;
            switch (enumOption.Value)
            {
                case EBrowseInformation.None:
                    // do nothing
                    break;

                case EBrowseInformation.Full:
                    if (browseDir.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/FR\"{0}\"", browseDir));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/FR{0}", browseDir));
                    }
                    break;

                case EBrowseInformation.NoLocalSymbols:
                    if (browseDir.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/Fr\"{0}\"", browseDir));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/Fr{0}", browseDir));
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized EBrowseInformation option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary BrowseInformationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<EBrowseInformation> enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            C.CompilerOptionCollection options = sender as C.CompilerOptionCollection;
            // the trailing directory separator is important, or unexpected rebuilds occur
            string browseDir = options.OutputDirectoryPath +"\\";
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("BrowseInformation", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBrowseInformation.None:
                        dictionary.Add("BrowseInformation", "false");
                        break;

                    // TODO: there does not appear to be a different set of values in MSBUILD
                    case EBrowseInformation.Full:
                    case EBrowseInformation.NoLocalSymbols:
                        dictionary.Add("BrowseInformation", "true");
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized EBrowseInformation option");
                }
            }
            dictionary.Add("BrowseInformationFile", browseDir);
            return dictionary;
        }

        private static void StringPoolingCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/GF");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary StringPoolingVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("StringPooling", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void DisableLanguageExtensionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/Za");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DisableLanguageExtensionsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("DisableLanguageExtensions", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void ForceConformanceInForLoopScopeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/Zc:forScope");
            }
            else
            {
                commandLineBuilder.Add("/Zc:forScope-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ForceConformanceInForLoopScopeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("ForceConformanceInForLoopScope", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void ShowIncludesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/showIncludes");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ShowIncludesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("ShowIncludes", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void UseFullPathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/FC");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary UseFullPathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("UseFullPaths", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void CompileAsManagedCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EManagedCompilation> enumOption = option as Opus.Core.ValueTypeOption<EManagedCompilation>;
            switch (enumOption.Value)
            {
                case EManagedCompilation.NoCLR:
                    break;

                case EManagedCompilation.CLR:
                    commandLineBuilder.Add("/clr");
                    break;

                case EManagedCompilation.PureCLR:
                    commandLineBuilder.Add("/clr:pure");
                    break;

                case EManagedCompilation.SafeCLR:
                    commandLineBuilder.Add("/clr:safe");
                    break;

                case EManagedCompilation.OldSyntaxCLR:
                    commandLineBuilder.Add("/clr:oldsyntax");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized EManagedCompilation option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary CompileAsManagedVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<EManagedCompilation> enumOption = option as Opus.Core.ValueTypeOption<EManagedCompilation>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("CompileAsManaged", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EManagedCompilation.NoCLR:
                        dictionary.Add("CompileAsManaged", "false");
                        break;

                    case EManagedCompilation.CLR:
                        dictionary.Add("CompileAsManaged", "true");
                        break;

                    case EManagedCompilation.PureCLR:
                        dictionary.Add("CompileAsManaged", "Pure");
                        break;

                    case EManagedCompilation.SafeCLR:
                        dictionary.Add("CompileAsManaged", "Safe");
                        break;

                    case EManagedCompilation.OldSyntaxCLR:
                        dictionary.Add("CompileAsManaged", "OldSyntax");
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized EManagedCompilation option");
                }
            }
            return dictionary;
        }

        protected static void DebugTypeSetHandler(object sender, Opus.Core.Option option)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (options.DebugSymbols)
            {
                Opus.Core.ValueTypeOption<EDebugType> enumOption = option as Opus.Core.ValueTypeOption<EDebugType>;
                switch (options.DebugType)
                {
                    case EDebugType.Embedded:
                        options.ProgramDatabaseFilePath = null;
                        break;

                    case EDebugType.ProgramDatabase:
                    case EDebugType.ProgramDatabaseEditAndContinue:
                        {
                            string pdbPathName = System.IO.Path.Combine(options.OutputDirectoryPath, options.ProgramDatabaseName) + ".pdb";
                            options.ProgramDatabaseFilePath = pdbPathName;
                        }
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
            else
            {
                options.ProgramDatabaseFilePath = null;
            }
        }

        private static void DebugTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (options.DebugSymbols)
            {
                Opus.Core.ValueTypeOption<EDebugType> enumOption = option as Opus.Core.ValueTypeOption<EDebugType>;
                switch (options.DebugType)
                {
                    case EDebugType.Embedded:
                        commandLineBuilder.Add("/Z7");
                        break;

                    case EDebugType.ProgramDatabase:
                        {
                            commandLineBuilder.Add("/Zi");

                            if (null == options.ProgramDatabaseFilePath)
                            {
                                throw new Opus.Core.Exception("PDB file path has not been set");
                            }
                            if (options.ProgramDatabaseFilePath.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("/Fd\"{0}\"", options.ProgramDatabaseFilePath));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("/Fd{0}", options.ProgramDatabaseFilePath));
                            }
                        }
                        break;

                    case EDebugType.ProgramDatabaseEditAndContinue:
                        {
                            commandLineBuilder.Add("/ZI");

                            if (null == options.ProgramDatabaseFilePath)
                            {
                                throw new Opus.Core.Exception("PDB file path has not been set");
                            }
                            if (options.ProgramDatabaseFilePath.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("/Fd\"{0}\"", options.ProgramDatabaseFilePath));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("/Fd{0}", options.ProgramDatabaseFilePath));
                            }
                        }
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DebugTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            string attributeName = "DebugInformationFormat";
            if (options.DebugSymbols)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                Opus.Core.ValueTypeOption<EDebugType> enumOption = option as Opus.Core.ValueTypeOption<EDebugType>;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                {
                    switch (options.DebugType)
                    {
                        case EDebugType.Embedded:
                            dictionary.Add(attributeName, options.DebugType.ToString("D"));
                            break;

                        case EDebugType.ProgramDatabase:
                        case EDebugType.ProgramDatabaseEditAndContinue:
                            dictionary.Add(attributeName, options.DebugType.ToString("D"));
                            dictionary.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                            break;

                        default:
                            throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                    }
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                {
                    switch (options.DebugType)
                    {
                        case EDebugType.Embedded:
                            dictionary.Add("DebugInformationFormat", "OldStyle");
                            break;

                        case EDebugType.ProgramDatabase:
                            dictionary.Add("DebugInformationFormat", "ProgramDatabase");
                            dictionary.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                            break;

                        case EDebugType.ProgramDatabaseEditAndContinue:
                            dictionary.Add("DebugInformationFormat", "EditAndContinue");
                            dictionary.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                            break;

                        default:
                            throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                    }
                }
                return dictionary;
            }
            else
            {
                return null;
            }
        }

        private static void BasicRuntimeChecksCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return;
            }

            Opus.Core.ValueTypeOption<EBasicRuntimeChecks> enumOption = option as Opus.Core.ValueTypeOption<EBasicRuntimeChecks>;
            switch (enumOption.Value)
            {
                case EBasicRuntimeChecks.None:
                    break;

                case EBasicRuntimeChecks.StackFrame:
                    commandLineBuilder.Add("/RTCs");
                    break;

                case EBasicRuntimeChecks.UninitializedVariables:
                    commandLineBuilder.Add("/RTCu");
                    break;

                case EBasicRuntimeChecks.StackFrameAndUninitializedVariables:
                    commandLineBuilder.Add("/RTC1");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for VisualC.EBasicRuntimeChecks");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary BasicRuntimeChecksVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();

            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return dictionary;
            }

            Opus.Core.ValueTypeOption<EBasicRuntimeChecks> enumOption = option as Opus.Core.ValueTypeOption<EBasicRuntimeChecks>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBasicRuntimeChecks.None:
                    case EBasicRuntimeChecks.StackFrame:
                    case EBasicRuntimeChecks.UninitializedVariables:
                    case EBasicRuntimeChecks.StackFrameAndUninitializedVariables:
                        dictionary.Add("BasicRuntimeChecks", enumOption.Value.ToString("D"));
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EBasicRuntimeChecks");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBasicRuntimeChecks.None:
                        dictionary.Add("BasicRuntimeChecks", "Default");
                        break;

                    case EBasicRuntimeChecks.StackFrame:
                        dictionary.Add("BasicRuntimeChecks", "StackFrameRuntimeCheck");
                        break;

                    case EBasicRuntimeChecks.UninitializedVariables:
                        dictionary.Add("BasicRuntimeChecks", "UninitializedLocalUsageCheck");
                        break;

                    case EBasicRuntimeChecks.StackFrameAndUninitializedVariables:
                        dictionary.Add("BasicRuntimeChecks", "EnableFastChecks");
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EBasicRuntimeChecks");
                }
            }
            return dictionary;
        }

        private static void SmallerTypeConversionRuntimeCheckCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return;
            }

            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/RTCc");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary SmallerTypeConversionRuntimeCheckVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();

            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return dictionary;
            }

            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("SmallerTypeCheck", boolOption.Value.ToString());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                dictionary.Add("SmallerTypeCheck", boolOption.Value.ToString());
            }
            return dictionary;
        }

        private static void AdditionalOptionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (!System.String.IsNullOrEmpty(stringOption.Value))
            {
                dictionary.Add("AdditionalOptions", stringOption.Value);
            }
            return dictionary;
        }

        private static void InlineFunctionExpansionCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EInlineFunctionExpansion> enumOption = option as Opus.Core.ValueTypeOption<EInlineFunctionExpansion>;
            switch (enumOption.Value)
            {
                case EInlineFunctionExpansion.None:
                    commandLineBuilder.Add("/Ob0");
                    break;

                case EInlineFunctionExpansion.OnlyInline:
                    commandLineBuilder.Add("/Ob1");
                    break;

                case EInlineFunctionExpansion.AnySuitable:
                    commandLineBuilder.Add("/Ob2");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for VisualC.EInlineFunctionExpansion");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary InlineFunctionExpansionVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<EInlineFunctionExpansion> enumOption = option as Opus.Core.ValueTypeOption<EInlineFunctionExpansion>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EInlineFunctionExpansion.None:
                    case EInlineFunctionExpansion.OnlyInline:
                    case EInlineFunctionExpansion.AnySuitable:
                        dictionary.Add("InlineFunctionExpansion", enumOption.Value.ToString("D"));
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EInlineFunctionExpansion");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EInlineFunctionExpansion.None:
                        dictionary.Add("InlineFunctionExpansion", "Disabled");
                        break;

                    case EInlineFunctionExpansion.OnlyInline:
                        dictionary.Add("InlineFunctionExpansion", "OnlyExplicitInline");
                        break;

                    case EInlineFunctionExpansion.AnySuitable:
                        dictionary.Add("InlineFunctionExpansion", "AnySuitable");
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EInlineFunctionExpansion");
                }
            }
            return dictionary;
        }

        private static void OmitFramePointerCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/Oy");
            }
            else
            {
                commandLineBuilder.Add("/Oy-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OmitFramePointerVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("OmitFramePointers", boolOption.Value.ToString());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                dictionary.Add("OmitFramePointers", boolOption.Value.ToString());
            }
            return dictionary;
        }

        private static void DisableWarningsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string warning in disableWarningsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("/wd{0}", warning));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DisableWarningsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            System.Text.StringBuilder disableWarnings = new System.Text.StringBuilder();
            foreach (string warning in disableWarningsOption.Value)
            {
                disableWarnings.AppendFormat("{0};", warning);
            }
            dictionary.Add("DisableSpecificWarnings", disableWarnings.ToString());
            return dictionary;
        }

        private static void EnableIntrinsicFunctionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/Oi");
            }
            else
            {
                commandLineBuilder.Add("/Oi-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary EnableIntrinsicFunctionsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("EnableIntrinsicFunctions", boolOption.Value.ToString());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                dictionary.Add("IntrinsicFunctions", boolOption.Value.ToString());
            }
            return dictionary;
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.ObjectFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.ObjectFilePath), false);
            }
            if (null != this.ProgramDatabaseFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.ProgramDatabaseFilePath), false);
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualCCommon.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target) as VisualCCommon.Toolchain;
            VisualStudioProcessor.EVisualStudioTarget vsTarget = toolchain.VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception(System.String.Format("Unsupported VisualStudio target, '{0}'", vsTarget));
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}