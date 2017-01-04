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
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            XmlUtilities.XmlModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var targetNode = node.ExternalDependents[0];
            var toProject = targetNode.Data as IProject;

            var configCollection = toProject.Configurations;
            var configurationName = configCollection.GetConfigurationNameForTarget((Bam.Core.BaseTarget)targetNode.Target); // TODO: not accurate
            var configuration = configCollection[configurationName];

            var toolName = "VCPreBuildEventTool";
            var vcPreBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPreBuildEventTool)
            {
                vcPreBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPreBuildEventTool);
            }

            var commandLine = new System.Text.StringBuilder();

            var locationMap = moduleToBuild.Locations;
            var outputDir = locationMap[XmlUtilities.XmlModule.OutputDir];
            var outputDirPath = outputDir.GetSingleRawPath();
            commandLine.AppendFormat("IF NOT EXIST {0} MKDIR {0}{1}", outputDirPath, System.Environment.NewLine);

            var outputFileLoc = locationMap[XmlUtilities.XmlModule.OutputFile];
            var outputFilePath = outputFileLoc.GetSingleRawPath();

            var content = XmlUtilities.XmlDocumentToStringBuilder.Write(moduleToBuild.Document);
            foreach (var line in content.ToString().Split('\n'))
            {
                commandLine.AppendFormat("ECHO {0} >> {1}{2}", line, outputFilePath, System.Environment.NewLine);
            }

            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == toProject.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == toProject.VSTarget)
                {
                    attributeName = "Command";
                }

                lock (vcPreBuildEventTool)
                {
                    if (vcPreBuildEventTool.HasAttribute(attributeName))
                    {
                        var currentValue = vcPreBuildEventTool[attributeName];
                        currentValue += commandLine.ToString();
                        vcPreBuildEventTool[attributeName] = currentValue;
                    }
                    else
                    {
                        vcPreBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
