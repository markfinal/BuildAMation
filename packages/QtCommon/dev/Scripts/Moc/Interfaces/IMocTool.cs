// <copyright file="IMocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(ExportMocOptionsDelegateAttribute))]
    public interface IMocTool : Opus.Core.ITool
    {
    }
}