// <copyright file="IPosixSharedLibrarySymlinksTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalPosixSharedLibrarySymlinksToolOptionsDelegateAttribute),
                                   typeof(ExportPosixSharedLibrarySymlinksToolOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetPosixSharedLibrarySymlinksToolset")]
    public interface IPosixSharedLibrarySymlinksTool :
        Bam.Core.ITool
    {}
}
