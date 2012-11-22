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
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common compiler options
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
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLine);

            // compiler specific options
            this["64bit"].PrivateData = new PrivateData(SixtyFourBitCommandLine);
            this["StrictAliasing"].PrivateData = new PrivateData(StrictAliasingCommandLine);
            this["AllWarnings"].PrivateData = new PrivateData(AllWarningsCommandLine);
            this["ExtraWarnings"].PrivateData = new PrivateData(ExtraWarningsCommandLine);
            this["PositionIndependentCode"].PrivateData = new PrivateData(PositionIndependentCodeCommandLine);
            this["InlineFunctions"].PrivateData = new PrivateData(InlineFunctionsCommandLine);
            this["Pedantic"].PrivateData = new PrivateData(PedanticCL);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerInterface = this as ICCompilerOptions;
            compilerInterface.AllWarnings = true;
            compilerInterface.ExtraWarnings = true;

            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;
            this["64bit"] = new Opus.Core.ValueTypeOption<bool>(Opus.Core.OSUtilities.Is64Bit(target));

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerInterface.StrictAliasing = false;
                compilerInterface.InlineFunctions = false;
            }
            else
            {
                compilerInterface.StrictAliasing = true;
                compilerInterface.InlineFunctions = true;
            }

            compilerInterface.PositionIndependentCode = false;

            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths(node.Target));

            (this as C.ICCompilerOptions).TargetLanguage = C.ETargetLanguage.C;

            compilerInterface.Pedantic = true;
        }

        public CCompilerOptionCollection()
            : base()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        private static void SystemIncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.ICCompilerOptions optionCollection = sender as C.ICCompilerOptions;
            if (!optionCollection.IgnoreStandardIncludePaths)
            {
                Opus.Core.Log.Full("System include paths not explicitly added to the build");
                return;
            }

            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string switchPrefix = compiler.IncludePathCompilerSwitches[0];

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

        private static void IncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string switchPrefix = compiler.IncludePathCompilerSwitches[1];

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
                    {
                        commandLineBuilder.Add("-c");
                        string objPathName = options.ObjectFilePath;
                        if (objPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\"", objPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0}", objPathName));
                        }
                    }
                    break;

                case C.ECompilerOutput.Preprocess:
                    {
                        commandLineBuilder.Add("-E");
                        string objPathName = options.ObjectFilePath;
                        if (objPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\" ", objPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0} ", objPathName));
                        }
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

                C.ICCompilerOptions options = sender as C.ICCompilerOptions;
                if (options.TargetLanguage == C.ETargetLanguage.Cxx)
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

                case C.ETargetLanguage.Cxx:
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

        private static void PedanticCL(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-pedantic");
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

        private static void DisableWarningsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string warning in disableWarningsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wno-{0}", warning));
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            string objPathName = this.ObjectFilePath;
            if (null != objPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(objPathName), false);
            }

            return directoriesToCreate;
        }
    }
}
