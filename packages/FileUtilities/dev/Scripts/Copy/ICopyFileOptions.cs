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
    }
}
