// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    [System.Flags]
    public enum OutputFileFlags
    {
        MocGeneratedSourceFile = (1 << 0),
        MocGeneratedSourceFileCollection = (1 << 1)
    }
}