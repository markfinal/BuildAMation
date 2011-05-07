// <copyright file="MSBuildProperty.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildProperty : MSBuildBaseElement
    {
        public MSBuildProperty(System.Xml.XmlDocument document,
                               string name,
                               string value)
            : base(document, name)
        {
            this.XmlElement.InnerText = value;
        }
    }
}