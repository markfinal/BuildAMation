// <copyright file="ICopyFileOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public interface ICopyFileOptions
    {
        /// <summary>
        /// Gets or sets the destination directory.
        /// </summary>
        /// <value>The destination directory.</value>
        // StateOnly
        string DestinationDirectory
        {
            get;
            set;
        }

        // StateOnly
        string CommonBaseDirectory
        {
            get;
            set;
        }

        // StateOnly
        System.Type DestinationModuleType
        {
            get;
            set;
        }

        // StateOnly
        Opus.Core.LocationKey DestinationModuleOutputLocation
        {
            get;
            set;
        }

        // StateOnly
        string DestinationRelativePath
        {
            get;
            set;
        }

        // StateOnly
        System.Type SourceModuleType
        {
            get;
            set;
        }

        // StateOnly
        Opus.Core.LocationKey SourceModuleOutputLocation
        {
            get;
            set;
        }
    }
}
