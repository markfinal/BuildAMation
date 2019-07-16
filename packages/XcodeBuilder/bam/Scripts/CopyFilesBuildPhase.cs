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
    /// Class corresponding to a PBXCopyFilesBuildPhase in the Xcode project.
    /// </summary>
    public sealed class CopyFilesBuildPhase :
        BuildPhase
    {
        /// <summary>
        /// Which sub folder is specified for the copy operation.
        /// </summary>
        public enum ESubFolderSpec
        {
            AbsolutePath = 0,           //!< An absolute path
            Wrapper = 1,                //!< A wrapper (?)
            Executables = 6,            //!< Relative to executables
            Resources = 7,              //!< Relative to resources
            Frameworks = 10,            //!< Relative to frameworks
            SharedFrameworks = 11,      //!< Relative to shared frameworks
            SharedSupport = 12,         //!< Relative to shared support
            Plugins = 13,               //!< Relative to plugins
            JavaResources = 15,         //!< Relative to Java resources
            ProductsDirectory = 16      //!< Relative to the products directory
        }

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="name">Name of the build phase.</param>
        /// <param name="target">Target to add to.</param>
        public CopyFilesBuildPhase(
            string name,
            Target target)
            :
            base(target.Project, name, "PBXCopyFilesBuildPhase", target.GUID)
        {
            this.DestinationPath = string.Empty;
            this.SubFolderSpec = ESubFolderSpec.AbsolutePath;
        }

        /// <summary>
        /// Get the build action mask.
        /// </summary>
        protected override string BuildActionMask => "2147483647";

        /// <summary>
        /// Get whether the build phase only runs for deployment preprocessing.
        /// </summary>
        protected override bool RunOnlyForDeploymentPostprocessing => false;

        /// <summary>
        /// Get the destination path.
        /// </summary>
        public string DestinationPath { get; private set; }

        /// <summary>
        /// Get the sub-folder for the destination.
        /// </summary>
        public ESubFolderSpec SubFolderSpec { get; set; }

        /// <summary>
        /// Serialize the CopyFilesBuildPhase.
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
            text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}buildActionMask = {this.BuildActionMask};");
            text.AppendLine($"{indent2}dstPath = \"{this.DestinationPath}\";");
            text.AppendLine($"{indent2}dstSubfolderSpec = \"{(int)this.SubFolderSpec}\";");
            if (this.BuildFiles.Any())
            {
                text.AppendLine($"{indent2}files = (");
                foreach (var file in this.BuildFiles)
                {
                    text.AppendLine($"{indent3}{file.GUID} /* in {this.Name} */,");
                }
                text.AppendLine($"{indent2});");
            }
            var runOnly = this.RunOnlyForDeploymentPostprocessing ? "1" : "0";
            text.AppendLine($"{indent2}runOnlyForDeploymentPostprocessing = {runOnly};");
            text.AppendLine($"{indent}}};");
        }
    }
}
