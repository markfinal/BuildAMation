// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/IArchiverOptions.cs&IArchiverOptions.cs
//     -n=MingwCommon
//     -c=ArchiverOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData

namespace MingwCommon
{
    public partial class ArchiverOptionCollection
    {
        #region C.IArchiverOptions Option delegates
        private static void
        OutputTypeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    {
                        var options = sender as ArchiverOptionCollection;
                        var libraryLocation = options.OwningNode.Module.Locations[C.StaticLibrary.OutputFileLocKey];
                        var libraryFilePath = libraryLocation.GetSinglePath();
                        commandLineBuilder.Add(libraryFilePath);
                    }
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }
        private static void
        AdditionalOptionsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        #endregion
        #region IArchiverOptions Option delegates
        private static void
        CommandCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var commandOption = option as Bam.Core.ValueTypeOption<EArchiverCommand>;
            switch (commandOption.Value)
            {
                case EArchiverCommand.Replace:
                    commandLineBuilder.Add("-r");
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized command option");
            }
        }
        private static void
        DoNotWarnIfLibraryCreatedCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-c");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor);
            this["Command"].PrivateData = new PrivateData(CommandCommandLineProcessor);
            this["DoNotWarnIfLibraryCreated"].PrivateData = new PrivateData(DoNotWarnIfLibraryCreatedCommandLineProcessor);
        }
    }
}
