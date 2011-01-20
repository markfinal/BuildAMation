namespace Qt
{
    public sealed class MocPrivateData : CommandLineProcessor.ICommandLineDelegate
    {
        public MocPrivateData(CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }
}