// <copyright file="ICopyFileTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportCopyFileOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalCopyFileOptionsDelegateAttribute : System.Attribute
    {
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalCopyFileOptionsDelegateAttribute), typeof(ExportCopyFileOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("FileUtilities")]
    public interface ICopyFileTool : Opus.Core.ITool
    {
    }
}
