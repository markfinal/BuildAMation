// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\IArchiverOptions.cs;IArchiverOptions.cs -n=VisualCCommon -c=ArchiverOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs;..\..\..\VisualStudioProcessor\dev\Scripts\VisualStudioDelegate.cs -pv=PrivateData

namespace VisualCCommon
{
    public partial class ArchiverOptionCollection
    {
        #region C.IArchiverOptions Option delegates
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                    if (options.LibraryFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/OUT:\"{0}\"", options.LibraryFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/OUT:{0}", options.LibraryFilePath));
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                        returnVal.Add("OutputFile", options.LibraryFilePath);
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalOptions", stringOption.Value);
            return returnVal;
        }
        #endregion
        #region IArchiverOptions Option delegates
        private static void NoLogoCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("/NOLOGO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return returnVal;
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeVisualStudioProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsVisualStudioProcessor);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLineProcessor,NoLogoVisualStudioProcessor);
        }
    }
}
