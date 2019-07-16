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
namespace XcodeBuilder
{
    /// <summary>
    /// Class representing a PBXGroup in an Xcode project.
    /// </summary>
    public sealed class Group :
        Object
    {
        /// <summary>
        /// Constuct a group of the given name.
        /// </summary>
        /// <param name="project">Project in which to add the Group.</param>
        /// <param name="name">Name of the group.</param>
        public Group(
            Project project,
            string name)
            :
            base(project, name, "PBXGroup")
        {
            this.SourceTree = "<group>";
            this.Children = new Bam.Core.Array<Object>();
        }

        /// <summary>
        /// Construct a group of the given name, in the project for the given Target.
        /// </summary>
        /// <param name="target">Target within the Project to add the group.</param>
        /// <param name="name">Name of the group.</param>
        /// <param name="fullPath">Full path of the group.</param>
        public Group(
            Target target,
            string name,
            Bam.Core.TokenizedString fullPath)
            :
            base(target.Project, name, "PBXGroup", fullPath.ToString())
        {
            this.SourceTree = "<group>";
            this.Children = new Bam.Core.Array<Object>();
        }

        /// <summary>
        /// Construct a group of the given name, containing the child Objects provided.
        /// </summary>
        /// <param name="project">Project that the Group is added to.</param>
        /// <param name="name">Name of the group.</param>
        /// <param name="children">List of child Objects to add to the group.</param>
        public Group(
            Project project,
            string name,
            params Object[] children)
            :
            base(project, name, "PBXGroup", children.ToList().ConvertAll(item => item.GUID).ToArray())
        {
            this.SourceTree = "<group>";
            this.Children = new Bam.Core.Array<Object>();
            foreach (var child in children)
            {
                this.AddChild(child);
            }
        }

        /// <summary>
        /// Get the source tree for the group.
        /// </summary>
        public string SourceTree { get; private set; }

        /// <summary>
        /// Get the children for the group.
        /// </summary>
        public Bam.Core.Array<Object> Children { get; private set; }

        /// <summary>
        /// Get the parent of the group.
        /// </summary>
        public Group Parent { get; private set; }

        /// <summary>
        /// Add a child Object to the group.
        /// </summary>
        /// <param name="other">Object to add.</param>
        public void
        AddChild(
            Object other)
        {
            if (this == other)
            {
                return;
            }
            lock (this.Children)
            {
                var existingRef = this.Children.FirstOrDefault(item => item.GUID.Equals(other.GUID, System.StringComparison.Ordinal));
                if (null == existingRef)
                {
                    this.Children.Add(other);
                    if (other is Group)
                    {
                        var otherGroup = other as Group;
                        if (null != otherGroup.Parent && otherGroup.Parent != this)
                        {
                            throw new Bam.Core.Exception(
                                $"Group '{otherGroup.Name}' is already a child of '{otherGroup.Parent.Name}'. Asking to make it a child of '{this.Name}'"
                            );
                        }
                        otherGroup.Parent = this;
                    }
                }
            }
        }

        /// <summary>
        /// Serialize the group.
        /// </summary>
        /// <param name="text">StringBuilder to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            if (null != this.Name)
            {
                text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            }
            else
            {
                text.AppendLine($"{indent}{this.GUID} = {{");
            }
            text.AppendLine($"{indent2}isa = {this.IsA};");
            if (this.Children.Any())
            {
                text.AppendLine($"{indent2}children = (");
                foreach (var child in this.Children.OrderBy(item => item.Name))
                {
                    text.AppendLine($"{indent3}{child.GUID} /* {this.CleansePaths(child.Name)} */,");
                }
                text.AppendLine($"{indent2});");
            }
            if (null != this.Name)
            {
                text.AppendLine($"{indent2}name = \"{this.Name}\";");
            }
            text.AppendLine($"{indent2}sourceTree = \"{this.SourceTree}\";");
            text.AppendLine($"{indent}}};");
        }
    }
}
