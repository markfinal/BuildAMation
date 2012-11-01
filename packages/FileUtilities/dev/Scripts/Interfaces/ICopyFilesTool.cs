// <copyright file="ICopyFilesTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalOptionsDelegateAttribute),
                                   typeof(ExportOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("FileUtilities")]
    public interface ICopyFilesTool : Opus.Core.ITool
    {
    }
}
