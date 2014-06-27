// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(QtCommon.MocFileCollection moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var target = node.Target;
            var mocOptions = moduleToBuild.Options as QtCommon.MocOptionCollection;

            var parentNode = node.Parent;
            Opus.Core.DependencyNode targetNode;
            if ((null != parentNode) && (parentNode.Module is Opus.Core.IModuleCollection))
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
                targetNode = targetNode.EncapsulatingNode;
            }

            var project = this.Workspace.GetProject(targetNode);
            var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.ModuleName, moduleToBuild.OwningNode.ModuleName);
            // cannot add to the nativeTarget's build phases, so delay this til later

            ShellScriptHelper.WriteShellCommand(target, mocOptions, shellScriptBuildPhase);

            success = true;
            return null;
        }
    }
}
