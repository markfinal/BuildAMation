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
            var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOC files", moduleToBuild.OwningNode.ModuleName);
            // cannot add to the nativeTarget's build phases, so delay this til later

            // script for moc'ing all files
            // TODO: path to moc
            // TODO: command line parameters to moc
            shellScriptBuildPhase.ShellScriptLines.Add("for ((i=0; i < SCRIPT_INPUT_FILE_COUNT ; i++))");
            shellScriptBuildPhase.ShellScriptLines.Add("do");
            shellScriptBuildPhase.ShellScriptLines.Add("inputFile=`eval echo '$SCRIPT_INPUT_FILE_'$i`");
            shellScriptBuildPhase.ShellScriptLines.Add("outputFile=`eval echo '$SCRIPT_OUTPUT_FILE_'$i`");
            shellScriptBuildPhase.ShellScriptLines.Add("/Developer/Tools/Qt/moc $inputFile -o $outputFile");
            shellScriptBuildPhase.ShellScriptLines.Add("done");

            // TODO: move this to the MocFile build routine - always add in pairs
            shellScriptBuildPhase.InputPaths.Add("$(PACKAGE_DIR)/source/myobject.h");
            shellScriptBuildPhase.OutputPaths.Add("$(MOC_DIR)/moc_myobject.cpp");

            shellScriptBuildPhase.InputPaths.Add("$(PACKAGE_DIR)/source/myobject2.h");
            shellScriptBuildPhase.OutputPaths.Add("$(MOC_DIR)/moc_myobject2.cpp");

            success = true;
            return null;
        }
    }
}
