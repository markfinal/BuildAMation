// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        private static void CopyNodes(IProject toProject, Opus.Core.DependencyNode toCopy)
        {
            var configCollection = toProject.Configurations;
            // TODO: this should be done from the base target!
            var configurationName = configCollection.GetConfigurationNameForTarget(toCopy.Target); // TODO: not accurate
            var configuration = configCollection[configurationName];

            var toolName = "VCPostBuildEventTool";
            var vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            var sourceLoc = toCopy.Module.Locations[C.Application.OutputFile];
            var sourcePath = sourceLoc.GetSingleRawPath();
            var destinationDir = configuration.OutputDirectory;

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("IF NOT EXIST \"{0}\" MKDIR \"{0}\"{1}", destinationDir, System.Environment.NewLine);
            commandLine.AppendFormat("cmd.exe /c COPY \"{0}\" \"{1}\"{2}", sourcePath, destinationDir, System.Environment.NewLine);

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

                lock (vcPostBuildEventTool)
                {
                    if (vcPostBuildEventTool.HasAttribute(attributeName))
                    {
                        var currentValue = vcPostBuildEventTool[attributeName];
                        currentValue += commandLine.ToString();
                        vcPostBuildEventTool[attributeName] = currentValue;
                    }
                    else
                    {
                        vcPostBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                    }
                }
            }
        }

        public object Build(Publisher.ProductModule moduleToBuild, out bool success)
        {
            var primaryNode = Publisher.ProductModuleUtilities.GetPrimaryNode(moduleToBuild);
            var projectData = primaryNode.Data as IProject;
            CopyNodes(projectData, primaryNode);

            success = true;
            return null;
        }
    }
}
