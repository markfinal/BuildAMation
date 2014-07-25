// <copyright file="IPosixSharedLibrarySymlinksTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalPosixSharedLibrarySymlinksToolOptionsDelegateAttribute),
                                   typeof(ExportPosixSharedLibrarySymlinksToolOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetPosixSharedLibrarySymlinksToolset")]
    public interface IPosixSharedLibrarySymlinksTool :
        Opus.Core.ITool
    {}
}
