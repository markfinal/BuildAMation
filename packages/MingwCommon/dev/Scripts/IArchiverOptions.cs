// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public interface IArchiverOptions
    {
        MingwCommon.EArchiverCommand Command
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
