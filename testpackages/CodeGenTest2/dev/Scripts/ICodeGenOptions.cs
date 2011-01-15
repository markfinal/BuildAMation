namespace CodeGenTest2
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