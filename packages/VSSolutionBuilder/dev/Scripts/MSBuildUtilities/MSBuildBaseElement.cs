// <copyright file="IMSBuildBaseElement.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public abstract class MSBuildBaseElement
    {
        protected static string xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
        private string condition;
        private string label;

        public MSBuildBaseElement(System.Xml.XmlDocument document,
                                  string name)
        {
            if (System.String.IsNullOrEmpty(name))
            {
                throw new Opus.Core.Exception("Name of MSBuild XML element cannot be null");
            }

            this.XmlDocument = document;
            this.Name = name;
            this.condition = null;
            this.label = null;
            this.XmlElement = document.CreateElement("", name, xmlns);
        }

        protected System.Xml.XmlDocument XmlDocument
        {
            get;
            set;
        }

        protected System.Xml.XmlElement XmlElement
        {
            get;
            set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public string Condition
        {
            get
            {
                return this.condition;
            }

            set
            {
                if (null != this.condition)
                {
                    throw new Opus.Core.Exception("Condition has already been set");
                }

                this.condition = value;
                this.XmlElement.SetAttribute("Condition", value);
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }

            set
            {
                if (null != this.label)
                {
                    throw new Opus.Core.Exception("Label has already been set");
                }

                this.label = value;
                this.XmlElement.SetAttribute("Label", value);
            }
        }

        protected void AppendChild(MSBuildBaseElement child)
        {
            this.XmlElement.AppendChild(child.XmlElement);
        }
    }
}