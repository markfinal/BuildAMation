// <copyright file="ISymlinkTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportSymlinkOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalSymlinkOptionsDelegateAttribute : System.Attribute
    {
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalSymlinkOptionsDelegateAttribute), typeof(ExportSymlinkOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("FileUtilities")]
    public interface ISymlinkTool : Opus.Core.ITool
    {
    }
}
