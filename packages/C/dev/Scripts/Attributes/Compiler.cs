// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportCompilerOptionsDelegateAttribute :
        System.Attribute
    {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalCompilerOptionsDelegateAttribute :
        System.Attribute
    {}
}
