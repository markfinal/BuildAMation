// <copyright file="ISymlinkOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public interface ISymlinkOptions
    {
        /// <summary>
        /// Gets or sets the name of the target file, which is the symbolic link. If this is not set, the basename of the source file is used.
        /// </summary>
        /// <value>The name of the target.</value>
        string TargetName
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
