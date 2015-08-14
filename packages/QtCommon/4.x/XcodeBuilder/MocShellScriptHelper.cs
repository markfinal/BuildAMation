#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
