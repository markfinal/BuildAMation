#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            QtCommon.MocFileCollection moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var target = node.Target;
            var mocOptions = moduleToBuild.Options as QtCommon.MocOptionCollection;

            var parentNode = node.Parent;
            Bam.Core.DependencyNode targetNode;
            if ((null != parentNode) && (parentNode.Module is Bam.Core.IModuleCollection))
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
                targetNode = targetNode.EncapsulatingNode;
            }

            var project = this.Workspace.GetProject(targetNode);
            var baseTarget = (Bam.Core.BaseTarget)targetNode.Target;
            var configuration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);

            var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.ModuleName, moduleToBuild.OwningNode.ModuleName);
            // cannot add to the nativeTarget's build phases, so delay this til later

            MocShellScriptHelper.WriteShellCommand(target, mocOptions, shellScriptBuildPhase, configuration);

            success = true;
            return null;
        }
    }
}
