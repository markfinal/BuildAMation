// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>IntelCommon package</summary>
// <author>Mark Final</author>
namespace IntelCommon
{
    public interface IArchiverOptions
    {
        IntelCommon.EArchiverCommand Command
        {
            get;
            set;
        }

        bool DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }
    }
}