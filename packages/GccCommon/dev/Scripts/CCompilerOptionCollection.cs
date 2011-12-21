// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions, ICCompilerOptions
    {
        private void SetDelegates(Opus.Core.Target target)
        {
            // common compiler options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(ToolchainOptionCollectionCommandLine);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLine);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLine);
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLine);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLine);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLine);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLine);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLine);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLine);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLine);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLine);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLine);

            // compiler specific options
            this["64bit"].PrivateData = new PrivateData(SixtyFourBitCommandLine);
            this["StrictAliasing"].PrivateData = new PrivateData(StrictAliasingCommandLine);
            this["AllWarnings"].PrivateData = new PrivateData(AllWarningsCommandLine);
            this["ExtraWarnings"].PrivateData = new PrivateData(ExtraWarningsCommandLine);
            this["PositionIndependentCode"].PrivateData = new PrivateData(PositionIndependentCodeCommandLine);
            this["InlineFunctions"].PrivateData = new PrivateData(InlineFunctionsCommandLine);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;
            this["64bit"] = new Opus.Core.ValueTypeOption<bool>(Opus.Core.OSUtilities.Is64Bit(target.Platform));

            if (Opus.Core.EConfiguration.Debug == target.Configuration)
            {
                this.StrictAliasing = false;
                this.InlineFunctions = false;
                this.OmitFramePointer = false;
            }
            else
            {
                this.StrictAliasing = true;
                this.InlineFunctions = true;
                this.OmitFramePointer = true;
            }

            this.AllWarnings = true;
            this.ExtraWarnings = true;

            this.PositionIndependentCode = false;

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool) as CCompiler;
            this.SystemIncludePaths.AddRange(null, compilerInstance.IncludeDirectoryPaths(target));

            this.TargetLanguage = C.ETargetLanguage.C;

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

        private static void SystemIncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-isystem\"{0}\"", includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-isystem{0}", includePath));
                }
            }
        }

        private static void IncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-I\"{0}\"", includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-I{0}", includePath));
                }
            }
        }

        private static void DefinesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string define in definesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", define));
            }
        }

        private static void DebugSymbolsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
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
                        string objectPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".o";
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
            if (null == options.OutputName)
            {
                options.ObjectFilePath = null;
                return;
            }

            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLineBuilder.Add("-c");
                    if (options.ObjectFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-o \"{0}\"", options.ObjectFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-o {0}", options.ObjectFilePath));
                    }
                    break;

                case C.ECompilerOutput.Preprocess:
                    commandLineBuilder.Add("-E");
                    if (options.ObjectFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-o \"{0}\" ", options.ObjectFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-o {0} ", options.ObjectFilePath));
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
        }

        private static void OptimizationCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EOptimization> optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            switch (optimizationOption.Value)
            {
                case C.EOptimization.Off:
                    commandLineBuilder.Add("-O0");
                    break;

                case C.EOptimization.Size:
                    commandLineBuilder.Add("-Os");
                    break;

                case C.EOptimization.Speed:
                    commandLineBuilder.Add("-O1");
                    break;

                case C.EOptimization.Full:
                    commandLineBuilder.Add("-O3");
                    break;

                case C.EOptimization.Custom:
                    // do nothing
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized optimization option");
            }
        }

        private static void CustomOptimizationCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(customOptimizationOption.Value);
        }

        private static void WarningsAsErrorsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Add("-Werror");
            }
        }

        private static void IgnoreStandardIncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Add("-nostdinc");

                CCompilerOptionCollection options = sender as CCompilerOptionCollection;
                if (options.TargetLanguage == C.ETargetLanguage.CPlusPlus)
                {
                    commandLineBuilder.Add("-nostdinc++");
                }
            }
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
                    commandLineBuilder.Add("-x c");
                    break;

                case C.ETargetLanguage.CPlusPlus:
                    commandLineBuilder.Add("-x c++");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized target language option");
            }
        }

        private static void ShowIncludesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-H");
            }
        }

        private static void SixtyFourBitCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }

        private static void StrictAliasingCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fstrict-aliasing");
            }
            else
            {
                commandLineBuilder.Add("-fno-strict-aliasing");
            }
        }

        private static void AllWarningsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wall");
            }
        }

        private static void ExtraWarningsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wextra");
            }
        }

        private static void PositionIndependentCodeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fPIC");
            }
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

        private static void InlineFunctionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-finline-functions");
            }
            else
            {
                commandLineBuilder.Add("-fno-inline-functions");
            }
        }

        private static void OmitFramePointerCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fomit-frame-pointer");
            }
            else
            {
                commandLineBuilder.Add("-fno-omit-frame-pointer");
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.ObjectFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.ObjectFilePath), false);
            }

            return directoriesToCreate;
        }
    }
}
