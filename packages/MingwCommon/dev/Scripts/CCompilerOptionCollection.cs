// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public sealed class CompilerOutputPathFlag : C.CompilerOutputPathFlag
    {
        private CompilerOutputPathFlag(string name)
            : base(name)
        {
        }
    }

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

            // compiler specific options
            if (target.Platform == Opus.Core.EPlatform.Unix64)
            {
                this["64bit"].PrivateData = new PrivateData(SixtyFourBitCommandLine);
            }
            this["StrictAliasing"].PrivateData = new PrivateData(StrictAliasingCommandLine);
            this["AllWarnings"].PrivateData = new PrivateData(AllWarningsCommandLine);
            this["ExtraWarnings"].PrivateData = new PrivateData(ExtraWarningsCommandLine);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;
            if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                this["64bit"] = new Opus.Core.ValueTypeOption<bool>(true);
            }

            if (Opus.Core.EConfiguration.Debug == target.Configuration)
            {
                this.StrictAliasing = false;
            }
            else
            {
                this.StrictAliasing = true;
            }

            this.AllWarnings = true;
            this.ExtraWarnings = true;

            this.TargetLanguage = C.ETargetLanguage.C;

            CCompiler compilerInstance = C.CompilerFactory.GetTargetInstance(node.Target, C.ClassNames.CCompilerTool) as CCompiler;
            this.SystemIncludePaths.AddRange(null, compilerInstance.IncludeDirectoryPaths(node.Target));

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
                return this.OutputPaths[CompilerOutputPathFlag.ObjectFile];
            }

            set
            {
                this.OutputPaths[CompilerOutputPathFlag.ObjectFile] = value;
            }
        }

        public override string PreprocessedFilePath
        {
            get
            {
                return this.OutputPaths[CompilerOutputPathFlag.PreprocessedFile];
            }

            set
            {
                this.OutputPaths[CompilerOutputPathFlag.PreprocessedFile] = value;
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

        private static void SystemIncludePathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                commandLineBuilder.AppendFormat("-isystem\"{0}\" ", includePath);
            }
        }

        private static void IncludePathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                commandLineBuilder.AppendFormat("-I\"{0}\" ", includePath);
            }
        }

        private static void DefinesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string define in definesOption.Value)
            {
                commandLineBuilder.AppendFormat("-D{0} ", define);
            }
        }

        private static void DebugSymbolsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Append("-g ");
            }
        }

        protected static void OutputTypeSetHandler(object sender, Opus.Core.Option option)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
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

        private static void OutputTypeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLineBuilder.AppendFormat("-c -o \"{0}\" ", options.ObjectFilePath);
                    break;

                case C.ECompilerOutput.Preprocess:
                    commandLineBuilder.AppendFormat("-E -o \"{0}\" ", options.ObjectFilePath);
                    break;

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
                    commandLineBuilder.Append("-O0 ");
                    break;

                case C.EOptimization.Size:
                    commandLineBuilder.Append("-Os ");
                    break;

                case C.EOptimization.Speed:
                    commandLineBuilder.Append("-O1 ");
                    break;

                case C.EOptimization.Full:
                    commandLineBuilder.Append("-O3 ");
                    break;

                case C.EOptimization.Custom:
                    // do nothing
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized optimization option");
            }
        }

        private static void CustomOptimizationCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Append(customOptimizationOption.Value);
        }

        private static void WarningsAsErrorsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Append("-Werror ");
            }
        }

        private static void IgnoreStandardIncludePathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Append("-nostdinc ");

                CCompilerOptionCollection options = sender as CCompilerOptionCollection;
                if (options.TargetLanguage == C.ETargetLanguage.CPlusPlus)
                {
                    commandLineBuilder.Append("-nostdinc++ ");
                }
            }
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
                    commandLineBuilder.Append("-x c ");
                    break;

                case C.ETargetLanguage.CPlusPlus:
                    commandLineBuilder.Append("-x c++ ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized target language option");
            }
        }

        private static void ShowIncludesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-H ");
            }
        }

        private static void SixtyFourBitCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Append("-m64 ");
            }
        }

        private static void StrictAliasingCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-fstrict-aliasing ");
            }
            else
            {
                commandLineBuilder.Append("-fno-strict-aliasing ");
            }
        }

        private static void AllWarningsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-Wall ");
            }
        }

        private static void ExtraWarningsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-Wextra ");
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.ObjectFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.ObjectFilePath), false);
            }

            return directoriesToCreate;
        }
    }
}