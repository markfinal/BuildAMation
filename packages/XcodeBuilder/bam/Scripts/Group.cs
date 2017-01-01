#region License
// Copyright (c) 2010-2017, Mark Final
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
    public sealed class Group :
        Object
    {
        public Group(
            Project project,
            string name)
            :
            base(project, name, "PBXGroup")
        {
            this.SourceTree = "<group>";
            this.Children = new System.Collections.Generic.List<Object>();
        }

        public Group(
            Target target,
            string name,
            Bam.Core.TokenizedString fullPath)
            :
            base(target.Project, name, "PBXGroup", fullPath.Parse())
        {
            this.SourceTree = "<group>";
            this.Children = new System.Collections.Generic.List<Object>();
        }

        public Group(
            Project project,
            string name,
            params Object[] children)
            :
            base(project, name, "PBXGroup", children.ToList().ConvertAll(item => item.GUID).ToArray())
        {
            this.SourceTree = "<group>";
            this.Children = new System.Collections.Generic.List<Object>();
            foreach (var child in children)
            {
                this.AddChild(child);
            }
        }

        public string SourceTree
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<Object> Children
        {
            get;
            private set;
        }

        public Group Parent
        {
            get;
            private set;
        }

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
                var existingRef = this.Children.FirstOrDefault(item => item.GUID == other.GUID);
                if (null == existingRef)
                {
                    this.Children.Add(other);
                    if (other is Group)
                    {
                        var otherGroup = other as Group;
                        if (null != otherGroup.Parent && otherGroup.Parent != this)
                        {
                            throw new Bam.Core.Exception("Group '{0}' is already a child of '{1}'. Asking to make it a child of '{2}'",
                                otherGroup.Name, otherGroup.Parent.Name, this.Name);
                        }
                        otherGroup.Parent = this;
                    }
                }
            }
        }

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
                text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            }
            else
            {
                text.AppendFormat("{0}{1} = {{", indent, this.GUID);
            }
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            if (this.Children.Count > 0)
            {
                text.AppendFormat("{0}children = (", indent2);
                text.AppendLine();
                foreach (var child in this.Children.OrderBy(item => item.Name))
                {
                    text.AppendFormat("{0}{1} /* {2} */,", indent3, child.GUID, child.Name);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            if (null != this.Name)
            {
                text.AppendFormat("{0}name = \"{1}\";", indent2, this.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0}sourceTree = \"{1}\";", indent2, this.SourceTree);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }
}
