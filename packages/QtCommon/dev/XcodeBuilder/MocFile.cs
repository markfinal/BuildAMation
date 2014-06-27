// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public partial class XcodeBuilder
    {
        public object Build(QtCommon.MocFile moduleToBuild, out System.Boolean success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var baseTarget = (Opus.Core.BaseTarget)node.Target;

            var parentNode = node.Parent;
            Opus.Core.DependencyNode targetNode;
            if (parentNode.Module is Opus.Core.IModuleCollection)
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
            }

            var project = this.Workspace.GetProject(targetNode);
            var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);
            buildConfiguration.Options["MOC_DIR"].AddUnique(moduleToBuild.Locations[QtCommon.MocFile.OutputDir].GetSingleRawPath());
            buildConfiguration.Options["PACKAGE_DIR"].AddUnique(targetNode.Module.PackageLocation.GetSingleRawPath());
            success = true;
            return null;
        }
    }
}
