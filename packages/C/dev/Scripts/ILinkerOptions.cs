// <copyright file="ILinkerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ILinkerOptions
    {
        /// <summary>
        /// Specify the output type of the linked binary
        /// </summary>
        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Exclude standard libraries from the linking phase
        /// </summary>
        bool DoNotAutoIncludeStandardLibraries
        {
            get;
            set;
        }

        /// <summary>
        /// Generate debug symbols for the linked binary
        /// </summary>
        bool DebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Specify the subsystem for the linked binary
        /// </summary>
        C.ESubsystem SubSystem
        {
            get;
            set;
        }

        /// <summary>
        /// Specify whether linked binary is a dynamic library
        /// </summary>
        bool DynamicLibrary
        {
            get;
            set;
        }

        /// <summary>
        /// Specify search paths for libraries
        /// </summary>
        Opus.Core.DirectoryCollection LibraryPaths
        {
            get;
            set;
        }

        /// <summary>
        /// Specify standard libraries to link against
        /// </summary>
        Opus.Core.FileCollection StandardLibraries
        {
            get;
            set;
        }

        /// <summary>
        /// Specify user libraries to link against
        /// </summary>
        Opus.Core.FileCollection Libraries
        {
            get;
            set;
        }

        /// <summary>
        /// The link step generates a map file for the binary
        /// </summary>
        bool GenerateMapFile
        {
            get;
            set;
        }

        /// <summary>
        /// Additional options passed to the linker
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }
    }
}
