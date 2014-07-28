// <copyright file="MSBuildImport.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildImport :
        MSBuildBaseElement
    {
        public
        MSBuildImport(
            System.Xml.XmlDocument document,
            string projectFile) : base(document, "Import")
        {
            this.XmlElement.SetAttribute("Project", projectFile);
        }
    }
}
