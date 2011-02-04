// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, Opus.Core.DependencyNode node, out bool success)
        {
            if (null == node.Parent || (node.Parent.Module.GetType().BaseType.BaseType == typeof(C.ObjectFileCollection) && null == node.Parent.Parent))
            {
                // utility project
                success = true;
                return null;
            }

            Opus.Core.Target target = node.Target;
            string configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectData projectData = this.solutionFile.ProjectDictionary[node.ModuleName];
            ProjectConfiguration configuration = projectData.Configurations[configurationName];

            string toolName = "VCCLCompilerTool";
            ProjectTool vcCLCompilerTool = configuration.GetTool(toolName);
            if (null == vcCLCompilerTool)
            {
                vcCLCompilerTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLCompilerTool);

                if (objectFileCollection.Options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = objectFileCollection.Options as VisualStudioProcessor.IVisualStudioSupport;
                    VisualStudioProcessor.ToolAttributeDictionary settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (System.Collections.Generic.KeyValuePair<string, string> setting in settingsDictionary)
                    {
                        vcCLCompilerTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Opus.Core.Exception("Compiler options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}