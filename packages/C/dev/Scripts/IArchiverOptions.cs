// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface IArchiverOptions
    {
#if false
        C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get;
            set;
        }
#endif

        C.EArchiverOutput OutputType
        {
            get;
            set;
        }

        string AdditionalOptions
        {
            get;
            set;
        }
    }
}
