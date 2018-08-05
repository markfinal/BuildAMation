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
namespace C
{
    /// <summary>
    /// Generate source from an external program
    /// </summary>
    public class ExternalSourceGenerator :
        Bam.Core.Module
    {
#if BAM_V2
#else
        private IExternalSourceGeneratorPolicy policy = null;
#endif

        public ExternalSourceGenerator()
        {
            this.Arguments = new Bam.Core.TokenizedStringArray();
            this.InternalInputFiles = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString>();
            this.InternalExpectedOutputFileDictionary = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString>();
        }

        /// <summary>
        /// Specify the path to the external executable to run.
        /// </summary>
        /// <value>The executable.</value>
        public Bam.Core.TokenizedString Executable
        {
            get;
            set;
        }

        /// <summary>
        /// Specify the list of arguments to invoke the executable with.
        /// </summary>
        /// <value>The arguments.</value>
        public Bam.Core.TokenizedStringArray Arguments
        {
            get;
            private set;
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString>
        InternalInputFiles
        {
            get;
            set;
        }

        public System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> InputFiles
        {
            get
            {
                foreach (var input in this.InternalInputFiles)
                {
                    yield return input.Value;
                }
            }
        }

        public void
        AddInputFile(
            string name,
            Bam.Core.TokenizedString path)
        {
            if (this.InternalInputFiles.ContainsKey(name))
            {
                throw new Bam.Core.Exception("Input file '{0}' has already been added", name);
            }
            this.InternalInputFiles.Add(name, path);
            this.Macros.Add(name, path);
        }

        /// <summary>
        /// Gets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public Bam.Core.TokenizedString OutputDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Specify the output directory to write files.
        /// Sets the macro 'ExternalSourceGenerator.OutputDir' to this path.
        /// </summary>
        /// <value>The output directory.</value>
        public void
        SetOutputDirectory(
            Bam.Core.TokenizedString dir_path)
        {
            this.OutputDirectory = dir_path;
            this.Macros.Add("ExternalSourceGenerator.OutputDir", dir_path);
        }

        /// <summary>
        /// Specify the output directory to write files.
        /// Sets the macro 'OutputDir_SourceGenerator' to this path.
        /// </summary>
        /// <value>The output directory.</value>
        public void
        SetOutputDirectory(
            string dir_path)
        {
            this.SetOutputDirectory(this.CreateTokenizedString(dir_path));
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString> InternalExpectedOutputFileDictionary
        {
            get;
            set;
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, Bam.Core.TokenizedString> ExpectedOutputFiles
        {
            get
            {
                return this.InternalExpectedOutputFileDictionary;
            }
        }

        /// <summary>
        /// Adds the expected output file together with a key, that is used as a macro.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="path">Path.</param>
        public void
        AddExpectedOutputFile(
            string name,
            Bam.Core.TokenizedString path)
        {
            if (this.InternalExpectedOutputFileDictionary.ContainsKey(name))
            {
                throw new Bam.Core.Exception("Expected output file with key '{0}' has already been registered", name);
            }
            this.InternalExpectedOutputFileDictionary.Add(name, path);
            this.RegisterGeneratedFile(name, path);
        }

        protected override void EvaluateInternal()
        {
            this.ReasonToExecute = null; // assume it doesn't need updating until you find a reason for it to...

            Bam.Core.TokenizedString newest_output_file_path = null;
            System.DateTime newest_output_file_date = System.DateTime.MinValue;
            foreach (var output in this.InternalExpectedOutputFileDictionary)
            {
                var path = output.Value.ToString();
                if (!System.IO.File.Exists(path))
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(output.Value);
                    return;
                }
                var writeTime = System.IO.File.GetLastWriteTime(path);
                if (writeTime > newest_output_file_date)
                {
                    newest_output_file_path = output.Value;
                    newest_output_file_date = writeTime;
                }
            }

            foreach (var input in this.InternalInputFiles)
            {
                var path = input.Value.ToString();
                var writeTime = System.IO.File.GetLastWriteTime(path);
                if (writeTime > newest_output_file_date)
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(newest_output_file_path, input.Value);
                    return;
                }
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if BAM_V2
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileSupport.GenerateSource(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        if (this.Tool is Bam.Core.ICommandLineTool)
                        {
                            NativeBuilder.Support.RunCommandLineTool(this, context);
                        }
                        else
                        {
                            NativeBuilder.Support.RunArbitraryCommandLineTool(
                                this,
                                context,
                                this.Executable,
                                new Bam.Core.Array<int> { 0 },
                                this.Arguments
                            );
                        }
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    {
                        var args = new Bam.Core.StringArray();
                        args.Add(
                            System.String.Format(
                                "{0} {1}",
                                this.Executable.ToStringQuoteIfNecessary(),
                                this.Arguments.ToString(' ')
                            )
                        );

                        VSSolutionBuilder.Support.AddCustomBuildStep(
                            this,
                            this.InputFiles,
                            this.ExpectedOutputFiles.Values,
                            System.String.Format("Running '{0}'", args.ToString(' ')),
                            args,
                            true,
                            true
                        );
                    }
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.GenerateSource(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
#else
            if (null == this.policy)
            {
                throw new Bam.Core.Exception(
                    "No execution policy for {0} for build mode {1}",
                    this.ToString(),
                    Bam.Core.Graph.Instance.Mode
                );
            }
            if (null == this.Executable)
            {
                throw new Bam.Core.Exception("No executable was specified for {0}", this.ToString());
            }
            this.policy.GenerateSource(
                this,
                context,
                this.Executable,
                this.Arguments,
                this.OutputDirectory,
                this.InternalExpectedOutputFileDictionary,
                this.InternalInputFiles
            );
#endif
        }

#if BAM_V2
#else
        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "Native":
                case "VSSolution":
                case "Xcode":
                case "MakeFile":
                    var className = "C." + mode + "ExternalSourceGenerator";
                    this.policy = Bam.Core.ExecutionPolicyUtilities<IExternalSourceGeneratorPolicy>.Create(className);
                    break;
            }
        }
#endif
    }
}
