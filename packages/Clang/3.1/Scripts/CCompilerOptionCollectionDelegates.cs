// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICCompilerOptions.cs -n=Clang -c=CCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=PrivateData

namespace Clang
{
    public partial class CCompilerOptionCollection
    {
#if false
        private static void ToolchainOptionCollectionCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
#endif
        private static void DefinesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void IncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void SystemIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            CCompilerOptionCollection options = sender as CCompilerOptionCollection;
            if (null == options.OutputName)
            {
                options.ObjectFilePath = null;
                return;
            }

            commandLineBuilder.Add("-c");
            commandLineBuilder.Add("-o");
            commandLineBuilder.Add(options.ObjectFilePath);
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void WarningsAsErrorsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void IgnoreStandardIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void OptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void CustomOptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void TargetLanguageCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void ShowIncludesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void OmitFramePointerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void DisableWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
#if false
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(ToolchainOptionCollectionCommandLineProcessor);
#endif
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLineProcessor);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLineProcessor);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLineProcessor);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLineProcessor);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLineProcessor);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLineProcessor);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLineProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLineProcessor);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLineProcessor);
        }
    }
}
