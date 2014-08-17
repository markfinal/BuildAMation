#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace VSSolutionBuilder
{
    public abstract class MSBuildBaseElement
    {
        protected static string xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
        private string condition;
        private string label;
        protected System.Collections.Generic.Dictionary<System.Xml.XmlElement, MSBuildBaseElement> childElements = new System.Collections.Generic.Dictionary<System.Xml.XmlElement, MSBuildBaseElement>();

        public
        MSBuildBaseElement(
            System.Xml.XmlDocument document,
            string name)
        {
            if (System.String.IsNullOrEmpty(name))
            {
                throw new Bam.Core.Exception("Name of MSBuild XML element cannot be null");
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
                    throw new Bam.Core.Exception("Condition has already been set");
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
                    throw new Bam.Core.Exception("Label has already been set");
                }

                this.label = value;
                this.XmlElement.SetAttribute("Label", value);
            }
        }

        protected void
        AppendChild(
            MSBuildBaseElement child)
        {
            this.XmlElement.AppendChild(child.XmlElement);
            this.childElements.Add(child.XmlElement, child);
        }
    }
}
