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
namespace C
{
    /// <summary>
    /// Utility class offering support for Xcode project generation
    /// </summary>
    static partial class XcodeSupport
    {
        /// <summary>
        /// Add pre or post-build steps to an Xcode target for generating a file from
        /// ICommandLineTool output
        /// </summary>
        /// <param name="module">Module that will run the ICommandLineTool</param>
        /// <param name="outputKey">Pathkey for the output of the Module</param>
        public static void
        GenerateFileFromToolStandardOutput(
            Bam.Core.Module module,
            string outputKey)
        {
            var tool = module.Tool as Bam.Core.ICommandLineTool;
            var toolMeta = (tool as Bam.Core.Module).MetaData;
            var postBuildTarget = true;
            if (null == toolMeta)
            {
                // tool was not buildable
                postBuildTarget = false;
                var encapsulating = module.EncapsulatingModule;
                if (null == encapsulating.MetaData)
                {
                    var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
                    var target = workspace.EnsureTargetExists(encapsulating);
                }
                toolMeta = encapsulating.MetaData;
                if (null == toolMeta)
                {
                    throw new Bam.Core.Exception(
                        $"Unable to determine where to add commands for generating a file from tool standard output, for module {module.ToString()}"
                    );
                }
            }

            if (postBuildTarget)
            {
                XcodeBuilder.Support.AddPostBuildStepForCommandLineTool(
                    module,
                    tool as Bam.Core.Module,
                    out XcodeBuilder.Target target,
                    out XcodeBuilder.Configuration targetConfiguration,
                    redirectToFile: module.GeneratedPaths[outputKey]
                );
            }
            else
            {
                XcodeBuilder.Support.AddPreBuildStepForCommandLineTool(
                    module,
                    out XcodeBuilder.Target target,
                    out XcodeBuilder.Configuration targetConfiguration,
                    true,
                    false,
                    outputPaths: new Bam.Core.TokenizedStringArray { module.GeneratedPaths[outputKey] },
                    redirectToFile: module.GeneratedPaths[outputKey]
                );
            }

            // alias the tool's target so that inter-target dependencies can be set up
            module.MetaData = toolMeta;
        }
    }
}
