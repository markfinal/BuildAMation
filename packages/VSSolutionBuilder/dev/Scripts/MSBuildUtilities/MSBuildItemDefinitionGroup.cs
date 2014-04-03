// <copyright file="MSBuildItemDefinitionGroup.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildItemDefinitionGroup : MSBuildBaseElement
    {
        public MSBuildItemDefinitionGroup(System.Xml.XmlDocument document)
            : base(document, "ItemDefinitionGroup")
        {
        }

        public MSBuildItem CreateItem(string name)
        {
            MSBuildItem item = new MSBuildItem(this.XmlDocument, name, null);
            this.AppendChild(item);
            return item;
        }
    }
}