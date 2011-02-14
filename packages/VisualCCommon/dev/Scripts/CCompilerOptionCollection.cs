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
        private void SetDelegates(Opus.Core.Target target)
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
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;

            this.NoLogo = true;

            if (Opus.Core.EConfiguration.Debug == target.Configuration)
            {
                this.MinimalRebuild = true;
            }
            else
            {
                this.MinimalRebuild = false;
            }

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;
            this.SystemIncludePaths.AddRange(null, compilerInstance.IncludeDirectoryPaths(target));

            this.TargetLanguage = C.ETargetLanguage.C;

            this.WarningLevel = EWarningLevel.Level4;

            this.DebugType = EDebugType.Embedded;

            // TODO: can this be done via an option delegate from the VSSolutionBuilder?
            if (Opus.Core.State.BuilderName == "VSSolution")
            {
                this.BrowseInformation = EBrowseInformation.Full;
            }
            else
            {
                this.BrowseInformation = EBrowseInformation.None;
            }

            this.StringPooling = true;
            this.DisableLanguageExtensions = false;
            this.ForceConformanceInForLoopScope = true;
            this.UseFullPaths = true;
            this.CompileAsManaged = EManagedCompilation.NoCLR;

            this.SetDelegates(target);
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

        private static void ToolchainOptionCollectionCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            CommandLineProcessor.ICommandLineSupport commandLineSupport = toolchainOptions.Value as CommandLineProcessor.ICommandLineSupport;
            commandLineSupport.ToCommandLineArguments(commandLineBuilder, target);
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ToolchainOptionCollectionVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            VisualStudioProcessor.IVisualStudioSupport visualStudioSupport = toolchainOptions.Value as VisualStudioProcessor.IVisualStudioSupport;
            return visualStudioSupport.ToVisualStudioProjectAttributes(target);
        }

        private static void IncludePathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                commandLineBuilder.AppendFormat("/I\"{0}\" ", includePath);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IncludePathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            System.Text.StringBuilder includePaths = new System.Text.StringBuilder();
            foreach (string includePath in includePathsOption.Value)
            {
                includePaths.AppendFormat("\"{0}\";", includePath);
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalIncludeDirectories", includePaths.ToString());
            return dictionary;
        }

        private static void DefinesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string define in definesOption.Value)
            {
                commandLineBuilder.AppendFormat("/D{0} ", define);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DefinesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
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

        private static void OutputTypeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLineBuilder.AppendFormat("/c /Fo\"{0}\" ", options.ObjectFilePath);
                    break;

                case C.ECompilerOutput.Preprocess: // with line numbers
                    commandLineBuilder.AppendFormat("/P /Fo\"{0}\" ", options.ObjectFilePath);
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ECompilerOutput> processOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (processOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                case C.ECompilerOutput.Preprocess:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        dictionary.Add("GeneratePreprocessedFile", processOption.Value.ToString("D"));
                        CCompilerOptionCollection options = sender as CCompilerOptionCollection;
                        dictionary.Add("ObjectFile", options.ObjectFilePath);
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
        }

        private static void OptimizationCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EOptimization> optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            switch (optimizationOption.Value)
            {
                case C.EOptimization.Off:
                    commandLineBuilder.Append("/Od ");
                    break;

                case C.EOptimization.Size:
                    commandLineBuilder.Append("/Os ");
                    break;

                case C.EOptimization.Speed:
                    commandLineBuilder.Append("/O1 ");
                    break;

                case C.EOptimization.Full:
                    commandLineBuilder.Append("/Ox ");
                    break;

                case C.EOptimization.Custom:
                    // do nothing
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized optimization option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OptimizationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EOptimization> optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
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

        private static void CustomOptimizationCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.AppendFormat("{0} ", customOptimizationOption.Value);
        }

        private static VisualStudioProcessor.ToolAttributeDictionary CustomOptimizationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            // TODO;
            //Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            return null;
        }

        private static void WarningsAsErrorsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Append("/WX ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary WarningsAsErrorsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("WarnAsError", warningsAsErrorsOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void IgnoreStandardIncludePathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> includeStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (includeStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Append("/X ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IgnoreStandardIncludePathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> includeStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("IgnoreStandardIncludePath", includeStandardIncludePathsOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void TargetLanguageCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ETargetLanguage> targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            switch (targetLanguageOption.Value)
            {
                case C.ETargetLanguage.Default:
                    // do nothing
                    break;

                case C.ETargetLanguage.C:
                    commandLineBuilder.Append("/TC ");
                    break;

                case C.ETargetLanguage.CPlusPlus:
                    commandLineBuilder.Append("/TP ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized target language option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary TargetLanguageVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ETargetLanguage> targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
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

        private static void NoLogoCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Append("/nologo ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void MinimalRebuildCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection optionCollection = sender as CCompilerOptionCollection;
            Opus.Core.ValueTypeOption<bool> minimalRebuildOption = option as Opus.Core.ValueTypeOption<bool>;
            if (minimalRebuildOption.Value &&
                optionCollection.DebugSymbols &&
                (EManagedCompilation.NoCLR == optionCollection.CompileAsManaged) &&
                ((EDebugType.ProgramDatabase == optionCollection.DebugType) || (EDebugType.ProgramDatabaseEditAndContinue == optionCollection.DebugType)))
            {
                commandLineBuilder.Append("/Gm ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary MinimalRebuildVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
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

        private static void WarningLevelCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            commandLineBuilder.AppendFormat("/W{0} ", (int)enumOption.Value);
        }

        private static VisualStudioProcessor.ToolAttributeDictionary WarningLevelVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("WarningLevel", enumOption.Value.ToString("D"));
            return dictionary;
        }

        private static void BrowseInformationCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.CompilerOptionCollection options = sender as C.CompilerOptionCollection;
            Opus.Core.ValueTypeOption<EBrowseInformation> enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            switch (enumOption.Value)
            {
                case EBrowseInformation.None:
                    // do nothing
                    break;

                case EBrowseInformation.Full:
                    commandLineBuilder.AppendFormat("/FR\"{0}\"\\ ", options.OutputDirectoryPath);
                    break;

                case EBrowseInformation.NoLocalSymbols:
                    commandLineBuilder.AppendFormat("/Fr\"{0}\"\\ ", options.OutputDirectoryPath);
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized EBrowseInformation option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary BrowseInformationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EBrowseInformation> enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("BrowseInformation", enumOption.Value.ToString("D"));
            C.CompilerOptionCollection options = sender as C.CompilerOptionCollection;
            // the trailing directory separator is important, or unexpected rebuilds occur
            dictionary.Add("BrowseInformationFile", options.OutputDirectoryPath + "\\");
            return dictionary;
        }

        private static void StringPoolingCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/GF ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary StringPoolingVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("StringPooling", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void DisableLanguageExtensionsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/Za ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DisableLanguageExtensionsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("DisableLanguageExtensions", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void ForceConformanceInForLoopScopeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/Zc:forScope ");
            }
            else
            {
                commandLineBuilder.Append("/Zc:forScope- ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ForceConformanceInForLoopScopeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("ForceConformanceInForLoopScope", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void ShowIncludesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/showIncludes ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ShowIncludesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("ShowIncludes", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void UseFullPathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/FC ");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary UseFullPathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("UseFullPaths", boolOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void CompileAsManagedCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EManagedCompilation> enumOption = option as Opus.Core.ValueTypeOption<EManagedCompilation>;
            switch (enumOption.Value)
            {
                case EManagedCompilation.NoCLR:
                    break;

                case EManagedCompilation.CLR:
                    commandLineBuilder.Append("/clr ");
                    break;

                case EManagedCompilation.PureCLR:
                    commandLineBuilder.Append("/clr:pure ");
                    break;

                case EManagedCompilation.SafeCLR:
                    commandLineBuilder.Append("/clr:safe ");
                    break;

                case EManagedCompilation.OldSyntaxCLR:
                    commandLineBuilder.Append("/clr:oldsyntax ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized EManagedCompilation option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary CompileAsManagedVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EManagedCompilation> enumOption = option as Opus.Core.ValueTypeOption<EManagedCompilation>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("CompileAsManaged", enumOption.Value.ToString("D"));
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

        private static void DebugTypeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (options.DebugSymbols)
            {
                Opus.Core.ValueTypeOption<EDebugType> enumOption = option as Opus.Core.ValueTypeOption<EDebugType>;
                switch (options.DebugType)
                {
                    case EDebugType.Embedded:
                        commandLineBuilder.Append("/Z7 ");
                        break;

                    case EDebugType.ProgramDatabase:
                        {
                            commandLineBuilder.Append("/Zi ");

                            if (null == options.ProgramDatabaseFilePath)
                            {
                                throw new Opus.Core.Exception("PDB file path has not been set");
                            }
                            commandLineBuilder.Append(System.String.Format("/Fd\"{0}\" ", options.ProgramDatabaseFilePath));
                        }
                        break;

                    case EDebugType.ProgramDatabaseEditAndContinue:
                        {
                            commandLineBuilder.Append("/ZI ");

                            if (null == options.ProgramDatabaseFilePath)
                            {
                                throw new Opus.Core.Exception("PDB file path has not been set");
                            }
                            commandLineBuilder.Append(System.String.Format("/Fd\"{0}\" ", options.ProgramDatabaseFilePath));
                        }
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DebugTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            string attributeName = "DebugInformationFormat";
            if (options.DebugSymbols)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                Opus.Core.ValueTypeOption<EDebugType> enumOption = option as Opus.Core.ValueTypeOption<EDebugType>;
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
                return dictionary;
            }
            else
            {
                return null;
            }
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
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target);
            return dictionary;
        }
    }
}