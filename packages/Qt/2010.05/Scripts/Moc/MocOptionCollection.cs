namespace Qt
{
    public sealed class MocOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, Opus.Core.IOutputPaths
    {
        public MocOptionCollection(Opus.Core.DependencyNode node)
        {
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            throw new System.NotImplementedException();
        }

        System.Collections.Generic.Dictionary<string, string> Opus.Core.IOutputPaths.GetOutputPaths()
        {
            throw new System.NotImplementedException();
        }
    }
}