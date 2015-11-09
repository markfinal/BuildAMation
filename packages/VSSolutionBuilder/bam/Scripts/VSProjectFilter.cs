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
    public sealed class VSProjectFilter
    {
        private static readonly string SourceGroupName = "Source Files";
        private static readonly string HeaderGroupName = "Header Files";
        private static readonly string OtherGroupName = "Other Files";
        private static readonly string ResourcesGroupName = "Resource Files";
        private Bam.Core.Array<VSSettingsGroup> Source = new Bam.Core.Array<VSSettingsGroup>();
        private Bam.Core.Array<VSSettingsGroup> Headers = new Bam.Core.Array<VSSettingsGroup>();
        private Bam.Core.Array<VSSettingsGroup> Others = new Bam.Core.Array<VSSettingsGroup>();
        private Bam.Core.Array<VSSettingsGroup> Resources = new Bam.Core.Array<VSSettingsGroup>();

        public void
        AddHeader(
            Bam.Core.TokenizedString path)
        {
            lock (this.Headers)
            {
                if (!this.Headers.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Header, include: path);
                    group.AddSetting("Filter", HeaderGroupName);
                    this.Headers.AddUnique(group);
                }
            }
        }

        public void
        AddSource(
            Bam.Core.TokenizedString path)
        {
            lock (this.Source)
            {
                if (!this.Source.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Compiler, include: path);
                    group.AddSetting("Filter", SourceGroupName);
                    this.Source.AddUnique(group);
                }
            }
        }

        public void
        AddOther(
            Bam.Core.TokenizedString path)
        {
            lock (this.Others)
            {
                if (!this.Others.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.CustomBuild, include: path);
                    group.AddSetting("Filter", OtherGroupName);
                    this.Others.AddUnique(group);
                }
            }
        }

        public void
        AddResource(
            Bam.Core.TokenizedString path)
        {
            lock (this.Resources)
            {
                if (!this.Resources.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Resource, include: path);
                    group.AddSetting("Filter", ResourcesGroupName);
                    this.Resources.AddUnique(group);
                }
            }
        }

        public System.Xml.XmlDocument
        Serialize()
        {
            var document = new System.Xml.XmlDocument();

            var projectEl = this.CreateRootProject(document);
            projectEl.SetAttribute("ToolsVersion", "4.0"); // TODO: get this number from VisualC

            System.Action<System.Xml.XmlElement, string, Bam.Core.Array<VSSettingsGroup>> createXML = (parentEl, groupName, list) =>
                {
                    var allFiles = document.CreateVSItemGroup(parentEl: projectEl);

                    var idEl = document.CreateVSElement("Filter", parentEl: parentEl);
                    idEl.SetAttribute("Include", groupName);
                    document.CreateVSElement("UniqueIdentifier", new DeterministicGuid("VSFilter" + groupName).Guid.ToString("B").ToUpper(), parentEl: idEl);
                    var extensions = new Bam.Core.StringArray();
                    foreach (var setting in list)
                    {
                        var path = setting.Include;
                        var extension = System.IO.Path.GetExtension(path.Parse()).TrimStart(new[] { '.' });
                        extensions.AddUnique(extension);

                        setting.Serialize(document, allFiles);
                    }
                    document.CreateVSElement("Extensions", extensions.ToString(';'), parentEl: idEl);
                };

            var containerEl = document.CreateVSItemGroup(parentEl: projectEl);
            if (this.Source.Count > 0)
            {
                createXML(containerEl, SourceGroupName, this.Source);
            }
            if (this.Headers.Count > 0)
            {
                createXML(containerEl, HeaderGroupName, this.Headers);
            }
            if (this.Others.Count > 0)
            {
                createXML(containerEl, OtherGroupName, this.Others);
            }
            if (this.Resources.Count > 0)
            {
                createXML(containerEl, ResourcesGroupName, this.Resources);
            }

            return document;
        }

        private System.Xml.XmlElement
        CreateRootProject(
            System.Xml.XmlDocument document)
        {
            var project = document.CreateVSElement("Project");
            document.AppendChild(project);
            return project;
        }
    }
}
