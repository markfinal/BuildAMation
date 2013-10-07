// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ICCompilerOptions.cs:ICCompilerOptions.cs -n=ClangCommon -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=PrivateData

namespace ClangCommon
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptions Option delegates
        private static void DefinesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (var define in definesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", define));
            }
        }
        private static void DefinesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void IncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var toolset = target.Toolset;
            var compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var switchPrefix = compiler.IncludePathCompilerSwitches[0];
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            // TODO: convert to 'var'
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
        private static void IncludePathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void SystemIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            IncludePathsCommandLineProcessor(sender, commandLineBuilder, option, target);
        }
        private static void SystemIncludePathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var options = sender as CCompilerOptionCollection;
            if (null == options.OutputName)
            {
                options.ObjectFilePath = null;
                return;
            }
            commandLineBuilder.Add("-c");
            commandLineBuilder.Add("-o");
            commandLineBuilder.Add(options.ObjectFilePath);
        }
        private static void OutputTypeXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
            else
            {
                commandLineBuilder.Add("-g0");
            }
        }
        private static void DebugSymbolsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void WarningsAsErrorsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Add("-Werror");
            }
        }
        private static void WarningsAsErrorsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void IgnoreStandardIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var ignoreStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Add("-nostdinc");
                var options = sender as C.ICCompilerOptions;
                if (options.TargetLanguage == C.ETargetLanguage.Cxx)
                {
                    commandLineBuilder.Add("-nostdinc++");
                }
            }
        }
        private static void IgnoreStandardIncludePathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void OptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
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
        private static void OptimizationXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void CustomOptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(customOptimizationOption.Value);
        }
        private static void CustomOptimizationXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void TargetLanguageCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
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
        private static void TargetLanguageXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void ShowIncludesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-H");
            }
        }
        private static void ShowIncludesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static void AdditionalOptionsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void OmitFramePointerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fomit-frame-pointer");
            }
            else
            {
                commandLineBuilder.Add("-fno-omit-frame-pointer");
            }
        }
        private static void OmitFramePointerXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void DisableWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (var warning in disableWarningsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wno-{0}", warning));
            }
        }
        private static void DisableWarningsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void CharacterSetCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<C.ECharacterSet>;
            var cOptions = sender as C.ICCompilerOptions;
            switch (enumOption.Value)
            {
                case C.ECharacterSet.NotSet:
                    break;
                case C.ECharacterSet.Unicode:
                    cOptions.Defines.Add("_UNICODE");
                    cOptions.Defines.Add("UNICODE");
                    break;
                case C.ECharacterSet.MultiByte:
                    cOptions.Defines.Add("_MBCS");
                    break;
            }
        }
        private static void CharacterSetXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void LanguageStandardCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void LanguageStandardXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        #region ICCompilerOptions Option delegates
        private static void PositionIndependentCodeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var pic = option as Opus.Core.ValueTypeOption<bool>;
            if (pic.Value)
            {
                commandLineBuilder.Add("-fPIC");
            }
        }
        private static void PositionIndependentCodeXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor,DefinesXcodeProjectProcessor);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLineProcessor,IncludePathsXcodeProjectProcessor);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLineProcessor,SystemIncludePathsXcodeProjectProcessor);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeXcodeProjectProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsXcodeProjectProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor,WarningsAsErrorsXcodeProjectProcessor);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLineProcessor,IgnoreStandardIncludePathsXcodeProjectProcessor);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLineProcessor,OptimizationXcodeProjectProcessor);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLineProcessor,CustomOptimizationXcodeProjectProcessor);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLineProcessor,TargetLanguageXcodeProjectProcessor);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLineProcessor,ShowIncludesXcodeProjectProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsXcodeProjectProcessor);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLineProcessor,OmitFramePointerXcodeProjectProcessor);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLineProcessor,DisableWarningsXcodeProjectProcessor);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLineProcessor,CharacterSetXcodeProjectProcessor);
            this["LanguageStandard"].PrivateData = new PrivateData(LanguageStandardCommandLineProcessor,LanguageStandardXcodeProjectProcessor);
            this["PositionIndependentCode"].PrivateData = new PrivateData(PositionIndependentCodeCommandLineProcessor,PositionIndependentCodeXcodeProjectProcessor);
        }
    }
}
