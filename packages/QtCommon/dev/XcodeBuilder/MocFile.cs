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

            Opus.Core.BaseOptionCollection complementOptionCollection = null;
            if (node.EncapsulatingNode.Module is Opus.Core.ICommonOptionCollection)
            {
                var commonOptions = (node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection).CommonOptionCollection;
                if (commonOptions is QtCommon.MocOptionCollection)
                {
                    complementOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                }
            }

            if (null != parentNode)
            {
                // > 1 because the MocOutputPath is currently in the options, hence never null
                if (complementOptionCollection.OptionNames.Count > 1)
                {
                    // use a custom moc
                    var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.UniqueModuleName, node.ModuleName);

                    ShellScriptHelper.WriteShellCommand(node.Target, moduleToBuild.Options, shellScriptBuildPhase);

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
                throw new Opus.Core.Exception("Single MOC file support in Xcode not yet supported");
            }

            success = true;
            return null;
        }
    }
}
