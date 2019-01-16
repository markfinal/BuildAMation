#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace Publisher
{
    public class CollatedCommandLineTool :
        CollatedFile,
        Bam.Core.ICommandLineTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // share the metadata in case it's queried
            this.MetaData = (this as ICollatedObject).SourceModule.Tool.MetaData;
        }

        private Bam.Core.ICommandLineTool
        GetTool() => (this as ICollatedObject).SourceModule as Bam.Core.ICommandLineTool;

        System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> Bam.Core.ICommandLineTool.EnvironmentVariables => this.GetTool().EnvironmentVariables;

        Bam.Core.TokenizedString Bam.Core.ICommandLineTool.Executable => this.GeneratedPaths[CollatedFile.CopiedFileKey]; // use the copied path

        Bam.Core.StringArray Bam.Core.ICommandLineTool.InheritedEnvironmentVariables => this.GetTool().InheritedEnvironmentVariables;

        Bam.Core.TokenizedStringArray Bam.Core.ICommandLineTool.InitialArguments => this.GetTool().InitialArguments;

        Bam.Core.Array<int> Bam.Core.ICommandLineTool.SuccessfulExitCodes => this.GetTool().SuccessfulExitCodes;

        Bam.Core.TokenizedStringArray Bam.Core.ICommandLineTool.TerminatingArguments => this.GetTool().TerminatingArguments;

        string Bam.Core.ICommandLineTool.UseResponseFileOption => this.GetTool().UseResponseFileOption;

        Bam.Core.Settings Bam.Core.ITool.CreateDefaultSettings<T>(T module) => this.GetTool().CreateDefaultSettings(module);
    }
}
