// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public interface IArchiverOptions
    {
        GccCommon.EArchiverCommand Command
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
