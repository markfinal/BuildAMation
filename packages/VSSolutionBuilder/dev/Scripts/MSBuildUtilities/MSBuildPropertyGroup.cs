// <copyright file="MSBuildPropertyGroup.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildPropertyGroup : MSBuildBaseElement
    {
        public MSBuildPropertyGroup(System.Xml.XmlDocument document)
            : base(document, "PropertyGroup")
        {
        }

        public MSBuildProperty CreateProperty(string name, string value)
        {
            MSBuildProperty property = new MSBuildProperty(this.XmlDocument, name, value);
            this.AppendChild(property);
            return property;
        }
    }
}