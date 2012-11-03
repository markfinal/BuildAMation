// <copyright file="ICSharpCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalCscOptionsDelegateAttribute),
                                   typeof(ExportCscOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("csharp")]
    public interface ICSharpCompilerTool : Opus.Core.ITool
    {
    }
}