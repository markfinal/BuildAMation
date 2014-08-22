#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace XcodeBuilder
{
    public static class ShellScriptHelper
    {
        public static void
        WriteShellCommand(
            Bam.Core.Target target,
            Bam.Core.BaseOptionCollection mocOptions,
            PBXShellScriptBuildPhase shellScriptBuildPhase,
            XCBuildConfiguration configuration)
        {
            var tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            var toolExePath = tool.Executable((Bam.Core.BaseTarget)target);

            var commandLineBuilder = new Bam.Core.StringArray();
            commandLineBuilder.Add(toolExePath);
            if (mocOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = mocOptions as CommandLineProcessor.ICommandLineSupport;
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
