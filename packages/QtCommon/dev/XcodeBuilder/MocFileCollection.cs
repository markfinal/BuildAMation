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

            var tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            var toolExePath = tool.Executable((Opus.Core.BaseTarget)target);

            var commandLineBuilder = new Opus.Core.StringArray();
            commandLineBuilder.Add(toolExePath);
            if (mocOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = mocOptions as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Opus.Core.StringArray();
                excludedOptionNames.Add("MocOutputPath");
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptionNames);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }
            commandLineBuilder.Add("$inputFile");
            commandLineBuilder.Add("-o$outputFile");

            // script for moc'ing all files
            shellScriptBuildPhase.ShellScriptLines.Add("for ((i=0; i < SCRIPT_INPUT_FILE_COUNT ; i++))");
            shellScriptBuildPhase.ShellScriptLines.Add("do");
            shellScriptBuildPhase.ShellScriptLines.Add("inputFile=`eval echo '$SCRIPT_INPUT_FILE_'$i`");
            shellScriptBuildPhase.ShellScriptLines.Add("outputFile=`eval echo '$SCRIPT_OUTPUT_FILE_'$i`");
            shellScriptBuildPhase.ShellScriptLines.Add("echo \\\"Moc'ing $inputFile\\\"");
            shellScriptBuildPhase.ShellScriptLines.Add(commandLineBuilder.ToString());
            shellScriptBuildPhase.ShellScriptLines.Add("done");

            success = true;
            return null;
        }
    }
}
