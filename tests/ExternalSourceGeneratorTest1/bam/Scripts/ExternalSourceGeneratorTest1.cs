#region License
// Copyright (c) 2010-2018, Mark Final
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
using Bam.Core;
using System.Linq;
namespace ExternalSourceGeneratorTest1
{
    public class ExternalSourceGenerator :
        Bam.Core.Module
    {
        public ExternalSourceGenerator()
        {
            this.Arguments = new Bam.Core.TokenizedStringArray();
            this.ExpectedOutputFiles = new Bam.Core.TokenizedStringArray();
        }

        public Bam.Core.TokenizedString Executable
        {
            get;
            set;
        }

        public Bam.Core.TokenizedStringArray Arguments
        {
            get;
            private set;
        }

        public Bam.Core.TokenizedString OutputDirectory
        {
            get;
            set;
        }

        public Bam.Core.TokenizedStringArray ExpectedOutputFiles
        {
            get;
            private set;
        }

        public override void Evaluate()
        {
            //throw new System.NotImplementedException();
        }

        protected override void ExecuteInternal(ExecutionContext context)
        {
            if (null == this.Executable)
            {
                throw new Bam.Core.Exception("No executable was specified");
            }
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(this.OutputDirectory.ToString());
            var program = this.Executable.ToStringQuoteIfNecessary();
            var arguments = this.Arguments.ToString(' ');
            Bam.Core.Log.Info("Running: {0} {1}", program, arguments);
            Bam.Core.OSUtilities.RunExecutable(program, arguments);
            foreach (var expected_file in this.ExpectedOutputFiles)
            {
                if (!System.IO.File.Exists(expected_file.ToString()))
                {
                    throw new Bam.Core.Exception("Expected '{0}' to exist, but it does not", expected_file.ToString());
                }
            }
        }

        protected override void GetExecutionPolicy(string mode)
        {
            //throw new System.NotImplementedException();
        }
    }

    public class PythonSourceGenerator :
        ExternalSourceGenerator
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Executable = Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("python").FirstOrDefault());

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.OutputDirectory);
                    }
                });
        }
    }

    public sealed class TestApp :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var python_generator = Bam.Core.Module.Create<PythonSourceGenerator>();
            python_generator.OutputDirectory = this.CreateTokenizedString("$(packagebuilddir)/$(config)");
            python_generator.ExpectedOutputFiles.Add(this.CreateTokenizedString("$(0)/generated_class.h", new[] { python_generator.OutputDirectory }));
            python_generator.ExpectedOutputFiles.Add(this.CreateTokenizedString("$(0)/generated_class.cpp", new[] { python_generator.OutputDirectory }));
            python_generator.Arguments.Add(this.CreateTokenizedString("$(packagedir)/generators/class.py"));
            python_generator.Arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("--specfile"));
            python_generator.Arguments.Add(this.CreateTokenizedString("$(packagedir)/specs/generated_class.xml"));
            python_generator.Arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("--output-header"));
            python_generator.Arguments.Add(python_generator.ExpectedOutputFiles[0]);
            python_generator.Arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("--output-source"));
            python_generator.Arguments.Add(python_generator.ExpectedOutputFiles[1]);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/main.cpp");
            source.DependsOn(python_generator);
            source.UsePublicPatches(python_generator);
            source.AddFile(python_generator.ExpectedOutputFiles[1]);

            var headers = this.CreateHeaderContainer();
            headers.AddFile(python_generator.ExpectedOutputFiles[0]);
        }
    }
}
