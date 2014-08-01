// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/IArchiverOptions.cs&IArchiverOptions.cs
//     -n=VisualCCommon
//     -c=ArchiverOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs
//     -pv=PrivateData

namespace VisualCCommon
{
    public partial class ArchiverOptionCollection
    {
        #region C.IArchiverOptions Option delegates
        private static void
        OutputTypeCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    var options = sender as ArchiverOptionCollection;
                    var libraryLocation = options.OwningNode.Module.Locations[C.StaticLibrary.OutputFileLocKey];
                    var libraryFilePath = libraryLocation.GetSinglePath();
                    commandLineBuilder.Add(System.String.Format("-OUT:{0}", libraryFilePath));
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        OutputTypeVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    {
                        var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        var options = sender as ArchiverOptionCollection;
                        returnVal.Add("OutputFile", options.OwningNode.Module.Locations[C.StaticLibrary.OutputFileLocKey].GetSinglePath());
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }
        private static void
        AdditionalOptionsCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        AdditionalOptionsVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalOptions", stringOption.Value);
            return returnVal;
        }
        #endregion
        #region IArchiverOptions Option delegates
        private static void
        NoLogoCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("-NOLOGO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        NoLogoVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return returnVal;
        }
        #endregion
        protected override void
        SetDelegates(
            Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeVisualStudioProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsVisualStudioProcessor);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLineProcessor,NoLogoVisualStudioProcessor);
        }
    }
}
