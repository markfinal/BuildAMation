namespace Qt
{
    public interface IMocOptions
    {
        string MocOutputPath
        {
            get;
            set;
        }

        Opus.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }
    }
}