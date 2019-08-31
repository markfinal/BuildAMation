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
using System.Linq;
namespace ExternalSourceGeneratorTest2
{
    class PythonSourceGenerator :
        C.ExternalSourceGenerator
    {
        protected override void
        Init()
        {
            base.Init();

            this.Executable = Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("python").FirstOrDefault());

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.OutputDirectory);
                    }
                });
        }
    }

    sealed class SpecificPythonGenerator :
        PythonSourceGenerator
    {
        protected override void
        Init()
        {
            base.Init();

            this.AddInputFile("PyScript", this.CreateTokenizedString("$(packagedir)/generators/class.py"));
            this.AddInputFile("SpecFile", this.CreateTokenizedString("$(packagedir)/specs/generated_class.xml"));

            this.SetOutputDirectory("$(packagebuilddir)/$(config)");
            this.AddExpectedOutputFile("header", this.CreateTokenizedString("$(0)/generated_class.h", new[] { this.OutputDirectory }));
            this.AddExpectedOutputFile("source", this.CreateTokenizedString("$(0)/generated_class.cpp", new[] { this.OutputDirectory }));

            this.Arguments.Add(this.CreateTokenizedString("$(PyScript)"));
            this.Arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("--specfile"));
            this.Arguments.Add(this.CreateTokenizedString("$(SpecFile)"));
            this.Arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("--output-header"));
            this.Arguments.Add(this.ExpectedOutputFiles["header"]);
            this.Arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("--output-source"));
            this.Arguments.Add(this.ExpectedOutputFiles["source"]);
        }
    }

    sealed class TestApp :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var python_generator = Bam.Core.Graph.Instance.FindReferencedModule<SpecificPythonGenerator>();

            var source = this.CreateCxxSourceCollection("$(packagedir)/source/main.cpp");
            source.DependsOn(python_generator);
            source.UsePublicPatches(python_generator);
            source.AddFile(python_generator.ExpectedOutputFiles["source"]);

            var headers = this.CreateHeaderCollection();
            headers.AddFile(python_generator.ExpectedOutputFiles["header"]);
        }
    }
}
