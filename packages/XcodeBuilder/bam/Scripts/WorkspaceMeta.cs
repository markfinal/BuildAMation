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
namespace XcodeBuilder
{
    public sealed class WorkspaceMeta :
        System.Collections.Generic.IEnumerable<Project>
    {
        public WorkspaceMeta()
        {
            this.Projects = new System.Collections.Generic.Dictionary<Bam.Core.PackageDefinition, Project>();
        }

        private System.Collections.Generic.Dictionary<Bam.Core.PackageDefinition, Project> Projects
        {
            get;
            set;
        }

        public Project
        FindOrCreateProject(
            Bam.Core.Module module,
            XcodeMeta.Type projectType)
        {
            lock(this)
            {
                // Note: if you want a Xcode project per module, change this from keying off of the package
                // to the module type
                var package = module.PackageDefinition;
                if (this.Projects.ContainsKey(package))
                {
                    var project = this.Projects[package];
                    project.AddNewProjectConfiguration(module);
                    return project;
                }
                else
                {
                    var project = new Project(module, package.Name);
                    this.Projects[package] = project;
                    return project;
                }
            }
        }

        public System.Collections.Generic.IEnumerator<Project> GetEnumerator()
        {
            foreach (var project in this.Projects)
            {
                yield return project.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
