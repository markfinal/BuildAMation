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
namespace CodeGenTest
{
namespace V2
{
    public sealed class XcodeGenerateSource :
        IGeneratedSourcePolicy
    {
        void
        IGeneratedSourcePolicy.GenerateSource(
            GeneratedSourceModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.Tool compiler,
            Bam.Core.V2.TokenizedString generatedFilePath)
        {
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var command = new System.Text.StringBuilder();
            // recode the executable path for Xcode
            var xcodePath = Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(config)", encapsulating).Parse();
            xcodePath += "/" + System.IO.Path.GetFileName(compiler.Executable.Parse());
            command.AppendFormat(xcodePath);
            // TODO: change this to a configuration directory really
            command.AppendFormat(" {0}", Bam.Core.V2.TokenizedString.Create("$(buildroot)", sender).Parse());
            command.AppendFormat(" {0}", "Generated");

            var commands = new Bam.Core.StringArray();
            commands.Add(command.ToString());
            // TODO: this is not ideal
            // the reason this happens is that this policy is being executed prior to the encapsulating
            // module, and thus, the Xcode meta data doesn't exist for it
            // since I know it's an application, I can do this
            var application = new XcodeBuilder.V2.XcodeProgram(encapsulating, encapsulating.GeneratedPaths[C.V2.ConsoleApplication.Key]);
            application.AddPreBuildCommands(commands);

            var compilerProject = (compiler as GeneratedSourceTool).BuildOfTool.MetaData as XcodeBuilder.V2.XcodeCommonProject;
            application.RequiresProject(compilerProject);
        }
    }
}
}
