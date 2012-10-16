// <copyright file="IArchiverInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface IArchiverInfo
    {
        string StaticLibraryPrefix
        {
            get;
        }

        string StaticLibrarySuffix
        {
            get;
        }

        string StaticLibraryOutputSubDirectory
        {
            get;
        }
    }
}