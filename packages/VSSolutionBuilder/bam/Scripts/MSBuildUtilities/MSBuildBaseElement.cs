#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
