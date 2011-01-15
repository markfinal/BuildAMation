// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportArchiverOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalArchiverOptionsDelegateAttribute : System.Attribute
    {
    }

    public abstract class Archiver
    {
    }
}