// <copyright file="TextFileToolAttributes.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>

namespace XmlUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportTextFileOptionsDelegateAttribute :
        System.Attribute
    {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalTextFileOptionsDelegateAttribute :
        System.Attribute
    {}
}
