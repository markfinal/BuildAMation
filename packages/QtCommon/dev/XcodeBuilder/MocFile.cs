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

            if (null != parentNode)
            {
                var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOC files", parentNode.ModuleName);
                shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
            }
            else
            {
                throw new Opus.Core.Exception("Single MOC file support in Xcode not yet supported");
            }

            success = true;
            return null;
        }
    }
}
