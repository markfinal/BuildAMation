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
using System.Linq;
namespace VSSolutionBuilder
{
    static class XmlDocumentExtensions
    {
        public static System.Xml.XmlElement
        CreateVSElement(
            this System.Xml.XmlDocument document,
            string name,
            string value = null,
            string condition = null,
            System.Xml.XmlElement parentEl = null)
        {
            const string ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var el = document.CreateElement(name, ns);
            if (null != value)
            {
                el.InnerText = value;
            }
            if (null != condition)
            {
                el.SetAttribute("Condition", condition);
            }
            if (null != parentEl)
            {
                parentEl.AppendChild(el);
            }
            return el;
        }

        public static System.Xml.XmlElement
        CreateVSItemGroup(
            this System.Xml.XmlDocument document,
            string label = null,
            System.Xml.XmlElement parentEl = null)
        {
            var itemGroup = document.CreateVSElement("ItemGroup", parentEl: parentEl);
            if (null != label)
            {
                itemGroup.SetAttribute("Label", label);
            }
            return itemGroup;
        }

        public static System.Xml.XmlElement
        CreateVSPropertyGroup(
            this System.Xml.XmlDocument document,
            string label = null,
            string condition = null,
            System.Xml.XmlElement parentEl = null)
        {
            var propertyGroup = document.CreateVSElement("PropertyGroup", condition: condition, parentEl: parentEl);
            if (null != label)
            {
                propertyGroup.SetAttribute("Label", label);
            }
            return propertyGroup;
        }

        public static System.Xml.XmlElement
        CreateVSItemDefinitionGroup(
            this System.Xml.XmlDocument document,
            string condition = null,
            System.Xml.XmlElement parentEl = null)
        {
            return document.CreateVSElement("ItemDefinitionGroup", condition: condition, parentEl: parentEl);
        }

        public static System.Xml.XmlElement
        CreateVSImport(
            this System.Xml.XmlDocument document,
            string importPath,
            System.Xml.XmlElement parentEl = null)
        {
            var import = document.CreateVSElement("Import", parentEl: parentEl);
            import.SetAttribute("Project", importPath);
            return import;
        }
    }
}
