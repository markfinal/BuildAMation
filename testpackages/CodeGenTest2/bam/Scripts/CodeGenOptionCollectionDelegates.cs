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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=ICodeGenOptions.cs
//     -n=CodeGenTest2
//     -c=CodeGenOptionCollection
//     -p
//     -d
//     -dd=../../../../packages/CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace CodeGenTest2
{
    public partial class CodeGenOptionCollection
    {
        #region ICodeGenOptions Option delegates
        private static void
        OutputSourceDirectoryCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            Bam.Core.ReferenceTypeOption<string> stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        private static void
        OutputNameCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            Bam.Core.ReferenceTypeOption<string> stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLineProcessor);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLineProcessor);
        }
    }
}
