#region License
// Copyright (c) 2010-2016, Mark Final
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
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            XmlUtilities.TextFileModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var targetNode = node.ExternalDependents[0];
            var project = this.Workspace.GetProject(targetNode);
            var baseTarget = (Bam.Core.BaseTarget)targetNode.Target;
            var configuration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);

            var outputFileLoc = moduleToBuild.Locations[XmlUtilities.TextFileModule.OutputFile];
            var outputFilePath = outputFileLoc.GetSingleRawPath();

            var moduleName = node.ModuleName;
            var fileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.Text, outputFilePath, project.RootUri);
            var sourcesBuildPhase = project.SourceBuildPhases.Get("MiscSources", moduleName);
            var data = project.BuildFiles.Get(moduleName, fileRef, sourcesBuildPhase);
            if (null == data)
            {
                throw new Bam.Core.Exception("Build file not available");
            }

            var content = moduleToBuild.Content.ToString();
            string shellScriptName = "Writing text file for " + targetNode.UniqueModuleName;
            var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get(shellScriptName, node.ModuleName);
            shellScriptBuildPhase.OutputPaths.Add(outputFilePath);
            shellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("if [ \\\"${{CONFIGURATION}}\\\" = \\\"{0}\\\" ]; then", configuration.Name));
            foreach (var line in content.Split('\n'))
            {
                var escapedLine = line.Replace("\"", "\\\"");
                shellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("echo \\\"{0}\\\" >> $SCRIPT_OUTPUT_FILE_0", escapedLine));
            }
            shellScriptBuildPhase.ShellScriptLines.Add("fi");

            // is this a post action?
            {
                // because this is performed AFTER the application, we can directly add to the build phases
                var nativeTarget = targetNode.Data as PBXNativeTarget;
                nativeTarget.BuildPhases.Insert(0, shellScriptBuildPhase);
            }

            success = true;
            return data;
        }
    }
}
