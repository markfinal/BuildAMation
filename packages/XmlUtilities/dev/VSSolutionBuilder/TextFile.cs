#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            XmlUtilities.TextFileModule moduleToBuild,
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
            var outputDir = locationMap[XmlUtilities.TextFileModule.OutputDir];
            var outputDirPath = outputDir.GetSingleRawPath();
            commandLine.AppendFormat("IF NOT EXIST {0} MKDIR {0}{1}", outputDirPath, System.Environment.NewLine);

            var outputFileLoc = locationMap[XmlUtilities.TextFileModule.OutputFile];
            var outputFilePath = outputFileLoc.GetSingleRawPath();

            foreach (var line in moduleToBuild.Content.ToString().Split('\n'))
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
