#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Class representing the filter files associated with VisualStudio project files
    /// </summary>
    public sealed class VSProjectFilter
    {
        private System.Collections.Generic.Dictionary<string, Bam.Core.Array<VSSettingsGroup>> Filters = new System.Collections.Generic.Dictionary<string, Bam.Core.Array<VSSettingsGroup>>();

        /// <summary>
        /// Create a filter for the given project.
        /// </summary>
        /// <param name="project">Project associated with the filter.</param>
        public VSProjectFilter(
            VSProject project)
        {
            this.Project = project;
        }

        private VSProject Project { get; set; }

        private void
        AddFilters(
            Bam.Core.Module module,
            string filterPath)
        {
            if (!this.Filters.ContainsKey(filterPath))
            {
                this.Filters.Add(filterPath, new Bam.Core.Array<VSSettingsGroup>());
            }
            if (!filterPath.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                return;
            }
            // the entire hierarchy needs to be added, even if there are no files in each subdirectory
            var parent = System.IO.Path.GetDirectoryName(filterPath);
            if (null == parent)
            {
                return;
            }
            this.AddFilters(module, parent);
        }

        /// <summary>
        /// Add the file associated with the given settings group to the filter.
        /// </summary>
        /// <param name="sourceGroup">Settings group to add.</param>
        public void
        AddFile(
            VSSettingsGroup sourceGroup)
        {
            if (null != sourceGroup.RelativeDirectory)
            {
                var path = sourceGroup.RelativeDirectory.ToString();
                this.AddFilters(sourceGroup.Project.Module, path);
                var filter = this.Filters[path];
                if (filter.Any(item =>
                    {
                        lock (item.Path)
                        {
                            if (!item.Path.IsParsed)
                            {
                                item.Path.Parse();
                            }
                        }
                        return item.Path.ToString().Equals(sourceGroup.Path.ToString(), System.StringComparison.Ordinal);
                    }))
                {
                    return;
                }
                var newGroup = new VSSettingsGroup(
                    this.Project,
                    sourceGroup.Group,
                    sourceGroup.Path
                );
                newGroup.AddSetting(
                    "Filter",
                    sourceGroup.RelativeDirectory
                );
                filter.AddUnique(newGroup);
            }
            else
            {
                // this codepath is followed when source files are in the package directory directly
                new VSSettingsGroup(
                    this.Project,
                    sourceGroup.Group,
                    sourceGroup.Path
                );
            }
        }

        /// <summary>
        /// Serialize the filter to an XML document.
        /// </summary>
        /// <returns>XML document containing the filter.</returns>
        public System.Xml.XmlDocument
        Serialize()
        {
            var document = new System.Xml.XmlDocument();

            var projectEl = this.CreateRootProject(document);
            var visualCMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            projectEl.SetAttribute("ToolsVersion", visualCMeta.VCXProjFiltersToolsVersion);

            var filtersEl = document.CreateVSItemGroup(parentEl: projectEl);

            foreach (var filter in this.Filters)
            {
                var filterEl = document.CreateVSElement("Filter", parentEl: filtersEl);
                filterEl.SetAttribute("Include", filter.Key);
                document.CreateVSElement("UniqueIdentifier", new DeterministicGuid("VSFilter" + filter.Key).Guid.ToString("B").ToUpper(), parentEl: filterEl);

                var filesEl = document.CreateVSItemGroup(parentEl: projectEl);
                var extensions = new Bam.Core.StringArray();
                foreach (var setting in filter.Value)
                {
                    var path = setting.Path;
                    var extension = System.IO.Path.GetExtension(path.ToString()).TrimStart(new[] { '.' });
                    extensions.AddUnique(extension);
                    setting.Serialize(document, filesEl);
                }
                if (extensions.Count > 0)
                {
                    document.CreateVSElement("Extensions", extensions.ToString(';'), parentEl: filterEl);
                }
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
