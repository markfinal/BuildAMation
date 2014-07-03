// <copyright file="ShellScriptHelper.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public static class ShellScriptHelper
    {
        public static void
        WriteShellCommand(
            Opus.Core.Target target,
            Opus.Core.BaseOptionCollection mocOptions,
            PBXShellScriptBuildPhase shellScriptBuildPhase)
        {
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
        }
    }
}
