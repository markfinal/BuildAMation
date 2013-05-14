// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public enum OutputFileFlags
    {
        CopiedFile = (1 << 0),
        SymlinkFile = (1 << 1)
    }
}
