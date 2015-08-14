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
    public class MSBuildProject :
        MSBuildBaseElement
    {
        public
        MSBuildProject(
            System.Xml.XmlDocument document,
            string toolsVersion,
            string defaultTargets) : base(document, "Project")
        {
            if (null != defaultTargets)
            {
                this.XmlElement.SetAttribute("DefaultTargets", defaultTargets);
            }
            this.XmlElement.SetAttribute("ToolsVersion", toolsVersion);
            this.XmlDocument.AppendChild(this.XmlElement);
        }

        public
        MSBuildProject(
            System.Xml.XmlDocument document,
            string toolsVersion)
            : this(document, toolsVersion, null)
        {}

        public MSBuildPropertyGroup
        CreatePropertyGroup()
        {
            var propertyGroup = new MSBuildPropertyGroup(this.XmlDocument);
            this.AppendChild(propertyGroup);
            return propertyGroup;
        }

        public MSBuildItemGroup
        CreateItemGroup()
        {
            var itemGroup = new MSBuildItemGroup(this.XmlDocument);
            this.AppendChild(itemGroup);
            return itemGroup;
        }

        public MSBuildItemDefinitionGroup
        CreateItemDefinitionGroup()
        {
            var itemDefGroup = new MSBuildItemDefinitionGroup(this.XmlDocument);
            this.AppendChild(itemDefGroup);
            return itemDefGroup;
        }

        public MSBuildImport
        CreateImport(
            string projectFile)
        {
            var import = new MSBuildImport(this.XmlDocument, projectFile);
            this.AppendChild(import);
            return import;
        }
    }
}
