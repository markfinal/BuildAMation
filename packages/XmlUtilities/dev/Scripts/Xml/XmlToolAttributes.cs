// <copyright file="XmlToolAttributes.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>

namespace XmlUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportXmlWriterOptionsDelegateAttribute : System.Attribute
    {
    }
    
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalXmlWriterOptionsDelegateAttribute : System.Attribute
    {
    }
}
