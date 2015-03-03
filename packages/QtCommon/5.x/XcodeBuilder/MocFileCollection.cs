#region License
// Copyright 2010-2015 Mark Final
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
