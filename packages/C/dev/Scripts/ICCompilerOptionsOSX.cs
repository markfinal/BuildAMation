// <copyright file="ICCompilerOptionsOSX.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ICCompilerOptionsOSX
    {
        /// <summary>
        /// List of directories the compiler searches for Frameworks
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.DirectoryCollection FrameworkSearchDirectories
        {
            get;
            set;
        }
    }
}
