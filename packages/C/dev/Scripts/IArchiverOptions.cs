// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface IArchiverOptions
    {
        C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get;
            set;
        }

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
