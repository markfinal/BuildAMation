// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        private static void
        CopyNodes(
            Publisher.ProductModule moduleToBuild,
            IProject toProject,
            Opus.Core.DependencyNode toCopy)
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

            // TODO: the key needs to be on an options interface or similiar
            var sourceLoc = toCopy.Module.Locations[C.Application.OutputFile];
            var sourcePath = sourceLoc.GetSingleRawPath();

            var destinationDir = configuration.OutputDirectory;
            var destinationDirPath = destinationDir.GetSingleRawPath();

            var newKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(toCopy.Module, toCopy.Module, C.Application.OutputFile);
            var primaryKey = new Opus.Core.LocationKey(newKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            var destPath = Publisher.ProductModuleUtilities.GenerateDestinationPath(sourcePath, destinationDirPath, moduleToBuild, primaryKey);

            var commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("cmd.exe /c COPY \"{0}\" \"{1}\"{2}", sourcePath, destPath, System.Environment.NewLine);

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

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var primaryNode = Publisher.ProductModuleUtilities.GetPrimaryNode(moduleToBuild);
            var projectData = primaryNode.Data as IProject;

            foreach (var dependency in primaryNode.ExternalDependents)
            {
                var module = dependency.Module;
                var moduleType = module.GetType();
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic;
                var fields = moduleType.GetFields(flags);
                foreach (var field in fields)
                {
                    var candidates = field.GetCustomAttributes(typeof(Publisher.PublishModuleDependencyAttribute), false);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }
                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish module dependency found");
                    }
                    var candidateData = field.GetValue(module) as Opus.Core.Array<Opus.Core.LocationKey>;
                    foreach (var key in candidateData)
                    {
                        CopyNodes(moduleToBuild, projectData, module.OwningNode);
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
