#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace XcodeBuilder
{
    public static class MocShellScriptHelper
    {
        public static void
        WriteShellCommand(
            Bam.Core.Target target,
            Bam.Core.BaseOptionCollection options,
            PBXShellScriptBuildPhase shellScriptBuildPhase,
            XCBuildConfiguration configuration)
        {
            var tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            var toolExePath = tool.Executable((Bam.Core.BaseTarget)target);

            var commandLineBuilder = new Bam.Core.StringArray();
            commandLineBuilder.Add(toolExePath);
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }
            commandLineBuilder.Add("-o $outputFile");
            commandLineBuilder.Add("$inputFile");

            // script for moc'ing all files
            shellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("if [ \\\"${{CONFIGURATION}}\\\" = \\\"{0}\\\" ]; then", configuration.Name));
            shellScriptBuildPhase.ShellScriptLines.Add("for ((i=0; i < SCRIPT_INPUT_FILE_COUNT ; i++))");
            shellScriptBuildPhase.ShellScriptLines.Add("do");
            shellScriptBuildPhase.ShellScriptLines.Add("inputFile=`eval echo '$SCRIPT_INPUT_FILE_'$i`");
            shellScriptBuildPhase.ShellScriptLines.Add("outputFile=`eval echo '$SCRIPT_OUTPUT_FILE_'$i`");
            shellScriptBuildPhase.ShellScriptLines.Add("echo \\\"Moc'ing $inputFile\\\"");
            shellScriptBuildPhase.ShellScriptLines.Add(commandLineBuilder.ToString());
            shellScriptBuildPhase.ShellScriptLines.Add("done");
            shellScriptBuildPhase.ShellScriptLines.Add("fi");
        }
    }
}
