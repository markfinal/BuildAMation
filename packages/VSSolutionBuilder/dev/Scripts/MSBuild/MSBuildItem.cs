// <copyright file="MSBuildItem.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildItem : MSBuildBaseElement
    {
        public MSBuildItem(System.Xml.XmlDocument document,
                           string name,
                           string include)
            : base(document, name)
        {
            // null is only valid when used in an item definition group
            if (null != include)
            {
                this.XmlElement.SetAttribute("Include", include);
            }
        }

        public MSBuildMetaData CreateMetaData(string name, string value)
        {
            MSBuildMetaData metaData = new MSBuildMetaData(this.XmlDocument, name, value);
            this.AppendChild(metaData);
            return metaData;
        }
    }
}