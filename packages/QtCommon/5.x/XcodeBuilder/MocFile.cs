#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace XcodeBuilder
{
    public partial class XcodeBuilder
    {
        public object
        Build(
            QtCommon.MocFile moduleToBuild,
            out System.Boolean success)
        {
            var node = moduleToBuild.OwningNode;

            var parentNode = node.Parent;
            Bam.Core.DependencyNode targetNode;
            if ((parentNode != null) && (parentNode.Module is Bam.Core.IModuleCollection))
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
            }

            var project = this.Workspace.GetProject(targetNode);
            var baseTarget = (Bam.Core.BaseTarget)targetNode.Target;
            var configuration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);

            if (null != parentNode)
            {
                Bam.Core.BaseOptionCollection complementOptionCollection = null;
                if (node.EncapsulatingNode.Module is Bam.Core.ICommonOptionCollection)
                {
                    var commonOptions = (node.EncapsulatingNode.Module as Bam.Core.ICommonOptionCollection).CommonOptionCollection;
                    if (commonOptions is QtCommon.MocOptionCollection)
                    {
                        complementOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                    }
                }

                if ((complementOptionCollection != null) && (complementOptionCollection.OptionNames.Count > 0))
                {
                    // use a custom moc
                    var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.UniqueModuleName, node.ModuleName);

                    MocShellScriptHelper.WriteShellCommand(node.Target, moduleToBuild.Options, shellScriptBuildPhase, configuration);

                    shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                    shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
                }
                else
                {
                    var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + parentNode.ModuleName, parentNode.ModuleName);
                    shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                    shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
                }
            }
            else
            {
                var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.UniqueModuleName, node.ModuleName);

                MocShellScriptHelper.WriteShellCommand(node.Target, moduleToBuild.Options, shellScriptBuildPhase, configuration);

                shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
            }

            success = true;
            return null;
        }
    }
}
