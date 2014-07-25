// <copyright file="IWinImportLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // extend any implementation of ILinkerTool with this on Windows
    public interface IWinImportLibrary
    {
        string ImportLibraryPrefix
        {
            get;
        }

        string ImportLibrarySuffix
        {
            get;
        }

        string ImportLibrarySubDirectory
        {
            get;
        }
    }
}
