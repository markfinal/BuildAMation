// <copyright file="ProjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectFile
    {
        public static string ConvertDirectorySeparators(string pathName)
        {
            string fixedPathName = pathName.Replace('/', '\\');
            return fixedPathName;
        }

        public ProjectFile(string pathName)
        {
            this.RelativePath = ConvertDirectorySeparators(pathName);
        }

        public string RelativePath
        {
            get;
            private set;
        }

        public ProjectFileConfigurationCollection FileConfigurations
        {
            get;
            set;
        }

        public void Serialize(System.Xml.XmlWriter xmlWriter, System.Uri projectUri)
        {
            xmlWriter.WriteStartElement("File");
            {
                xmlWriter.WriteAttributeString("RelativePath", Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri));
                if (null != this.FileConfigurations)
                {
                    foreach (ProjectFileConfiguration configuration in this.FileConfigurations)
                    {
                        configuration.Serialize(xmlWriter, projectUri);
                    }
                }
            }
            xmlWriter.WriteEndElement();
        }
    }
}