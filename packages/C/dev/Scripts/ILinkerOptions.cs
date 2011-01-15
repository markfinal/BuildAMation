// <copyright file="ILinkerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ILinkerOptions
    {
        C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get;
            set;
        }

        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        bool IgnoreStandardLibraries
        {
            get;
            set;
        }

        bool DebugSymbols
        {
            get;
            set;
        }

        C.ESubsystem SubSystem
        {
            get;
            set;
        }

        bool DynamicLibrary
        {
            get;
            set;
        }

        Opus.Core.DirectoryCollection LibraryPaths
        {
            get;
            set;
        }

        Opus.Core.FileCollection StandardLibraries
        {
            get;
            set;
        }

        Opus.Core.FileCollection Libraries
        {
            get;
            set;
        }

        bool GenerateMapFile
        {
            get;
            set;
        }
    }
}