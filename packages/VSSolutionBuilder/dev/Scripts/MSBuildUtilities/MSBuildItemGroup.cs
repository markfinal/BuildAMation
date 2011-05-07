// <copyright file="MSBuildItemGroup.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildItemGroup : MSBuildBaseElement
    {
        public MSBuildItemGroup(System.Xml.XmlDocument document)
            : base(document, "ItemGroup")
        {
        }

        public MSBuildItem CreateItem(string name, string include)
        {
            MSBuildItem item = new MSBuildItem(this.XmlDocument, name, include);
            this.AppendChild(item);
            return item;
        }

        public MSBuildItem FindItem(string name, string include)
        {
            foreach (System.Xml.XmlElement element in this.XmlElement.ChildNodes)
            {
                MSBuildItem b = this.childElements[element] as MSBuildItem;
                if ((b.Name == name) && (b.Include == include))
                {
                    return b;
                }
            }
            return null;
        }
    }
}