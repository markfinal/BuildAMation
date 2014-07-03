// <copyright file="ILinkerOptionsOSX.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ILinkerOptionsOSX
    {
        /// <summary>
        /// List of names of OSX frameworks to include in the link step
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Opus.Core.StringArray Frameworks
        {
            get;
            set;
        }

        /// <summary>
        /// List of directories the linker searches for Frameworks
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Opus.Core.DirectoryCollection FrameworkSearchDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// Suppress read only relocations
        /// </summary>
        /// <value><c>true</c> if read only relocations; otherwise, <c>false</c>.</value>
        bool SuppressReadOnlyRelocations
        {
            get;
            set;
        }
    }
}
