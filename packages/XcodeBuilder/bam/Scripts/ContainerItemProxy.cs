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
    /// Class corresponding to a PBXContainerItemProxy in an Xcode project.
    /// </summary>
    public sealed class ContainerItemProxy :
        Object
    {
        private ContainerItemProxy(
            Project project,
            Object portal,
            Object remote)
            :
            base(project, null, "PBXContainerItemProxy", project.GUID, portal.GUID, remote.GUID)
        {
            this.ContainerPortal = portal;
            this.RemoteName = null;
            project.AppendContainerItemProxy(this);
        }

        /// <summary>
        /// Construct an instance, used for Targets in a different Project to that in which the Target exists.
        /// </summary>
        /// <param name="project">Project to add the ContainerItemProxy to.</param>
        /// <param name="portal">The container portal</param>
        /// <param name="reference">The Target that is referenced.</param>
        public ContainerItemProxy(
            Project project,
            Object portal,
            Target reference)
            :
            this(project, portal, reference as Object) // 'as' is to disambiguate the constructors
        {
            this.Remote = reference;
            this.ProxyType = 1;
        }

        /// <summary>
        /// Construct an instance, used for Targets in the same Project.
        /// </summary>
        /// <param name="projectAndPortal">Project to add the ContainerItemProxy to.</param>
        /// <param name="reference">The Target that is referenced.</param>
        public ContainerItemProxy(
            Project projectAndPortal,
            Target reference)
            :
            this(projectAndPortal, projectAndPortal, reference)
        {
            this.Remote = reference;
            this.ProxyType = 2;
        }

        /// <summary>
        /// Construct an instance, for a FileReference in a different Project.
        /// </summary>
        /// <param name="project">Project to add the ContainerItemProxy to</param>
        /// <param name="portal">The container portal</param>
        /// <param name="reference">The FileReference to add a reference to.</param>
        /// <param name="refName">The name for which to refer to the remote.</param>
        public ContainerItemProxy(
            Project project,
            Object portal,
            FileReference reference,
            string refName)
            :
            this(project, portal, reference)
        {
            this.Remote = reference;
            this.ProxyType = 2;
            this.RemoteName = refName;
        }

        /// <summary>
        /// Get the container portal.
        /// </summary>
        public Object ContainerPortal { get; private set; }
        private int ProxyType { get; set; }
        /// <summary>
        /// Get the remote.
        /// </summary>
        public Object Remote { get; private set; }
        private string RemoteName { get; set; }

        /// <summary>
        /// Serialize the ContainerItemProxy.
        /// </summary>
        /// <param name="text">The StringBuilder to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
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
            text.AppendLine($"{indent2}containerPortal = {this.ContainerPortal.GUID} /* {this.ContainerPortal.Name} */;");
            text.AppendLine($"{indent2}proxyType = {this.ProxyType};");
            text.AppendLine($"{indent2}remoteGlobalIDString = {this.Remote.GUID};");
            var remoteInfo = this.RemoteName ?? this.Remote.Name;
            text.AppendLine($"{indent2}remoteInfo = {remoteInfo};");
            text.AppendLine($"{indent}}};");
        }
    }
}
