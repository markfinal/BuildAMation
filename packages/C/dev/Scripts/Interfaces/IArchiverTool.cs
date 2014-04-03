// <copyright file="IArchiverTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalArchiverOptionsDelegateAttribute),
                                   typeof(ExportArchiverOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetArchiverToolset")]
    public interface IArchiverTool : Opus.Core.ITool
    {
        string StaticLibraryPrefix
        {
            get;
        }

        string StaticLibrarySuffix
        {
            get;
        }

        string StaticLibraryOutputSubDirectory
        {
            get;
        }
    }
}