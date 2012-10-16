// <copyright file="ILinkerInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ILinkerInfo
    {
        string ExecutableSuffix
        {
            get;
        }

        string MapFileSuffix
        {
            get;
        }

        string ImportLibraryPrefix
        {
            get;
        }

        string ImportLibrarySuffix
        {
            get;
        }

        string DynamicLibraryPrefix
        {
            get;
        }

        string DynamicLibrarySuffix
        {
            get;
        }

        string ImportLibrarySubDirectory
        {
            get;
        }

        string BinaryOutputSubDirectory
        {
            get;
        }
    }
}