#region License
// Copyright (c) 2010-2016, Mark Final
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
    public sealed class TargetDependency :
        Object
    {
        public TargetDependency(
            Project project,
            Target dependency,
            ContainerItemProxy proxy)
        {
            this.IsA = "PBXTargetDependency";
            this.Name = "PBXTargetDependency";
            this.Dependency = dependency;
            this.Proxy = proxy;

            project.TargetDependencies.AddUnique(this);
        }

        public Target Dependency
        {
            get;
            private set;
        }

        public ContainerItemProxy Proxy
        {
            get;
            private set;
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
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
            if (null != this.Dependency)
            {
                text.AppendFormat("{0}target = {1} /* {2} */;", indent2, this.Dependency.GUID, this.Dependency.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0}targetProxy = {1} /* {2} */;", indent2, this.Proxy.GUID, this.Proxy.Name);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }
}
