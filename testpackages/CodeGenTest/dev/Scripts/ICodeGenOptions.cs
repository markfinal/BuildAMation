namespace CodeGenTest
{
    public interface ICodeGenOptions
    {
        string OutputSourceDirectory
        {
            get;
            set;
        }

        string OutputName
        {
            get;
            set;
        }
    }
}
