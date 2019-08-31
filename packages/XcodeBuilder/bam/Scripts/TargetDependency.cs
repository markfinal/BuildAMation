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
namespace XcodeBuilder
{
    /// <summary>
    /// Class representing a PBXTargetDependency in the Xcode project
    /// </summary>
    sealed class TargetDependency :
        Object
    {
        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="project">Project to add to</param>
        /// <param name="dependency">Target that is the dependency</param>
        /// <param name="proxy">Proxy object</param>
        public TargetDependency(
            Project project,
            Target dependency,
            ContainerItemProxy proxy)
            :
            base(project, null, "PBXTargetDependency", project.GUID, dependency.GUID, proxy.GUID)
        {
            this.Dependency = dependency;
            this.Proxy = proxy;

            project.AppendTargetDependency(this);
        }

        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="project">Project to add to</param>
        /// <param name="name">Name of the target</param>
        /// <param name="proxy">Proxy object</param>
        public TargetDependency(
            Project project,
            string name,
            ContainerItemProxy proxy)
            :
            base(project, name, "PBXTargetDependency", project.GUID, name, proxy.GUID)
        {
            this.Dependency = null;
            this.Proxy = proxy;

            project.AppendTargetDependency(this);
        }

        /// <summary>
        /// Get the dependency
        /// </summary>
        public Target Dependency { get; private set; }

        /// <summary>
        /// Get the proxy
        /// </summary>
        public ContainerItemProxy Proxy { get; private set; }

        /// <summary>
        /// Serialize the TargetDependency
        /// </summary>
        /// <param name="text">StringBuilder to write to</param>
        /// <param name="indentLevel">Number of tabs to indent by</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            if (null != this.Name)
            {
                text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            }
            else
            {
                text.AppendLine($"{indent}{this.GUID} = {{");
            }
            text.AppendLine($"{indent2}isa = {this.IsA};");
            if (null != this.Dependency)
            {
                text.AppendLine($"{indent2}target = {this.Dependency.GUID} /* {this.Dependency.Name} */;");
            }
            else
            {
                text.AppendLine($"{indent2}name = {this.Name};");
            }
            text.AppendLine($"{indent2}targetProxy = {this.Proxy.GUID} /* {this.Proxy.Name} */;");
            text.AppendLine($"{indent}}};");
        }
    }
}
