// <copyright file="ICSharpCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportCscOptionsDelegateAttribute :
        System.Attribute
    {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalCscOptionsDelegateAttribute :
        System.Attribute
    {}

    [Opus.Core.LocalAndExportTypes(typeof(LocalCscOptionsDelegateAttribute),
                                   typeof(ExportCscOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("dotnet")]
    public interface ICSharpCompilerTool :
        Opus.Core.ITool
    {}
}
