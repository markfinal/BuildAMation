// <copyright file="IArchiverTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalArchiverOptionsDelegateAttribute),
                                   typeof(ExportArchiverOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetArchiverToolset")]
    public interface IArchiverTool :
        Bam.Core.ITool
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
